-- Add notification_guid column to all queue tables

-- Adding to notifications table
ALTER TABLE queues.notifications
ADD COLUMN IF NOT EXISTS notification_guid UUID DEFAULT gen_random_uuid() NOT NULL;

-- Create index for faster lookup
CREATE INDEX IF NOT EXISTS idx_notifications_guid 
ON queues.notifications(notification_guid);

-- Adding to emails_to_be_merged_into_template table
ALTER TABLE queues.emails_to_be_merged_into_template
ADD COLUMN IF NOT EXISTS notification_guid UUID DEFAULT gen_random_uuid() NOT NULL;

-- Create index for faster lookup
CREATE INDEX IF NOT EXISTS idx_emails_to_be_merged_into_template_guid 
ON queues.emails_to_be_merged_into_template(notification_guid);

-- Adding to emails_to_be_sent table
ALTER TABLE queues.emails_to_be_sent
ADD COLUMN IF NOT EXISTS notification_guid UUID DEFAULT gen_random_uuid() NOT NULL;

-- Create index for faster lookup
CREATE INDEX IF NOT EXISTS idx_emails_to_be_sent_guid 
ON queues.emails_to_be_sent(notification_guid); 