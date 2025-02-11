# ER Diagrams

## EmailTemplateDB
```mermaid
erDiagram

    EmailTemplates {
        UUID templateId PK
        UUID tenantId
        VARCHAR(255) name
        VARCHAR(255) subject
        TEXT bodyHtml
        TEXT bodyText
        TIMESTAMPTZ createdDate
        TIMESTAMPTZ modifiedDate
    }

    EmailTemplateMetadata {
        UUID metadataId PK
        UUID templateId FK
        VARCHAR(255) key
        TEXT value
    }

    EmailTemplatePlaceholders {
        UUID placeholderId PK
        UUID templateId FK
        VARCHAR(255) placeholderName
        VARCHAR(50) placeholderDataType
    }

    EmailTemplates ||--o{ EmailTemplateMetadata: "has"
    EmailTemplates ||--o{ EmailTemplatePlaceholders: "has"
```

## NotificationOrchestratorWorkerDB
```mermaid
erDiagram

    NotificationsToOrchestrate {
        UUID notificationId PK
        UUID userId
        UUID templateId FK
        UUID documentRef
        BOOLEAN isInstant
        TIMESTAMPTZ queuedAt
        JSONB templateParams
    }

    NotificationsToSend {
        UUID notificationId PK
        UUID userId FK
        TIMESTAMPTZ scheduledSendTime
    }

    NotificationsToOrchestrate ||--o{ NotificationsToSend: "summarizes"
```

## MessageQueueDB
```mermaid
erDiagram

    NotificationsToBeOrchestrated {
        UUID notificationId PK
        UUID userId
        UUID templateId
        JSONB templateParams
        TIMESTAMPTZ createdAt
        TEXT status      // Status of the message, e.g., 'NEW', 'PROCESSED'
    }

    NotificationsToBeSent {
        UUID notificationId PK
        UUID userId
        UUID templateId
        JSONB templateParams
        TIMESTAMPTZ scheduledSendTime
        TIMESTAMPTZ createdAt
    }

    NotificationsPostponed {
        UUID notificationId PK
        UUID userId
        UUID templateId
        JSONB templateParams
        TIMESTAMPTZ postponementPeriod
        TEXT aggregatedDetails  // Potentially hold info about summaries
        TIMESTAMPTZ createdAt
    }

    NotificationsToBeOrchestrated ||--o{ NotificationsToBeSent: "triggers"    
    NotificationsToBeOrchestrated ||--o{ NotificationsPostponed: "results in"
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