# Sequence diagram showing the login flow with tenant recognition

```mermaid
sequenceDiagram
    actor User
    participant WebApp
    participant Gateway
    participant TenantService
    participant Keycloak
    
    User->>WebApp: Visits client1.imscase.dk
    WebApp->>Gateway: Login request
    Gateway->>TenantService: Extract tenant from subdomain
    TenantService-->>Gateway: Tenant info (client1)
    
    Gateway->>Keycloak: Auth request with tenant realm
    Keycloak-->>Gateway: JWT token
    Gateway-->>WebApp: Auth success + JWT
    WebApp-->>User: Logged in successfully
``` 