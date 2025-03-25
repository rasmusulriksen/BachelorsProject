CREATE TABLE notification.notification (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_user_id VARCHAR(100) NOT NULL,
    feed_user_id VARCHAR(100) NOT NULL,
    activity_type VARCHAR(255) NOT NULL,
    activity_summary TEXT NOT NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    post_date BIGINT NOT NULL
);

CREATE INDEX ix_notifications_feed_user_id ON notification.notification(feed_user_id);
CREATE INDEX ix_notifications_post_date ON notification.notification(post_date DESC); 