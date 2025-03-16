CREATE OR REPLACE FUNCTION queues.insert_into_queue(
	queue_name text,
	message json,
	notification_guid uuid)
    RETURNS uuid
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
AS $BODY$
        DECLARE
            inserted_guid UUID;
            query text;
            schema_name text;
            table_name text;
        BEGIN
            -- Split the queue_name into schema and table parts
            schema_name := split_part(queue_name, '.', 1);
            table_name := split_part(queue_name, '.', 2);
            
            -- Dynamically construct the INSERT query
            query := format('
                INSERT INTO %I.%I (message, notification_guid) 
                VALUES ($1, $2)
                RETURNING notification_guid', schema_name, table_name);
            
            -- Execute the query and get the inserted GUID
            EXECUTE query INTO inserted_guid USING message, notification_guid;

            RETURN inserted_guid;
        END;
        $BODY$;

ALTER FUNCTION queues.insert_into_queue(queue_name text, message json, notification_guid uuid)
    OWNER TO admin;
