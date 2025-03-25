-- SQL script to create notification schema and tables

-- Create notification schema
CREATE SCHEMA IF NOT EXISTS notification;

-- Create notification_preferences table
CREATE TABLE IF NOT EXISTS notification.notification_preferences (
    username VARCHAR(255) PRIMARY KEY,  -- Primary key to identify the user
    case_owner BOOLEAN NOT NULL DEFAULT FALSE,
    email_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    in_app_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    links_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Seed a test user into the notification_preferences table
INSERT INTO notification.notification_preferences (username, case_owner, email_enabled, in_app_enabled, links_enabled)
VALUES ('rasmus.ulriksen', true, true, true, true);

-- Create notification table
CREATE TABLE IF NOT EXISTS notification.notification (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_user_id VARCHAR(100) NOT NULL,
    feed_user_id VARCHAR(100) NOT NULL,
    activity_type VARCHAR(255) NOT NULL,
    activity_summary TEXT NOT NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    post_date BIGINT NOT NULL
);