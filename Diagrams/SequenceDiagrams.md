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
    LegacyMonolith->>LegacyMonolith: usersToNotify = documentNotificationService.getUsersFromNotifyField(docRef)
    loop foreach user in usersToNotify
        LegacyMonolith->>MessageQueueAPI: POST api/publish?queueName=notifications_to_be_orchestrated
        MessageQueueAPI-->>LegacyMonolith: Returns 200 OK
    end
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
        NotificationOrchestratorWorker->>NotificationOrchestratorWorker: SendNowOrPostpone()
        alt SendNow()
            NotificationOrchestratorWorker->>EmailTemplateAPI: POST api/PopulateEmailTemplate <br> Body: PopulateEmailTemplateDTO
            EmailTemplateAPI-->>NotificationOrchestratorWorker: Returns IActionResult<EmailTemplateReadyToSend> (JSON/string with html content)
            NotificationOrchestratorWorker->>MessageQueueAPI: publish?queueName=notifications_to_be_sent
        else Postpone()
            NotificationOrchestratorWorker->>NotificationOrchestratorWorker: notification.DueTime = notificationPreferences.WhenToReceiveNotifications
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

## SequenceDiagram4: Users receive in-app and email notifications at 08:00, 12:00 and 16:00

```mermaid
sequenceDiagram
    participant NotificationOrchestratorWorker
    participant EmailTemplateAPI
    participant MessageQueueAPI
    participant LegacyMonolith
    participant EmailSenderWorker
    participant SMTPServer

    NotificationOrchestratorWorker->>NotificationOrchestratorWorker: List<Notification> notifications = SELECT * FROM postponed_messages WHERE due_time IS NOW (at 08:00, 12:00, 16:00)
    NotificationOrchestratorWorker->>LegacyMonolith: POST api/openesdh/activities <br> Request body: List<Notification>
    loop foreach activity in activities
        LegacyMonolith->>LegacyMonolith: oeActivityService.postActivity(activity)
    end
    LegacyMonolith-->>NotificationOrchestratorWorker: Returns 200 OK
    NotificationOrchestratorWorker->>EmailTemplateAPI: GET api/emailtemplate/notificationssummary
    EmailTemplateAPI-->>NotificationOrchestratorWorker: Returns notifications summary email template (JSON)
    NotificationOrchestratorWorker->>NotificationOrchestratorWorker: List<User> users = FindAllUsersInPostponedNotifications()
    loop foreach user in users
        NotificationOrchestratorWorker->>NotificationOrchestratorWorker: MergeAllNotificationsForUserIntoOneEmailSummary()
        NotificationOrchestratorWorker->>NotificationOrchestratorWorker: PopulateEmailTemplateWithDynamicData()
        NotificationOrchestratorWorker->>MessageQueueAPI: POST api/publish?queueName=notifications_to_be_sent
    end
    EmailSenderWorker->>MessageQueueAPI: GET api/dequeue?queueName=notifications_to_be_sent (every 10 seconds)
    MessageQueueAPI-->>EmailSenderWorker: Returns List<Notification>
    EmailSenderWorker->>SMTPServer: Connects to SMTP-server and sends the email
```