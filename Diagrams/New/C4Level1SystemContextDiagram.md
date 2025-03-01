# System Context Diagram for IMS Case

This diagram shows:
- IMS Case as the central system
- External systems that interact with IMS Case
- The key users/actors of the system
- The main relationships between systems

```mermaid
C4Context
    Person(schoolAdministrator, "School Administrator", "End user (frequent user)")


    System_Ext(officeAddin, "Office Add-in", "An IMS Case plugin for Microsoft Office apps<br>i.e. Word, Excel, Outlook")
    System_Ext(addo, "Visma Addo", "Used for signing documents")

    System_Ext(imsDigitalPost, "IMS DigitalPost", "Handles secure digital<br>communication with citizens")
    System(imsCase, "IMS Case", "Core case management system<br>for handling documents, tasks,<br>and notifications")

    System_Ext(smtp, "SMTP Server", "Used for sending emails")
    System_Ext(firstAgenda, "FirstAgenda", "System for managing meetings")

    Person(schoolLeader, "School Leader", "End user (occasional user)")
    Person(student, "Student", "Subject of cases, documents etc. <br> Is often notified")

    System_Ext(optagelse, "Optagelse.dk", "National application portal<br>for educational institutions")

    %% Define Relationships
    Rel(schoolAdministrator, imsCase, "Manages cases and documents")
    Rel(schoolAdministrator, officeAddin, "Uses office apps")
    Rel(schoolAdministrator, imsDigitalPost, "Sends digital mail")
    BiRel(officeAddin, imsCase, "Integrates with")
    BiRel(imsCase, imsDigitalPost, "Integrates with")
    BiRel(imsCase, addo, "Integrates with")
    Rel(imsCase, smtp, "Integrates with")
    Rel(smtp, schoolLeader, "Receives email")
    BiRel(imsCase, firstAgenda, "Integrates with")
    BiRel(imsCase, optagelse, "Integrates with")
    Rel(smtp, student, "Receives email")
    Rel(schoolLeader, imsCase, "Signs documents, <br>gives approvals")


    %% Layout Configuration
    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
``` 