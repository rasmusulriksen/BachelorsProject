## MessageQueueAPI

```mermaid
sequenceDiagram
    participant NotificationAPI
    participant MessageQueueAPI
    participant client1db as {clientId}.queues.notification

    NotificationAPI->>MessageQueueAPI: POST /messagequeue/publish/{eventName} <br> Header: X-Tenant-Identifier: clientId <br> Body: message
    MessageQueueAPI->>MessageQueueAPI: connectionStringFactory.CreateConnectionString(clientId)
    MessageQueueAPI->>MessageQueueAPI: queueName = GetDbTableForEventName(eventName)
    MessageQueueAPI->>MessageQueueAPI: connectionInfo = BuildDbConnection(connectionString)
    MessageQueueAPI->>MessageQueueAPI: queueInserter = new QueueInserter(queueName, connectionInfo)
    MessageQueueAPI->>client1db: queueInserter.Insert(message)
    client1db-->>MessageQueueAPI: Return inserted message ID
    MessageQueueAPI-->>NotificationAPI: Return success status and message ID
```

<br>
<br>
<br>
<br>
<br>

## NotificationAPI

### NotificationAPI.BackgroundService

```mermaid
sequenceDiagram

    participant NotificationAPI.BackgroundService as NotificationAPI.BackgroundService
    participant MessageQueueAPI

    loop for each tenant in tenants
        NotificationAPI.BackgroundService->>MessageQueueAPI: GET /messagequeue/poll <br> Header: X-Tenant-Identifier: tenant.TenantIdentifier
        MessageQueueAPI-->>NotificationAPI.BackgroundService: Return notifications from {tenant.TenantIdentifier}.unprocessed_notifications

        loop for each notification in notifications
            NotificationAPI.BackgroundService->>NotificationAPI.BackgroundService: Read recipient's preferences
            NotificationAPI.BackgroundService->>MessageQueueAPI: POST /messagequeue/publish/EmailTemplateShouldBePopulated <br> Header: X-Tenant-Identifier: tenant.TenantIdentifier <br> Body: emailNotification
            MessageQueueAPI-->>NotificationAPI.BackgroundService: Return OK
        end
    end
```

### NotificationAPI.API

```mermaid
sequenceDiagram
    participant ClientNMonolith as Monolith
    participant NotificationAPI.API
    participant db as Database
    participant MessageQueueAPI

    ClientNMonolith->>NotificationAPI.API: POST /notification <br> Header: X-Tenant-Identifier: clientId <br> Body: notification
    NotificationAPI.API->>db: Insert notification into {clientId}.notification.notification
    db-->>NotificationAPI.API: Return OK
    NotificationAPI.API->>MessageQueueAPI: POST /messagequeue/publish/NotificationInitialized <br> Header: X-Tenant-Identifier: clientId <br> Body: notification
    MessageQueueAPI-->>NotificationAPI.API: Return OK
    NotificationAPI.API-->>ClientNMonolith: Return OK
```

<br>
<br>
<br>
<br>
<br>

## EmailTemplateAPI

### EmailTemplateAPI.BackgroundService

```mermaid
sequenceDiagram
    participant EmailTemplateAPI.BackgroundService as EmailTemplateAPI.BackgroundService
    participant MessageQueueAPI

    loop for each tenant in tenants
        EmailTemplateAPI.BackgroundService->>MessageQueueAPI: GET /messagequeue/poll <br> Header: X-Tenant-Identifier: tenant.TenantIdentifier
        MessageQueueAPI-->>EmailTemplateAPI.BackgroundService: Return email data from {tenant.TenantIdentifier}.queues.emails_to_be_populated

        loop for each email
            EmailTemplateAPI.BackgroundService->>EmailTemplateAPI.BackgroundService: Get Handlebars.NET template from event name
            EmailTemplateAPI.BackgroundService->>EmailTemplateAPI.BackgroundService: Merge data into template
            EmailTemplateAPI.BackgroundService->>MessageQueueAPI: POST /messagequeue/publish/EmailTemplateHasBeenPopulated <br> Header: X-Tenant-Identifier: tenant.TenantIdentifier <br> Body: outboundEmail
            MessageQueueAPI-->>EmailTemplateAPI.BackgroundService: Return success status
        end
    end
```

