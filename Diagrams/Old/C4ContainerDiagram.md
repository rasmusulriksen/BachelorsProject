# Questions to be answered

- Should the gateway create a tracingId/correlationId that is propagated with all subsequent requests?
- Or should we "just" use Dapr for all communication which then provides out-of-the-box distributed tracing
- Do we need to support the ability to see a communication log (See what has been sent to a specific contact). Or is this too CRM-ish?

```mermaid
flowchart TB

    %% Styling
    classDef description stroke-width:0px, color:#fff, fill:transparent, font-size:12px

    %% Components
    Client1((client1.imscase.dk))

    subgraph Frontend
        FrontendDescription["\- WebApp <br> \- TypeScript <br> \- Vue3 <br> \- Vuetify <br>\- Pinia"]:::description
    end

    subgraph Middleware
        subgraph Keycloak
            KeycloakDescription["\- Open source Identity Provider <br> \- Each tenant has its own realm"]:::description
        end
        subgraph TenantAPI
            TenantAPIDescription["\- C# API <br> \- Wraps a database holding tenant info <br> \- Tells the Gateway which Keycloak realm to authenticate up against"]:::description
        end
        subgraph Gateway
            GatewayDescription["\- C# API <br> \- Authenticates <br> \- Identifies tenant <br> \- Forwards request with tenantId and JWT (is tenantId baked into JWT?)"]:::description
        end
    end

    subgraph Backend
        subgraph ExternalAPI
            ExternalAPIDescription["\- C# API <br> \- Public <br> \- Authorizes requests <br> \- Only for basic use cases"]:::description
        end
        subgraph InternalAPI
            InternalAPIDescription["\- C# API <br> \- Private <br> \- Only reachable from Gateway and LegacyMonolith <br> \- Authorizes requests <br>"]:::description
        end
        subgraph LegacyMonolith
            LegacyMonolithDescription["\- Java monolith <br> \- Built on Alfresco ECM <br> \- Will be gradually outphased for microservices"]:::description
        end
    end

    Client2((client2.imsdigitalpost.dk))

    subgraph NotificationService

        subgraph NotificationSchedulerWorker
            NotificationSchedulerWorkerDescription["<br>- C# ServiceWorker <br>- Runs every 10 seconds <br>- Dequeues notifications from the table 'notifications_to_be_orchestrated' <br>- Looks up the recipient user's notification preferences and decides to either send or postpone <br>- SendNotificationNow() publishes the event InstantNotificationScheduled <br>- PostponeNotification() saves the notification to NotificationSchedulerWorkerDB <br> - At 08:00, 12:00 and 16:00 it selects all notifications from the database, groups by recipient, foreach recipient publishes the event SummaryNotificationScheduled <br>- Events include an identifier that the subscriber will use to find the right email template"]:::description
            NotificationSchedulerWorkerDB@{ shape: cyl, label: "NotificationSchedulerWorkerDB \n -PostgreSQL \n -Shared table"}
        end

        subgraph EmailSenderWorker
            EmailSenderWorkerDescription["<br>- C# ServiceWorker <br>- Runs every 10 seconds <br>- Dequeues notifications from the table 'notifications_to_be_sent' and just sends them. Simple as that. <br> -Can be used for all sending purposes throughout the system (single-notification emails, aggregated-notifications emails)"]:::description
        end

        subgraph EmailTemplateAPI
            EmailTemplateAPIDescription["\- C# API <br> \- Lets users CRUD their own custom email templates <br> \- Lets users preview what the final email will look like <br>"]:::description
            EmailTemplateDB@{ shape: cyl, label: "EmailTemplateDB \n -PostgreSQL \n -Shared table " }
            click EmailTemplateDB href "https://github.com/rasmusulriksen/BachelorsProject/blob/master/Diagrams/ERDiagramEmailTemplateDB.md"
        end

        subgraph EmailTemplatePopulatorWorker
            EmailTemplatePopulatorDescription["\- ServiceWorker <br> \- Fills out template with dynamic data (Handlebars.Net) and publishes the event EmailTemplatePopulated"]:::description
        end

        subgraph InAppNotificationSenderWorker
            InAppNotificationSenderWorkerDescription["\- ServiceWorker <br> \- Sends in app notifications to NotificationAPI"]:::description
        end

        subgraph NotificationSettingsAPI
            NotificationSettingsAPIDescription["<br>- C# API <br>- Each tenant can configure very specific notification settings which are stored here. In this first iteration it will only be frequency (Immediate, 8am, 12 am, 4pm)"]:::description
            NotificationSettingsDB@{ shape: cyl, label: "NotificationSettingsDB \n -PostgreSQL \n -Shared table" }
        end
    end

    %% Relationships
    Client1 -->|Interacts with| Frontend
    Frontend -->|Requests| Gateway
    Gateway -->|"Authenticates (receives JWT)"| Keycloak
    Gateway -->|Queries tenant info| TenantAPI

    Gateway -->|"Forwards request to <br> \(if route includes /newapi/\)"| InternalAPI
    Gateway -->|Forwards request to| LegacyMonolith

    Client2 -->|Integrates with| ExternalAPI

    LegacyMonolith -->|Requests| InternalAPI
    LegacyMonolith -->|"_messageQueue.publish('NotificationCreated')"| NotificationSchedulerWorker

    InternalAPI -->|"_messageQueue.publish('NotificationCreated')"| NotificationSchedulerWorker

    InternalAPI -->|"_daprClient.InvokeMethodAsync('EmailTemplateAPI', 'template/{templateId}' ) <br> (CRUD custom email templates)"| EmailTemplateAPI

    InternalAPI -->|"_daprClient.InvokeMethodAsync('NotificationSettingsAPI', 'preferences') <br> (CRUD notification settings)"| NotificationSettingsAPI

    ExternalAPI -->|"_messageQueue.publish('NotificationCreated')"| NotificationSchedulerWorker

    NotificationSchedulerWorker -->|"_daprClient.InvokeMethodAsync('NotificationSettingsAPI', 'preferences/{userId}') <br> (Checks user's preferences)"|NotificationSettingsAPI

    NotificationSchedulerWorker -->|"_messageQueue.publish('InstantNotificationScheduled')"|EmailTemplatePopulatorWorker
    NotificationSchedulerWorker -->|"_messageQueue.publish('SummaryNotificationScheduled')"|EmailTemplatePopulatorWorker

    NotificationSchedulerWorker -->|"_messageQueue.publish('SummaryNotificationScheduled')"|InAppNotificationSenderWorker

    EmailTemplatePopulatorWorker -->|"_messageQueue.publish('EmailTemplatePopulated')"|EmailSenderWorker

    EmailSenderWorker -->|"Connects to SMTP-server and sends the email"|SMTPServer["SMTP Server, external (heysender.com)"]

```
