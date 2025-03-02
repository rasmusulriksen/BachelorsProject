# Deployment diagram showing the infrastructure and how the software system's containers are deployed

This diagram shows:

- The physical/virtual infrastructure that the system runs on
- How each service is deployed as a separate container/unit
- Details about deployment nodes, network boundaries, etc.
- Clear layering: Client → Middleware → Backend
- Database architecture showing siloed tenant databases
- Separation between internal and external API access patterns
- Basic communication flows between main components

<br>
<br>

```mermaid
flowchart TD
    classDef container fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef plane fill:transparent,stroke:#666,stroke-width:2px,color:#fff
    classDef database fill:#3a3a3a,stroke:#666,stroke-width:1px,color:#fff
    classDef gateway fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef webapp fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef client fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10
    classDef monolith fill:#2a2a2a,stroke:#666,stroke-width:2px,color:#fff,rx:10

    subgraph ClientLayer["Client Layer"]
        WebApp["Internal WebApp<br>Vue3 + TypeScript"]:::webapp
        ExternalClients["External Client<br>Systems"]:::client
    end

    subgraph MiddlewareLayer["Middleware Layer"]
        Gateway["API Gateway"]:::gateway

        subgraph ControlPlane["Control Plane"]
            subgraph TenantService
                TenantAPI["TenantAPI"]:::container
                TenantDB[("TenantDB")]:::database
            end
            subgraph OnboardingService
                OnboardingAPI["OnboardingAPI"]:::container
                OnboardingDB[("OnboardingDB")]:::database
            end
            subgraph IdentityProvider
                Keycloak["Keycloak"]:::container
                Tenant1Realm[("Tenant1Realm")]:::database
                Tenant2Realm[("Tenant2Realm")]:::database
                TenantNRealm[("TenantNRealm")]:::database
            end
        end
    end

    subgraph BackendLayer["Backend Layer"]
        subgraph APIs["APIs"]
            IMSCaseAPIInt["IMSCaseAPIInternal"]:::container
            IMSCaseAPIExt["IMSCaseAPIExternal"]:::container
        end

            subgraph Services["Microservices"]
                NotificationService["NotificationService"]:::container
                TaskService["TaskService"]:::container
            end

            subgraph Monoliths["Monoliths"]

                subgraph MonolithN["LegacyMonolithN"]
                    LegacyAppN["LegacyMonolithN"]:::container
                    LegacyDBN[("LegacyMonolithNDB")]:::database
                end
                MonolithN:::monolith

                subgraph Monolith2["LegacyMonolith2"]
                    LegacyApp2["LegacyMonolith2"]:::container
                    LegacyDB2[("LegacyMonolith2DB")]:::database
                end
                Monolith2:::monolith

                subgraph Monolith1["LegacyMonolith1"]
                    LegacyApp1["LegacyMonolith1"]:::container
                    LegacyDB1[("LegacyMonolith1DB")]:::database
                end
                Monolith1:::monolith

        end
        subgraph Databases["Tenant Databases"]
            Client1DB[("Client1DB<br>- notifications<br>- tasks")]:::database
            Client2DB[("Client2DB<br>- notifications<br>- tasks")]:::database
            ClientNDB[("ClientNDB<br>- notifications<br>- tasks")]:::database
        end
    end

    %% Basic interactions
    WebApp --> Gateway
    ExternalClients --> Gateway
    Gateway --> ControlPlane
    Gateway --> APIs
    APIs --> Services
    APIs --> Monoliths
    Services --> Databases
```
