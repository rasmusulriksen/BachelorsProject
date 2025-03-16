CREATE OR REPLACE FUNCTION queues.take_elements_for_processing(
	queue_name text,
	calling_processor_id text,
	num_elements integer)
    RETURNS TABLE(message json, notification_guid uuid) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
    ROWS 1000

AS $BODY$
DECLARE
    query text;
BEGIN
    IF num_elements < 1 THEN
        RAISE EXCEPTION USING
            message = 'Input num_elements must be greater than 0',
            detail  = 'Input was: ' || num_elements;
    END IF;

    -- Dynamically construct the UPDATE query
    query := format('
        UPDATE %s AS free
        SET processing_status = ''processing'',
            processor_id = $1
        WHERE free.notification_guid IN (
            SELECT q.notification_guid
            FROM %s AS q
            WHERE q.processing_status = ''waiting''
            ORDER BY q.notification_guid
            LIMIT %s
            FOR UPDATE SKIP LOCKED
        )
        RETURNING free.message, free.notification_guid
    ', queue_name, queue_name, num_elements);

    -- Return the query results
    RETURN QUERY EXECUTE query USING calling_processor_id;
END;
$BODY$;

ALTER FUNCTION queues.take_elements_for_processing(queue_name text, calling_processor_id text, num_elements integer)
    OWNER TO admin;