### EmailTemplateAPI.API

```mermaid
sequenceDiagram
    participant ClientNMonolith as Monolith
    participant EmailTemplateService as EmailTemplateAPI.API
    participant db as Database

    ClientNMonolith->>EmailTemplateService: GET /api/EmailTemplates <br> (Open list of all email templates) <br> Header: X-Tenant-Identifier: clientId
    EmailTemplateService->>db: select * from {clientId}.email_templates
    db-->>EmailTemplateService: Return all templates
    EmailTemplateService-->>ClientNMonolith: Return all templates

    ClientNMonolith->>EmailTemplateService: GET /api/EmailTemplates/{id} <br> (Open specific email template) <br> Header: X-Tenant-Identifier: clientId
    EmailTemplateService->>db: select * from {clientId}.email_templates where id = {id}
    db-->>EmailTemplateService: Return template
    EmailTemplateService-->>ClientNMonolith: Return template

    ClientNMonolith->>EmailTemplateService: PUT /api/EmailTemplates <br> (Update email template) <br> Header: X-Tenant-Identifier: clientId <br> Body: EmailTemplate
    EmailTemplateService->>db: update {clientId}.email_templates set {EmailTemplate} where id = {id}
    db-->>EmailTemplateService: Confirm update
    EmailTemplateService-->>ClientNMonolith: Return updated template
```

<br>
<br>
<br>
<br>
<br>

## EmailSenderAPI

### EmailSenderAPI.BackgroundService

```mermaid
sequenceDiagram
    participant EmailSenderAPI.BackgroundService as EmailSenderAPI.BackgroundService
    participant MessageQueueAPI
    participant TenantAPI
    participant SMTPServer

    loop for each tenant in tenants
        EmailSenderAPI.BackgroundService->>MessageQueueAPI: GET /messagequeue/poll <br> Header: X-Tenant-Identifier: tenant.TenantIdentifier
        MessageQueueAPI-->>EmailSenderAPI.BackgroundService: Return emails from {tenant.TenantIdentifier}.queues.emails_to_be_sent

        loop for each email
            EmailSenderAPI.BackgroundService->>TenantAPI: GET /api/smtp-connection <br> Header: X-Tenant-Identifier: tenant.TenantIdentifier
            TenantAPI-->>EmailSenderAPI.BackgroundService: Return SMTP connection
            EmailSenderAPI.BackgroundService->>SMTPServer: Send email
            SMTPServer-->>EmailSenderAPI.BackgroundService: Return OK
        end
    end
```

<br>
<br>
<br>
<br>
<br>

## TenantAPI

### TenantAPI.OnboardTenant

```mermaid
sequenceDiagram
    participant AdminControlPanel
    participant TenantAPI
    participant TenantDb as tenant_database.tenants
    participant PostgresServer as Database Server

    AdminControlPanel->>TenantAPI: POST /tenantapi/onboard <br> Body: OnboardTenantRequest
    
    TenantAPI->>TenantAPI: Generate secure password
    
    TenantAPI->>TenantDb: Create tenant record
    TenantDb-->>TenantAPI: Return OK
    
    TenantAPI->>PostgresServer: CREATE DATABASE {tenantIdentifier}
    PostgresServer-->>TenantAPI: Return OK
    
    TenantAPI->>PostgresServer: Create schema "notification" <br> Create tables "notification_preferences" and "notification"
    PostgresServer-->>TenantAPI: Return OK
    
    TenantAPI->>PostgresServer: Create schema "queues" <br> Create queue tables and functions
    PostgresServer-->>TenantAPI: Return OK
    
    TenantAPI->>PostgresServer: CREATE USER "{tenantIdentifier}_user" <br> GRANT permissions
    PostgresServer-->>TenantAPI: Return OK
    
    TenantAPI-->>AdminControlPanel: Return OK
```

