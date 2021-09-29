# Protocol Documentation - !!! IS NOT UP TO DATE AT THE MOMENT !!!

This document describes contracts used to communicate with PostOffice from within DataHub/GreenEnergyHub. 
The first section is for .NET users who can benefit from the nuget package provided. 
All other users should skip the first section and only read section two where ProtoBuf contracts are described.

## Table of Contents

- [.NET users](#.NETusers)
    - [DataAvailableNotificationDto](#DataAvailableNotificationDto)
    - [DataBundleRequestDto](#DataBundleRequestDto)
    - [DataBundleResponseDto](#DataBundleResponseDto)
- [Other users](#.OtherUsers)
    - [DataAvailableNotificationContract.proto](#DataAvailableNotificationContract.proto)

<a name=".NETusers"></a>

## .NET users
The nuget package provided contains all necessary logic and infrastructure components to enable communication between PostOffice and a sub domain within DataHub/GreenEnergyHub.
Here, all the contracts (DTO's) are described.

To get the nuget package, search for 'GreenEnergyHub.PostOffice.Communicator' from nuget.org.

<hr>

<a name=".DataAvailableNotificationDto"></a>

### DataAvailableNotificationDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| Uuid | Guid | required | Identifier for the Data Available Notification sent from the sub domain | Must be a valid Guid |
| GlobalLocationNumber | GlobalLocationNumber | required | The Market Operator to receive the data | Must be a known GLN number |
| MessageType | MessageType | required | The RSM type the Data Available Notification consists of | Must be a known RSM type |
| Origin | enum | required | The sub domain which sends the Data Available Notification | Must be a known sub domain within DataHub/GreenEnergyHub |
| SupportsBundling | bool | required | Flag to indicate whether or not the data in the Data Available Notification can be bundled | N/A |
| RelativeWeight | int | required | The weight of the data | Must be a number between 0 and 2147483647 (Int32.MaxValue) |

<hr>

<a name=".DataBundleRequestDto"></a>

### DataBundleRequestDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| IdempotencyId | string | required | An Id for sub domains to check whether or not it has received the same message multiple times | None at the moment |
| DataAvailableNotificationIds | IEnumerable<string> | required | One or multiple Id's to identify requested data bundle | None at the moment |
  
<hr>
  
<a name=".DataBundleResponseDto"></a>

### DataBundleResponseDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| DataAvailableNotificationIds | IEnumerable<string> | required | One or multiple Id's to identify requested data bundle | None at the moment |
| ContentUri | Uri | optional | Uri to get requested data | Must be a valid Uri to data storage |
| IsErrorResponse | bool | required | Flag to indicate if response is error | N/A |
| ResponseError | DataBundleResponseError | optional | One or multiple Id's to identify requested data bundle |

<hr>

<a name=".DequeueNotificationDto"></a>

### DequeueNotificationDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- |
| DataAvailableNotificationIds | ICollection<string> | required | One or multiple Id's to identify data to dequeue | None at the moment |
| GlobalLocationNumber | GlobalLocationNumber | required | The Market Operator to receive the data | Must be a known GLN number |

<hr>

<a name=".OtherUsers"></a>

## Other users

TODO

<a name=".DataAvailableNotificationContract.proto"></a>

### DataAvailableNotificationContract.proto

TODO
