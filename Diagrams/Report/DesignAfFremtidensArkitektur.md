# Design af fremtidens arkitektur

## Diagram1: AS IS

```mermaid
flowchart TD
    %% Define styles for all diagram elements
    classDef monolith fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef database fill:#3a3a3a,stroke:#666,stroke-width:1px,color:#fff
    classDef column fill:transparent,stroke:transparent,stroke-width:0px

    subgraph ColumnN[" "]
        direction LR
        subgraph LegacyMonolithN["LegacyMonolithN"]
            LMN["CaseAPI<br>DocumentAPI<br>FileAPI<br>WorkflowAPI<br>UserAPI<br>NotificationAPI<br>SigningAPI<br>BackupAPI<br>CassationAPI<br>AuditAPI<br>AuthAPI<br>AnonymizationAPI"]
            DBN[(DatabaseN)]
            LMN --> DBN
        end
    end

    subgraph Column2[" "]
        direction LR
        subgraph LegacyMonolith2["LegacyMonolith2"]
            LM2["CaseAPI<br>DocumentAPI<br>FileAPI<br>WorkflowAPI<br>UserAPI<br>NotificationAPI<br>SigningAPI<br>BackupAPI<br>CassationAPI<br>AuditAPI<br>AuthAPI<br>AnonymizationAPI"]
            DB2[(Database2)]
            LM2 --> DB2
        end
    end

    subgraph Column1[" "]
        direction LR
        subgraph LegacyMonolith1["LegacyMonolith1"]
            LM1["CaseAPI<br>DocumentAPI<br>FileAPI<br>WorkflowAPI<br>UserAPI<br>NotificationAPI<br>SigningAPI<br>BackupAPI<br>CassationAPI<br>AuditAPI<br>AuthAPI<br>AnonymizationAPI"]
            DB1[(Database1)]
            LM1 --> DB1
        end
    end

    %% Apply classes
    class LM1,LM2,LMN monolith
    class DB1,DB2,DBN database
    class Column1,Column2,ColumnN column

    %% Make column containers invisible
    style Column1 fill:transparent,stroke:transparent
    style Column2 fill:transparent,stroke:transparent
    style ColumnN fill:transparent,stroke:transparent
```

<br>
<br>
<br>
<br>
<br>

## Diagram2: TO BE

```mermaid
flowchart TD
    %% Define styles for all diagram elements
    classDef monolith fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef microservice fill:#3a3a3a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef database fill:#3a3a3a,stroke:#666,stroke-width:1px,color:#fff
    classDef tenant fill:#1a1a1a,stroke:#666,stroke-width:2px,color:#fff,rx:0
    classDef messageBroker fill:#1a1a1a,stroke:#666,stroke-width:2px,color:#fff,rx:5

    subgraph Clients
        subgraph client1
        end
        subgraph client2
        end
    end

    subgraph Middleware
        subgraph TenantAPI
        end
        subgraph Gateway
        end
        subgraph IdentityProvider
        end
    end

    subgraph APIs
        subgraph InternalAPI
        end

        subgraph ExternalAPI
        end
    end

    subgraph Monoliths["Monoliths"]
        subgraph LegacyMonolith1["Alfresco Monolith 1"]
            LM1["CaseAPI<br>DocumentAPI<br>FileAPI"]
            DB1[(Database1)]
            LM1 --> DB1
        end

        subgraph LegacyMonolith2["Alfresco Monolith 2"]
            LM2["CaseAPI<br>DocumentAPI<br>FileAPI"]
            DB2[(Database2)]
            LM2 --> DB2
        end

        subgraph LegacyMonolithN["Alfresco Monolith N"]
            LMN["CaseAPI<br>DocumentAPI<br>FileAPI"]
            DBN[(DatabaseN)]
            LMN --> DBN
        end
    end

    subgraph MessageBroker["Message Broker"]
        MessageBus["MessageQueueAPI"]
    end

    subgraph MultiTenantServices["Multi-Tenant Microservices"]
        subgraph ServiceSet1[" "]
            direction LR
            WorkflowAPI["WorkflowAPI"]
            UserAPI["UserAPI"]
            NotificationAPI["NotificationAPI"]
        end

        subgraph ServiceSet2[" "]
            direction LR
            SigningAPI["SigningAPI"]
            BackupAPI["BackupAPI"]
            CassationAPI["CassationAPI"]
        end

        subgraph ServiceSet3[" "]
            direction LR
            AuditAPI["AuditAPI"]
            AuthAPI["AuthAPI"]
            AnonymizationAPI["AnonymizationAPI"]
        end
    end

    subgraph MicroserviceDatabase["Microservice databases <br> database-per-tenant"]
        TenantDB1[(Tenant1)]
        TenantDB2[(Tenant2)]
        TenantDBN[(TenantN)]
    end

    Clients --> Middleware
    Middleware --> APIs
    APIs --> LegacyMonolith1
    APIs --> LegacyMonolith2
    APIs --> LegacyMonolithN
    APIs --> MessageBroker

    LegacyMonolith1 --> MessageBroker
    LegacyMonolith2 --> MessageBroker
    LegacyMonolithN --> MessageBroker

    MessageBroker <--> MultiTenantServices

    MultiTenantServices --> MicroserviceDatabase

    %% Apply classes
    class LM1,LM2,LMN monolith
    class WorkflowAPI,UserAPI,NotificationAPI,SigningAPI,BackupAPI,CassationAPI,AuditAPI,AuthAPI,AnonymizationAPI,ExternalAPI,InternalAPI,Gateway,TenantAPI,client1,client2,IdentityProvider microservice
    class DB1,DB2,DBN,TenantDB1,TenantDB2,TenantDBN database

    %% Make service set containers invisible
    style ServiceSet1 fill:transparent,stroke:transparent
    style ServiceSet2 fill:transparent,stroke:transparent
    style ServiceSet3 fill:transparent,stroke:transparent
```

