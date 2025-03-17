CREATE TABLE IF NOT EXISTS notifications.notifications
(
    id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    message json NOT NULL,
    processor_id character varying(64) COLLATE pg_catalog."default",
    processed_at timestamp with time zone,
    processing_status queues.queue_processing_status NOT NULL DEFAULT 'waiting'::queues.queue_processing_status,
    result text COLLATE pg_catalog."default",
    username text NOT NULL,
    CONSTRAINT notifications_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE notifications.notifications
    OWNER to admin;
