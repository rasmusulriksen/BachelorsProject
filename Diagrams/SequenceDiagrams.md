# SequenceDiagrams

## SequenceDiagram1: User uploads a document to a case (with "Notify" selected) and the notification is passed to the MessageQueueAPI

```mermaid
sequenceDiagram
    participant Client1 as client1.imscase.dk
    participant Frontend
    participant Gateway
    participant Keycloak
    participant TenantAPI
    participant LegacyMonolith
    participant MessageQueueAPI

    Client1->>Frontend: Uploads document to case (with "Notify" selected)
    Frontend->>Gateway: POST api/upload
    Gateway->>Keycloak: Authenticate user
    Keycloak-->>Gateway: Returns JWT
    Gateway->>TenantAPI: Get tenant info
    TenantAPI-->>Gateway: Returns TenantInfo
    Gateway->>LegacyMonolith: POST api/upload
    LegacyMonolith->>LegacyMonolith: Upload document to case
    LegacyMonolith->>MessageQueueAPI: POST api/notify
    MessageQueueAPI->>MessageQueueAPI: INSERT INTO TABLE new_notifications
    MessageQueueAPI-->>LegacyMonolith: Returns 200 OK
    LegacyMonolith-->>Gateway: Returns 200 OK
    Gateway-->>Frontend: Returns 200 OK
    Frontend-->>Client1: Displays success message
```

<br><br><br><br><br><br><br><br>

## SequenceDiagram2: The NotificationOrchestratorWorker runs every 10 seconds, retrieves the new notifications from the MessageQueueAPI and decides between SendNotificationNow() and PostponeNotification()

```mermaid
sequenceDiagram
    participant MessageQueueAPI
    participant NotificationOrchestratorWorker
    participant NotificationSettingsAPI
    participant EmailTemplateAPI

    NotificationOrchestratorWorker->>MessageQueueAPI: GET api/notifications/new (every 10 seconds)
    MessageQueueAPI-->>NotificationOrchestratorWorker: Returns List<Notification>
    loop foreach notification in List<Notification>
        NotificationOrchestratorWorker->>NotificationSettingsAPI: GET api/notificationpreferences/{user}
        NotificationSettingsAPI-->>NotificationOrchestratorWorker: Returns notificationPreferences
        alt SendNotificationNow()
            NotificationOrchestratorWorker->>EmailTemplateAPI: GET api/emailtemplate/{notificationName}
            EmailTemplateAPI-->>NotificationOrchestratorWorker: Returns EmailTemplate (JSON)
            NotificationOrchestratorWorker->>NotificationOrchestratorWorker: PopulateEmailTemplateWithDynamicData()
            NotificationOrchestratorWorker->>MessageQueueAPI: POST api/notify
        else PostponeNotification()
            NotificationOrchestratorWorker->>NotificationOrchestratorWorker: notification.DueTime = notificationPreferences.BulkReceivalTimePreference
            NotificationOrchestratorWorker->>NotificationOrchestratorWorker: INSERT INTO TABLE postponed_notifications(notification)
        end 
    end
```

<br><br><br><br><br><br><br><br>

## SequenceDiagram3: The EmailSenderWorker runs every 10 seconds, retrieves the ready notifications from the MessageQueue and sends them to the SMTP-server

```mermaid
sequenceDiagram
    participant MessageQueueAPI
    participant EmailSenderWorker
    participant SMTPServer

    EmailSenderWorker->>MessageQueueAPI: GET api/notifications/ready (every 10 seconds)
    MessageQueueAPI-->>EmailSenderWorker: Returns List<Notification>
    EmailSenderWorker->>SMTPServer: Connects to SMTP-server and sends the email
```

<br><br><br><br><br><br><br><br>

## SequenceDiagram4: The postponed notifications are fetched, batched into 1-email-per-user, published to MessageQueueAPI and sent via EmailSenderWorker to the SMTP-server (happens 08:00, 12:00 and 16:00)

```mermaid
sequenceDiagram
    participant NotificationOrchestratorWorker
    participant EmailTemplateAPI
    participant MessageQueueAPI
    participant EmailSenderWorker
    participant SMTPServer

    NotificationOrchestratorWorker->>NotificationOrchestratorWorker: SELECT * FROM postponed_messages WHERE due_time IS NOW (at 08:00, 12:00, 16:00)
    NotificationOrchestratorWorker->>EmailTemplateAPI: GET api/emailtemplate/summary
    EmailTemplateAPI-->>NotificationOrchestratorWorker: Returns SummaryEmailTemplate (JSON)
    NotificationOrchestratorWorker->>NotificationOrchestratorWorker: List<User> users = FindAllUsersInPostponedNotifications()
    loop foreach user in users
        NotificationOrchestratorWorker->>NotificationOrchestratorWorker: MergeAllNotificationsIntoOneEmailSummary()
        NotificationOrchestratorWorker->>NotificationOrchestratorWorker: PopulateEmailTemplateWithDynamicData()
        NotificationOrchestratorWorker->>MessageQueueAPI: POST api/notify
    end
    EmailSenderWorker->>MessageQueueAPI: GET api/notifications/ready (every 10 seconds)
    MessageQueueAPI-->>EmailSenderWorker: Returns List<Notification>
    EmailSenderWorker->>SMTPServer: Connects to SMTP-server and sends the email
```