## Udfordringer

### Setting up LISTEN/NOTIFY connection with NotificationAPI and database

```mermaid
sequenceDiagram
    participant Client as Database Client
    participant DB as PostgreSQL Database
    participant Trigger as Database Trigger
    participant Tx as Transaction

    Note over Client, DB: Setting up the notification system

    Client->>DB: Connect to database
    DB-->>Client: Connection established
    
    Client->>DB: LISTEN channel_name
    DB-->>Client: Listening on channel_name
    
    Client->>DB: Create trigger function notify_event()
    DB-->>Client: Function created
    
    Client->>DB: Create trigger on table for INSERT events
    DB-->>Client: Trigger created
    
    Note over Client, DB: System is now set up. Client maintains open connection.
    
    Note over DB: Later, when data is inserted
    
    Tx->>DB: BEGIN TRANSACTION
    Tx->>DB: INSERT INTO some_table VALUES (...)
    DB->>Trigger: Trigger activated
    Trigger->>DB: PERFORM pg_notify('channel_name', payload)
    DB-->>Trigger: Notification queued
    Tx->>DB: COMMIT
    
    DB-->>Client: Asynchronous notification: channel_name, payload
    Client->>Client: Process notification in event handler
    
    Note over Client, DB: Connection remains open for future notifications
```

### PostgreSQL 17 LISTEN/NOTIFY solution

```mermaid
sequenceDiagram
    participant NotificationAPI
    participant MessageQueueAPI
    participant Database

    Note over NotificationAPI: On application start:
    NotificationAPI->>Database: LISTEN to table X.queues.notifications

    Note over NotificationAPI: Begin publishing message:
    NotificationAPI->>MessageQueueAPI: Publish message to tenant X
    MessageQueueAPI->>Database: Insert message to X.queues.notifications
    Database-->>NotificationAPI: NOTIFY "new message in tenant X"
    NotificationAPI->>NotificationAPI: Update tenantsWithNewMessages

    Note over NotificationAPI: Every 10 seconds:
    
    loop for each tenant in tenantsWithNewMessages
        NotificationAPI->>MessageQueueAPI: Poll tenant
        MessageQueueAPI->>Database: Get messages
        Database-->>MessageQueueAPI: Return messages
        MessageQueueAPI-->>NotificationAPI: Return messages
        NotificationAPI->>NotificationAPI: Process messages
    end
```

### Custom solution without LISTEN/NOTIFY

```mermaid
sequenceDiagram
    participant NotificationAPI
    participant MessageQueueAPI
    participant CentralRegistry as Central Queue Registry
    participant TenantDBs as Tenant Databases (200)

    MessageQueueAPI->>TenantDBs: Insert message to X.queues.notifications
    MessageQueueAPI->>CentralRegistry: UPDATE queue_registry SET notifications_pending = true WHERE tenant_id = 'X'
    
    Note over NotificationAPI: Every 10 seconds:
    NotificationAPI->>MessageQueueAPI: GET /messagequeue/active-tenants?queue=notifications
    MessageQueueAPI->>CentralRegistry: SELECT tenant_id FROM queue_registry WHERE notifications_pending = true
    CentralRegistry-->>MessageQueueAPI: Return ["X", "Y", "Z"]
    MessageQueueAPI-->>NotificationAPI: Return ["X", "Y", "Z"]
    
    loop for each tenant in ["X", "Y", "Z"]
        NotificationAPI->>MessageQueueAPI: Poll tenant for notifications
        MessageQueueAPI->>TenantDBs: Get messages from X.queues.notifications
        TenantDBs-->>MessageQueueAPI: Return messages
        MessageQueueAPI-->>NotificationAPI: Return messages
        
        NotificationAPI->>NotificationAPI: Process all messages for tenant X
        
        NotificationAPI->>MessageQueueAPI: POST /messagequeue/mark-processed?tenant=X&queue=notifications
        MessageQueueAPI->>CentralRegistry: UPDATE queue_registry SET notifications_pending = false WHERE tenant_id = 'X'
        CentralRegistry-->>MessageQueueAPI: Return OK
    end
```

