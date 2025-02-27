# System Context diagram showing IMS Case and its interactions with external systems

This diagram shows:
- IMS Case as the central system
- External systems that interact with IMS Case
- The key users/actors of the system
- The main relationships between systems

<br>
<br>

```mermaid
C4Context
    title System Context Diagram for IMS Case

    %% Define Participants
    Person(administrativeEmployee, "Administrative Employee", "An employee working in the administrative <br> department of i.e. a school")

    System(imsCase, "IMS Case", "Core case management system<br>for handling documents, tasks,<br>and notifications")

    System_Ext(imsDigitalPost, "IMS DigitalPost", "Handles secure digital<br>communication with citizens")
    System_Ext(optagelse, "Optagelse.dk", "National application portal<br>for educational institutions")
    System_Ext(officeAddin, "Office Add-in", "Microsoft Office integration<br>for document handling")

    %% Define Relationships
    Rel(administrativeEmployee, imsCase, "Manages cases and documents")
    Rel(administrativeEmployee, imsDigitalPost, "Manages sending of digital mail")
    Rel(administrativeEmployee, officeAddin, "Uses")
    Rel(imsCase, officeAddin, "Integrates with")
    Rel(imsCase, imsDigitalPost, "Manages sending of digital mail")
    Rel(imsCase, optagelse, "Receives applications from")
    Rel(imsDigitalPost, imsCase , "Sends documents to")

    %% Layout Configuration
    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
``` 