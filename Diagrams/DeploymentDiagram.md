```mermaid
flowchart TB

    classDef description stroke-width:0px, color:#fff, fill:transparent, font-size:12px

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
            GatewayDescription["\- C# API <br> \- Authenticates <br> \- Identifies tenant <br> \- Forwards request with tenantId in HTTP header"]:::description
        end
    end
    
    subgraph Backend
        subgraph IMSCaseAPI
            IMSCaseAPIDescription["\- C# API <br> \- Public <br> \- Authorizes requests <br> \- Only for basic use cases"]:::description
        end
        subgraph WebAppAPI
            WebAppAPIDescription["\- C# API <br> \- Private <br> \- Only reachable from Gateway and LegacyMonolith <br> \- Authorizes requests <br>"]:::description
        end
        subgraph LegacyMonolith
            LegacyMonolithDescription["\- Java monolith <br> \- Built on Alfresco ECM <br> \- Will be gradually outphased for microservices"]:::description
        end
    end

    Client2((client2.imsdigitalpost.dk))

    %% Message Queue
    MessageQueue(["MessageQueue <br> \- Dapr pub/sub"])
    
    subgraph EmailService
        EmailServiceDescription["\- C# ServiceWorker <br>\- Holds a list of email templates <br> \- Dynamically populates templates with FluentEmail"]:::description
    end
    
    %% External Systems
    Client1MailInbox

    %% Relationships
    Client1 -->|Interacts with| Frontend
    Frontend -->|Requests| Gateway
    Gateway -->|Authenticates| Keycloak
    Keycloak -->|Returns JWT| Gateway
    Gateway -->|Queries tenant info| TenantAPI
    TenantAPI -->|Returns TenantInfo| Gateway
    
    Gateway -->|"Forwards request to <br> \(if route includes /newapi/\)"| WebAppAPI
    Gateway -->|Forwards request to| LegacyMonolith

    Client2 -->|Integrates with| IMSCaseAPI

    LegacyMonolith -->|Requests| WebAppAPI
    LegacyMonolith -->|Publishes message| MessageQueue

    WebAppAPI -->|Publishes message| MessageQueue

    IMSCaseAPI -->|Publishes message| MessageQueue

    MessageQueue -->|Subscribes to message| EmailService

    EmailService -->|Sends email content and  metadata to| SMTPServer

    SMTPServer -->|Sends email to| Client1MailInbox[clientname\@clientdomain.dk]
```