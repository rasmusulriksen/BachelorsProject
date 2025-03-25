-- Create the notification_preferences table
CREATE TABLE notification.notification_preferences (
    username VARCHAR(255) PRIMARY KEY,  -- Primary key to identify the user
    case_owner BOOLEAN NOT NULL DEFAULT FALSE,
    email_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    in_app_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    links_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
