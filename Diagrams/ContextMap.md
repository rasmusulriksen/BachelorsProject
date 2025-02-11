```mermaid
flowchart TD
subgraph Notifications
    NotificationOrchestratorWorker
    EmailSenderWorker
    EmailTemplateAPI
    NotificationSettingsAPI
end
```