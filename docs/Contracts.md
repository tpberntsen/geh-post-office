# Protocol Documentation

This document describes contracts used to communicate with PostOffice from within DataHub/GreenEnergyHub. 
The first section is for .NET users who can benefit from the nuget package provided. 
All other users should skip the first section and only read section two where ProtoBuf contracts are described.

## Table of Contents

- [.NET users](#.NETusers)
    - [DataAvailableNotificationDto](#DataAvailableNotificationDto)
    - [DataBundleRequestDto](#DataBundleRequestDto)
    - [DataBundleResponseDto](#DataBundleResponseDto)
- [Other users](#.OtherUsers)
    - [DataAvailable.proto](#DataAvailable.proto)

<a name=".NETusers"></a>

## .NET users
The nuget package provided contains all necessary logic and infrastructure components to enable communication between PostOffice and a sub domain within DataHub/GreenEnergyHub.
Here, all the contracts (DTO's) are described.

To get the nuget package, search for 'GreenEnergyHub.PostOffice.Communicator' from nuget.org.

<hr>

<a name=".DataAvailableNotificationDto"></a>

### DataAvailableNotificationDto

The fields are based on strongly typed custom types. Below the table are each type and type limits described.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Uuid | Uuid | required | Identifier for the Data Available Notification sent from the sub domain |
| Recipient | Recipient | required | The Market Operator to receive the data |
| MessageType | MessageType | required | The RSM type the Data Available Notification consists of |
| Origin | Origin | required | The sub domain which sends the Data Available Notification |
| SupportsBundling | SupportsBundling | required | Flag to indicate whether or not the data in the Data Available Notification can be bundled |
| RelativeWeight | RelativeWeight | required | The weight of the data |

<b>Uuid</b> contains a <i>string</i> property named Id. Id must be a valid Guid in string format.

<b>Recipient</b> contains a <i>string</i> property named MarketOperator. MarketOperator must be a valid identifier of known market operators.

<b>MessageType</b> contains a <i>string</i> property named TypeOfMessage. TypeOfMessage must be an RSM type identifier.

<b>Origin</b> contains a <i>string</i> property named SubDomain. SubDomain must be a known sub domain within DataHub/GreenEnergyHub.

<b>SupportsBundling</b> contains a <i>bool</i> property named IsBundlingSupported.

<b>RelativeWeight</b> contains an <i>int</i> property named MessageWeight. MessageWeight must be a number between 0 and 2147483647 (Int32.MaxValue). 
Can only be 0 if IsBundlingSupported is set to false.

<hr>

<a name=".DataBundleRequestDto"></a>

### DataBundleRequestDto

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| IdempotencyId | string | required | An Id for sub domains to check whether or not it has received the same message multiple times |
| DataAvailableNotificationIds | IEnumerable<string> | required | One or multiple Id's to identify requested data bundle |
  
<hr>
  
<a name=".DataBundleResponseDto"></a>

### DataBundleResponseDto

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| DataAvailableNotificationIds | IEnumerable<string> | required | One or multiple Id's to identify requested data bundle |
| ContentUri | Uri | optional | Uri to get requested data |
| IsErrorResponse | IsErrorResponse | required | Flag to indicate if response is error |
| ResponseError | DataBundleResponseError | optional | One or multiple Id's to identify requested data bundle |

<hr>

<a name=".DequeueNotificationDto"></a>

### DequeueNotificationDto

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| DataAvailableNotificationIds | ICollection<string> | required | One or multiple Id's to identify data to dequeue |
| Recipient | string | optional | The Market Operator who sends the Dequeue message |

<hr>

<a name=".OtherUsers"></a>

## Other users


<a name=".DataAvailable.proto"></a>

### DataAvailable.proto