<br>
<br>
<br>
<br>
<br>

## Diagram3: TO BE (but too far fetched bearing in mind the current state of business and technical maturity)

```mermaid
flowchart TD
    %% Define styles for all diagram elements
    classDef monolith fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef microservice fill:#3a3a3a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef database fill:#3a3a3a,stroke:#666,stroke-width:1px,color:#fff
    classDef tenant fill:#1a1a1a,stroke:#666,stroke-width:2px,color:#fff,rx:0
    classDef messageBroker fill:#1a1a1a,stroke:#666,stroke-width:2px,color:#fff,rx:5

    subgraph Clients
        subgraph client1
        end
        subgraph client2
        end
    end

    subgraph Middleware
        subgraph TenantAPI
        end
        subgraph Gateway
        end
        subgraph IdentityProvider
        end
    end

    subgraph MessageBroker["Message Broker"]
        MessageBus["RabbitMQ"]
    end

    subgraph MultiTenantServices["Multi-Tenant Microservices"]
        subgraph ServiceSet1[" "]
            direction LR
            WorkflowAPI["WorkflowAPI"]
            UserAPI["UserAPI"]
            NotificationAPI["NotificationAPI"]
        end

        subgraph ServiceSet2[" "]
            direction LR
            SigningAPI["SigningAPI"]
            BackupAPI["BackupAPI"]
            CassationAPI["CassationAPI"]
        end

        subgraph ServiceSet3[" "]
            direction LR
            AuditAPI["AuditAPI"]
            AuthAPI["AuthAPI"]
            AnonymizationAPI["AnonymizationAPI"]
        end

        subgraph ServiceSet4[" "]
            direction LR
            CaseAPI["CaseAPI"]
            DocumentAPI["DocumentAPI"]
            FileAPI["FileAPI"]
        end
    end

    subgraph MicroserviceDatabase["Microservice databases <br> database-per-tenant"]
        TenantDB1[(Tenant1)]
        TenantDB2[(Tenant2)]
        TenantDBN[(TenantN)]
    end

    %% Client and API gateway interactions
    Clients --> Middleware
    Middleware --> MultiTenantServices

    %% Database interactions
    MultiTenantServices --> MicroserviceDatabase

    %% Event-driven communication through message broker
    MultiTenantServices <--> MessageBus

    %% Apply classes
    class WorkflowAPI,UserAPI,NotificationAPI,SigningAPI,BackupAPI,CassationAPI,AuditAPI,AuthAPI,AnonymizationAPI,CaseAPI,DocumentAPI,FileAPI microservice
    class DB1,DB2,DBN,TenantDB1,TenantDB2,TenantDBN database
    class MessageBus messageBroker

    %% Make service set containers invisible
    style ServiceSet1 fill:transparent,stroke:transparent
    style ServiceSet2 fill:transparent,stroke:transparent
    style ServiceSet3 fill:transparent,stroke:transparent
    style ServiceSet4 fill:transparent,stroke:transparent
```