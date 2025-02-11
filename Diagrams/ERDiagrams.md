# ER Diagrams

## EmailTemplateDB
- Schema per tenant
- The current implementation of custom email templates has several properties that I don't fully understand:
    - category, language, active
```mermaid
erDiagram

    system_email_templates {
        UUID templateId PK

    }

    custom_email_templates {
        UUID templateId PK
        VARCHAR(255) name
        VARCHAR(255) subject
        TEXT bodyHtml
        TEXT bodyText
        TIMESTAMPTZ createdDate
        TIMESTAMPTZ modifiedDate
        TEXT lastModifiedBy
    }

    email_templates_metadata {
        UUID metadataId PK
        UUID templateId FK
        VARCHAR(255) key
        TEXT value
    }

    email_templates_placeholders {
        UUID placeholderId PK
        UUID templateId FK
        VARCHAR(255) placeholderName
        VARCHAR(50) placeholderDataType
    }

    custom_email_templates ||--o{ email_templates_metadata: "has"
    custom_email_templates ||--o{ email_templates_placeholders: "has"
```

## NotificationOrchestratorWorkerDB
- Every email stored in this table will be sent as part of a batch job, where the user will receive an aggregated notification summary (i.e. every weekday at 08:00 AM).
- We don't need to store a templateId, because the same template will be used for these emails always.

```mermaid
erDiagram

    postponed_notifications {
        UUID notificationId
        UUID userId
        TEXT actionName
        JSONB inputParameters
        TIMESTAMPTZ scheduledSendTime
    }
```

## MessageQueueDB
```mermaid
erDiagram

    notifications_to_be_orchestrated {
        UUID notificationId PK
        VARCHAR(255) recipientEmail
        TEXT message
    }

    notifications_to_be_sent {
        UUID notificationId PK
        VARCHAR(255) recipientEmail
        TEXT message
    }

    notifications_postponed {
        UUID notificationId PK
        VARCHAR(255) recipientEmail
        TEXT message
        TIMESTAMPTZ was_postponed_at
        TIMESTAMPTZ will_be_sent_at
    }

    notifications_to_be_orchestrated ||--o{ notifications_to_be_sent: "can be labeled as"    
    notifications_to_be_orchestrated ||--o{ notifications_postponed: "can be labeled as"
```

## NotificationSettingsDB
```mermaid
erDiagram

    UserPreferences {
        UUID userId PK
        VARCHAR(100) notificationType PK
        VARCHAR(50) frequency
    }
```