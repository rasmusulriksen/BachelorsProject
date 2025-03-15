CREATE OR REPLACE FUNCTION queues.mark_element_as_done(
	queue_name text,
	notification_guid uuid,
	processing_result_text text,
	calling_processor_id text)
    RETURNS void
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE
AS $BODY$
DECLARE
    element_exists boolean;
    assigned_to_processor boolean;
BEGIN
    -- Check if element exists
    EXECUTE format('SELECT EXISTS(SELECT 1 FROM %s WHERE notification_guid = $1)', queue_name)
    INTO element_exists
    USING notification_guid;
    
    IF NOT element_exists THEN
        RAISE EXCEPTION USING
            message = 'No element with the given guid exists',
            detail  = 'Notification_guid was: ' || notification_guid;
    END IF;
    
    -- Check if element is assigned to the calling processor
    EXECUTE format('SELECT EXISTS(SELECT 1 FROM %s WHERE notification_guid = $1 AND processor_id = $2)', queue_name)
    INTO assigned_to_processor
    USING notification_guid, calling_processor_id;
    
    IF NOT assigned_to_processor THEN
        RAISE EXCEPTION USING
            message = 'The element with the given guid is not assigned to the calling processor',
            detail  = 'GUID was: ' || notification_guid || ', processor_id was: ' || calling_processor_id;
    END IF;
    
    -- Update the element status
    EXECUTE format('
        UPDATE %s
        SET processing_status = ''done'',
            result = $1
        WHERE notification_guid = $2 AND processor_id = $3;
    ', queue_name)
    USING processing_result_text, notification_guid, calling_processor_id;
END;
$BODY$;

ALTER FUNCTION queues.mark_element_as_done(queue_name text, notification_guid uuid, processing_result_text text, calling_processor_id text)
    OWNER TO admin;
