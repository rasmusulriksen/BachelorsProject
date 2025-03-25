-- PART 1: CREATE FUNCTIONS TO CREATE QUEUES

CREATE SCHEMA IF NOT EXISTS queues;

-- Create the enum type for queue processing status if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'queue_processing_status') THEN
        CREATE TYPE queues.queue_processing_status AS ENUM ('waiting', 'processing', 'done', 'failed');
    END IF;
END $$;

-- Create the function to set the processed_utc column to the current UTC timestamp
CREATE OR REPLACE FUNCTION queues.set_processed_at() RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.processed_at := CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$;

-- Create the procedure to create queue tables
CREATE OR REPLACE PROCEDURE queues.create_queue_table(queue_name TEXT)
LANGUAGE plpgsql
AS $$
BEGIN
    EXECUTE format('
        CREATE TABLE queues.%I (
            id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            message JSON NOT NULL,
            processor_id VARCHAR(64),
            processed_at TIMESTAMP WITH TIME ZONE,
            processing_status queues.queue_processing_status NOT NULL DEFAULT ''waiting'',
            result TEXT
        )', queue_name);

    -- Create an index on processing_status
    EXECUTE format('
        CREATE INDEX idx_%I_processing_status ON queues.%I (processing_status)', queue_name, queue_name);

    -- Create a trigger to set the default value of processed_utc to the current UTC timestamp
    EXECUTE format('
        CREATE TRIGGER set_processed_at_trigger_%I
        BEFORE INSERT OR UPDATE ON queues.%I
        FOR EACH ROW
        EXECUTE FUNCTION queues.set_processed_at();', queue_name, queue_name);

    --Create roles for inserter, remover, and discarder without login
    EXECUTE format('
        CREATE ROLE @TenantIdentifier_queue_%I_inserter NOLOGIN;', queue_name);

    EXECUTE format('
        CREATE ROLE @TenantIdentifier_queue_%I_processor NOLOGIN;', queue_name);

    EXECUTE format('
        CREATE ROLE @TenantIdentifier_queue_%I_discarder NOLOGIN;', queue_name);

    -- Create function to insert elements into the queue
    EXECUTE format('
        CREATE FUNCTION queues.%I_insert_into_queue(message JSON)
        RETURNS BIGINT
        LANGUAGE plpgsql
        AS $func$
        DECLARE
            inserted_id BIGINT;
        BEGIN
            INSERT INTO queues.%I (message) 
            VALUES (message)
            RETURNING id INTO inserted_id;

            RETURN inserted_id;
        END;
        $func$;', queue_name, queue_name);

    -- Grant execute permission on the function to the inserter role
    EXECUTE format('
        GRANT EXECUTE ON FUNCTION queues.%I_insert_into_queue(json) TO @TenantIdentifier_queue_%I_inserter;', queue_name, queue_name);

    EXECUTE format('
        GRANT SELECT, INSERT ON TABLE queues.%I TO @TenantIdentifier_queue_%I_inserter;', queue_name, queue_name);

    --
    -- Create function to get specific element in the queue
    --
    EXECUTE format('
        CREATE FUNCTION queues.%I_get_element_status(element_id BIGINT)
        RETURNS TABLE(id BIGINT, processor_id VARCHAR(64), processed_at TIMESTAMP WITH TIME ZONE, processing_status queues.queue_processing_status, result TEXT)
        LANGUAGE plpgsql
        AS $func$
        BEGIN
            RETURN QUERY
            SELECT q.id, q.processor_id, q.processed_at, q.processing_status, q.result
            FROM queues.%I AS q
            WHERE q.id = element_id;
        END;
        $func$;', queue_name, queue_name);

    -- Grant execute permission on the function to the inserter role
    EXECUTE format('
        GRANT EXECUTE ON FUNCTION queues.%I_get_element_status(bigint) TO @TenantIdentifier_queue_%I_inserter;', queue_name, queue_name);

    --
    -- Create function to take elements for processing from the queue
    --
    EXECUTE format('
        CREATE FUNCTION queues.%I_take_elements_for_processing(calling_processor_id TEXT, num_elements INT)
        RETURNS TABLE(id BIGINT, message JSON)
        LANGUAGE plpgsql
        AS $func$
        BEGIN
            IF num_elements < 1  THEN
                RAISE EXCEPTION USING
                    message = ''Input num_elements must be greater than 0'',
                    detail  = ''Input was: '' || num_elements;
            END IF;

            RETURN QUERY
            UPDATE queues.%I AS free
            SET processing_status = ''processing'',
                processor_id = calling_processor_id
            WHERE free.id IN (
                SELECT q.id
                FROM queues.%I AS q
                WHERE q.processing_status = ''waiting''
                ORDER BY q.id
                LIMIT num_elements
                FOR UPDATE SKIP LOCKED
            )
            RETURNING free.id, free.message;
        END;
        $func$;', queue_name, queue_name, queue_name);

    --Grant execute permission on the function to the processor role
    EXECUTE format('
        GRANT EXECUTE ON FUNCTION queues.%I_take_elements_for_processing(text, int) TO @TenantIdentifier_queue_%I_processor;', queue_name, queue_name);

    EXECUTE format('
        GRANT SELECT, UPDATE ON TABLE queues.%I TO @TenantIdentifier_queue_%I_processor;', queue_name, queue_name);

    --
    -- Create function to mark multiple elements as done in the queue
    --
    EXECUTE format('
        CREATE FUNCTION queues.%I_mark_element_as_done(element_id BIGINT, processing_result_text TEXT, calling_processor_id TEXT)
        RETURNS VOID
        LANGUAGE plpgsql
        AS $func$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM queues.%I WHERE id = element_id)  THEN
                RAISE EXCEPTION USING
                    message = ''No element with the given id exists'',
                    detail  = ''Id was: '' || element_id;
            END IF;

            IF NOT EXISTS (SELECT 1 FROM queues.%I WHERE id = element_id AND processor_id = calling_processor_id)  THEN
                RAISE EXCEPTION USING
                    message = ''The element with the given id is not assigned to the calling processor'',
                    detail  = ''Id was: '' || element_id || '', processor_id was: '' || calling_processor_id;
            END IF;

            UPDATE queues.%I
            SET processing_status = ''done'',
                result = processing_result_text
            WHERE id = element_id AND processor_id = calling_processor_id;
        END;
        $func$;', queue_name, queue_name, queue_name, queue_name);

    -- Grant execute permission on the function to the processor role
    EXECUTE format('
        GRANT EXECUTE ON FUNCTION queues.%I_mark_element_as_done(bigint, text, text) TO @TenantIdentifier_queue_%I_processor;', queue_name, queue_name);

    --
    -- Create function to mark multiple elements as failed in the queue
    --
    EXECUTE format('
        CREATE FUNCTION queues.%I_mark_element_as_failed(element_id BIGINT, processing_result_text TEXT, calling_processor_id TEXT)
        RETURNS VOID
        LANGUAGE plpgsql
        AS $func$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM queues.%I WHERE id = element_id)  THEN
                RAISE EXCEPTION USING
                    message = ''No element with the given id exists'',
                    detail  = ''Id was: '' || element_id;
            END IF;

            IF NOT EXISTS (SELECT 1 FROM queues.%I WHERE id = element_id AND processor_id = calling_processor_id)  THEN
                RAISE EXCEPTION USING
                    message = ''The element with the given id is not assigned to the calling processor'',
                    detail  = ''Id was: '' || element_id || '', processor_id was: '' || calling_processor_id;
            END IF;

            UPDATE queues.%I
            SET processing_status = ''failed'',
                result = processing_result_text
            WHERE id = element_id AND processor_id = calling_processor_id;
        END;
        $func$;', queue_name, queue_name, queue_name, queue_name);

    -- Grant execute permission on the function to the processor role
    EXECUTE format('
        GRANT EXECUTE ON FUNCTION queues.%I_mark_element_as_failed(bigint, text, text) TO @TenantIdentifier_queue_%I_processor;', queue_name, queue_name);

    --
    -- Create function to delete elements in the queue older than a certain time in days that are done
    -- 
    EXECUTE format('
        CREATE FUNCTION queues.%I_delete_old_done_elements(days_old INT)
        RETURNS VOID
        LANGUAGE plpgsql
        AS $func$
        BEGIN
            DELETE FROM queues.%I
            WHERE processing_status = ''done'' AND processed_at < CURRENT_TIMESTAMP - INTERVAL ''1 day'' * days_old;
        END;
        $func$;', queue_name, queue_name);
    
    -- Grant execute permission on the function to the discarder role
    EXECUTE format('
        GRANT EXECUTE ON FUNCTION queues.%I_delete_old_done_elements(int) TO @TenantIdentifier_queue_%I_discarder;', queue_name, queue_name);

    EXECUTE format('
        GRANT SELECT, DELETE ON TABLE queues.%I TO @TenantIdentifier_queue_%I_discarder;', queue_name, queue_name);

    --
    -- Create function toto delete element in the queue older than a certain time in days that are failed
    --
    EXECUTE format('
        CREATE FUNCTION queues.%I_delete_old_failed_elements(days_old INT)
        RETURNS VOID
        LANGUAGE plpgsql
        AS $func$
        BEGIN
            DELETE FROM queues.%I
            WHERE processing_status = ''failed'' AND processed_at < CURRENT_TIMESTAMP - INTERVAL ''1 day'' * days_old;
        END;
        $func$;', queue_name, queue_name);

    -- Grant execute permission on the function to the discarder role
    EXECUTE format('
        GRANT EXECUTE ON FUNCTION queues.%I_delete_old_failed_elements(int) TO @TenantIdentifier_queue_%I_discarder;', queue_name, queue_name);

END;
$$;




-- PART 2: CREATE STORED PROCEDURE TO DROP QUEUES

CREATE OR REPLACE PROCEDURE queues.drop_queue_table(queue_name TEXT, tenant_identifier TEXT)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Drop functions
    EXECUTE format('DROP FUNCTION IF EXISTS queues.%I_insert_into_queue(json);', queue_name);
    EXECUTE format('DROP FUNCTION IF EXISTS queues.%I_get_element_status(bigint);', queue_name);
    EXECUTE format('DROP FUNCTION IF EXISTS queues.%I_take_elements_for_processing(text, int);', queue_name);
    EXECUTE format('DROP FUNCTION IF EXISTS queues.%I_mark_elements_as_done(bigint[], text);', queue_name);
    EXECUTE format('DROP FUNCTION IF EXISTS queues.%I_mark_elements_as_failed(bigint[], text);', queue_name);
    EXECUTE format('DROP FUNCTION IF EXISTS queues.%I_delete_old_done_elements(int);', queue_name);
    EXECUTE format('DROP FUNCTION IF EXISTS queues.%I_delete_old_failed_elements(int);', queue_name);

    -- Drop trigger
    EXECUTE format('DROP TRIGGER IF EXISTS set_processed_at_trigger_%I ON queues.%I;', queue_name, queue_name);

    -- Drop index
    EXECUTE format('DROP INDEX IF EXISTS idx_%I_processing_status;', queue_name);

    -- Drop table
    EXECUTE format('DROP TABLE IF EXISTS queues.%I;', queue_name);

    -- Drop roles
    EXECUTE format('DROP ROLE IF EXISTS %I_queue_%I_inserter;', tenant_identifier, queue_name);
    EXECUTE format('DROP ROLE IF EXISTS %I_queue_%I_processor;', tenant_identifier, queue_name);
    EXECUTE format('DROP ROLE IF EXISTS %I_queue_%I_discarder;', tenant_identifier, queue_name);
END;
$$;



-- PART 3: CREATE 3 QUEUES

CALL queues.create_queue_table('unprocessed_notifications');
CALL queues.create_queue_table('emails_to_be_merged_into_template');
CALL queues.create_queue_table('emails_to_be_sent');