### MessageQueueAPI can now do multi-tenant bulk polling

```mermaid
sequenceDiagram

    participant NotificationAPI.BackgroundService as NotificationAPI.BackgroundService
    participant MessageQueueAPI

        NotificationAPI.BackgroundService->>MessageQueueAPI: GET /messagequeue/poll
        note over MessageQueueAPI: MessageQueueAPI can now do multi-tenant bulk polling
        MessageQueueAPI->>MessageQueueAPI: Loop through all tenants and aggregate notifications from all of them
        MessageQueueAPI-->>NotificationAPI.BackgroundService: Return notifications (from all tenants)

        loop for each notification in notifications
            NotificationAPI.BackgroundService->>NotificationAPI.BackgroundService: Read recipient's preferences
            NotificationAPI.BackgroundService->>MessageQueueAPI: POST /messagequeue/publish/EmailTemplateShouldBePopulated <br> Header: X-Tenant-Identifier: tenant.TenantIdentifier <br> Body: emailNotification
            MessageQueueAPI-->>NotificationAPI.BackgroundService: Return OK
        end
```

### Cursor's suggestion for a custom solution that is less coupled than the one above

```mermaid
sequenceDiagram
    participant NA as NotificationAPI.BackgroundService
    participant TDS as TenantDiscoveryService
    participant MQA as MessageQueueAPI
    participant TDB as Tenant Databases
    
    NA->>TDS: GET /api/tenants/with-messages?queue=notifications
    TDS->>TDS: Query tenants with pending messages
    TDS-->>NA: Return ["tenant1", "tenant2", ...]
    
    loop for each tenant in activeTenants
        NA->>MQA: GET /messagequeue/{tenant}/poll?queue=notifications
        MQA->>TDB: Query tenant-specific database
        TDB-->>MQA: Return messages
        MQA-->>NA: Return tenant's messages
        
        loop for each notification
            NA->>NA: Process notification
            NA->>MQA: POST /messagequeue/{tenant}/publish
            MQA-->>NA: Return OK
        end
    end
```

### It can then easily be replaced with RabbitMQ

```mermaid
sequenceDiagram
    participant NA as NotificationAPI
    participant RMQ as RabbitMQ
    
    Note over NA, RMQ: Pub/Sub Pattern (Push-based)
    
    NA->>RMQ: Subscribe to queue "tenant.*.notifications"
    
    RMQ-->>NA: Callback when message arrives for tenant1
    NA->>NA: Process notification for tenant1
    
    RMQ-->>NA: Callback when message arrives for tenant2
    NA->>NA: Process notification for tenant2
    
    Note over NA, RMQ: Messages are pushed to NotificationAPI<br>No polling or tenant discovery needed
```

```mermaid
sequenceDiagram
    participant NA as NotificationAPI
    participant TDS as TenantDiscoveryService
    participant MQA as MessageQueueAPI (with dual backends)
    participant PG as PostgreSQL Tables
    participant RMQ as RabbitMQ

    Note over NA, RMQ: Hybrid Transition Period
    
    NA->>TDS: GET /api/tenants/with-messages?queue=notifications
    TDS-->>NA: Return ["tenant1", "tenant2", ...]
    
    NA->>MQA: GET /messagequeue/tenant1/poll
    
    alt Tenant1 migrated to RabbitMQ
        MQA->>RMQ: Query from RabbitMQ
    else Tenant2 still on PostgreSQL
        MQA->>PG: Query from PostgreSQL
    end
    
    MQA-->>NA: Return messages
```