# MessageHub

[![codecov](https://codecov.io/gh/Energinet-DataHub/geh-post-office/branch/main/graph/badge.svg?token=Z6XE42U97U)](https://codecov.io/gh/Energinet-DataHub/geh-post-office)

Welcome to the MessageHub domain of the [Green Energy Hub project](https://github.com/Energinet-DataHub/green-energy-hub).

- [Intro](#intro)
- [Delivering documents to the post office](#delivering-documents-to-the-post-office)
- [Peek and dequeue documents from the post office](#peek-and-dequeue-documents-from-the-post-office)
- [Architecture](#architecture)

## Intro

MessageHub is the central place for handling outbound documents from within the Green Energy Hub (GEH), i.e. any documents sent from GEH to outside actors have to go through the MessageHub as seen on the System Context diagram below.

![SystemContext-System context drawio](https://user-images.githubusercontent.com/17023767/141441586-39002062-6d6f-4764-9b0b-b7c917d6ffe1.png)

Highlighted in red, the MessageHub acts as a middleman between other domains within GEH and market actors. Other domains deliver data to the MessageHub which are sent to market actors on request. By centralizing communication through MessageHub, domains avoid to implement local message hubs and instead can specialize in domain specific knowledge. Also, MessageHub keeps state on documents and requests which can be useful for internal business interests.

For a market actor to retrieve data from GEH, it needs to provide an appropriate type which the MessageHub uses to identify which domain(s) to retrieve the data from, see [Delivering documents to the post office](#delivering-documents-to-the-post-office).

The market actors will only be able to get (peek) and read documents that they are marked as recipients of. The same goes for documents market actors want to dequeue, i.e. tell MessageHub the document has been processed, and thereby indicate they are ready to get new documents. See [Fetching documents from the post office](#peek-and-dequeue-documents-from-the-post-office).

### Architecture

To document MessageHub architecture the [C4 model](https://c4model.com/) has been chosen which is a top-down architecture approach. The focus in this section is to try to communicate concepts within MessageHub and how internal subsystems behave and function on a higher level. The focus is also to let the reader understand interfaces, both within the MessageHub and between the MessageHub and domains and market actors. The deepest level of C4, the code level, has not been documented as recommended by the creator of the C4 model, [Simon Brown](https://simonbrown.je/). Instead, it is recommended to read the [codebase](https://github.com/Energinet-DataHub/geh-post-office/tree/main/source) to understand the details.

MessageHub architecture is built on [Domain Driven Design (DDD)](https://martinfowler.com/tags/domain%20driven%20design.html).

**Container diagram**  
The first level of C4 is the System Context diagram which was presented in [Intro](#intro). The diagram is used to get an overview of the system from a non-technical viewpoint.

Next is the container diagram showed below. The first thing to notice is that MessageHub is actually divided into two parts - the main system, MessageHub, and a subsystem, Libraries.

The main system handles core business logic and communication with market actors (through API management). Libraries handles primarily communication between MessageHub and other domains. Two databases also appear, one for storing data available-notifications and one for market actor-specific data.

![Container diagram](https://user-images.githubusercontent.com/17023767/141787188-5aea1090-ca82-4e44-bf38-e80c29c01903.png)

Inside MessageHub five boxes are drawed. From left, the boxes have the following responsibilities:

- **Sub domain entry point** acts as interface between Libraries and MessageHub and processes all data-available notifications.
- **Domain** contains business logic to represent MessageHub.
- **Application** handles all incoming API calls to MessageHub and coordinates tasks based on the request.
- **Infrastructure** accesses data.
- **Market actor entry point** acts as interface between market actors and MessageHub.

Libraries contains the following with responsibilities:

- **Model** contains business logic to represent Client and Core.
- **Client** implements logic to communicate with MessageHub and store market actor-specific data. This package is for other domains to use.
- **Core** implements logic to communicate with other domains and retrieve data for market actors. This package can be used by MessageHub.

**Why does Libraries exist?**  
Providing nuget packages instead of implementing the logic in the domains help to make the system platform independent. MessageHub and the rest of Green Energy Hub depend heavily on [Azure](https://azure.microsoft.com/) which might not be the best suited solution looking forward.

**Component diagram**  
Two component diagrams have been made to avoid making the diagram too detailed - one for MessageHub and one for Libraries.

![Component diagram - MessageHub](https://user-images.githubusercontent.com/17023767/141962183-89c8eecb-97ca-4922-a0c6-05fc03e83403.png)

![Component diagram - Libraries](https://user-images.githubusercontent.com/17023767/141967933-dd36c91b-db1a-43c6-ab23-63bd60ae41a8.png)

Flow of communication between MessageHub and domains is always through four Azure Service Bus queues as seen below.

![QueuesDiagram](https://user-images.githubusercontent.com/17023767/141968153-7baa3b44-d9da-4d59-b24e-8c26ebd8dd59.png)


## Delivering documents to the post office

ToDo

### Format

All documents inserted into the queues will have to comply with the protobuf contract.

If a document is inserted into the queue that does not comply with this contract, **IT WILL NOT** be handled.

More ToDo?

## Peek and dequeue documents from the post office

### Authenticating

TODO: This will have to be updated once we know more about how authentication is done throughout the system.

### GET:/Peek

ToDo

#### Peek URI Parameters

ToDo

#### Peek Responses

ToDo

### GET:/Dequeue

ToDo

#### Dequeue Request body

ToDo

#### Dequeue Responses

ToDo

## Types

### Peeked documents

ToDo

An array of documents.

```json
[
   {
      "Recipient": "string"
   }
]
```
