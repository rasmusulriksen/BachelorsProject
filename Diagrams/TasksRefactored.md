*The idea with this file is to illustrate the inherent problem of the Solr+Alfresco architecture compared to a "normal" REST API architecture*

## AS-IS: User navigates to "My tasks" to see a list of tasks
### Tasks are returned from a solr search (fast)

```mermaid
sequenceDiagram
    participant User
    participant WebApp
    participant Alfresco
    participant Solr

    User->>WebApp: Clicks on page "My Tasks"
    WebApp->>Alfresco: /alfresco/api/-default-/public/search/versions/1/search <br> Get list of tasks for user
    Alfresco->>Solr: Get list of tasks for user
    Solr->>Solr: Finds indexed tasks for user
    Solr-->>Alfresco: Returns list of indexed tasks
    Alfresco->>Alfresco: Uses index to find the actual tasks in database
    Alfresco-->>WebApp: Returns a list of tasks
    WebApp-->>User: Displays a list of tasks 
```

## TO-BE: User navigates to "My tasks" to see a list of tasks
### Tasks are returned from a query performed by the TasksAPI (fast)

```mermaid
sequenceDiagram
    participant User
    participant WebApp
    participant TasksAPI

    User->>WebApp: Clicks on page "My Tasks"
    WebApp->>TasksAPI: GET /api/tasks
    TasksAPI->>TasksAPI: Performs database query
    TasksAPI-->>WebApp: Returns a list of tasks
    WebApp-->>User: Displays a list of tasks 
```

## AS-IS: User adds a task to the "My tasks" list (eventual consistency with SOLR)
```mermaid
sequenceDiagram
    participant User
    participant WebApp
    participant Alfresco
    participant Solr

    User->>WebApp: Creates new task
    WebApp->>Alfresco: Create task
    Alfresco->>Alfresco: Insert task into database
    Alfresco-->>WebApp: Returns 200 OK
    WebApp-->>User: Displays "Task created"
    Solr->>Alfresco: Any news?
    Alfresco-->>Solr: Yes! A new task (node) has been created <br> Returns list of transactions
    Solr->>Solr: Updates index 
    WebApp->>WebApp: Waits 10 seconds
    WebApp->>Alfresco: /alfresco/api/-default-/public/search/versions/1/search <br> Get list of tasks for user
    Alfresco->>Solr: Where do I find the tasks for this user?
    Solr-->>Alfresco: Here! (returns index info)
    Alfresco->>Alfresco: Finds tasks in database
    Alfresco-->>WebApp: Returns list of tasks
    WebApp-->>User: Displays list of tasks
```

## TO-BE: User adds a task to the "My tasks" list (strong consistency with REST API)

```mermaid
sequenceDiagram
    participant User
    participant WebApp
    participant TasksAPI

    User->>WebApp: Creates new task
    WebApp->>TasksAPI: Create task
    Alfresco-->>WebApp: Returns 200 OK (And task created data)
    WebApp->>WebApp: Adds newly created task to list of tasks
    WebApp-->>User: Displays "Task created" and adds the new task to the list
```