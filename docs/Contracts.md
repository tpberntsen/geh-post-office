# Protocol Documentation

This document describes contracts used to communicate with PostOffice from within DataHub/GreenEnergyHub. 
The first section is for .NET users who can benefit from the nuget package provided. 
All other users should skip the first section and only read section two where ProtoBuf contracts are described.

## Table of Contents

- [.NET users](#.NETusers)
    - [DataAvailableNotificationDto](#.DataAvailableNotificationDto)
    - [DataBundleRequestDto](#.DataBundleRequestDto)
    - [DataBundleResponseDto](#.DataBundleResponseDto)
    - [DequeueNotificationDto](#.DequeueNotificationDto)
- [Other users](#.OtherUsers)
    - [DataAvailableNotificationContract.proto](#.DataAvailableNotificationContract.proto)
    - [RequestBundleRequest.proto](#.RequestBundleRequest.proto)
    - [RequestBundleResponse.proto](#.RequestBundleResponse.proto)
    - [DequeueContract.proto](#.DequeueContract.proto)

<a name=".NETusers"></a>

## .NET users
The nuget package provided contains all necessary logic and infrastructure components to enable communication between PostOffice and a sub domain within DataHub/GreenEnergyHub.
Here, all the contracts (DTO's) are described.

To get the nuget package, search for 'GreenEnergyHub.PostOffice.Communicator' from nuget.org.

<hr>
<br>

<a name=".DataAvailableNotificationDto"></a>

### DataAvailableNotificationDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| Uuid | Guid | required | Unique dataset identification | Must be a valid Guid |
| GlobalLocationNumber | GlobalLocationNumber | required | Market Operator to receive dataset | Must be a known GLN number |
| MessageType | MessageType | required | Message RSM type | Must be a known RSM type |
| Origin | enum | required | The sub domain which sends the Data Available Notification | Must be a known sub domain within DataHub/GreenEnergyHub |
| SupportsBundling | bool | required | Flag to indicate if message is capable of being bundled with similar messages | N/A |
| RelativeWeight | int | required | The relative weight of the dataset | Must be a number between 0 and 2147483647 (Int32.MaxValue) |

<hr>
<br>

<a name=".DataBundleRequestDto"></a>

### DataBundleRequestDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| IdempotencyId | string | required | An Id for sub domains to check whether or not it has received the same message multiple times | None at the moment |
| DataAvailableNotificationIds | IEnumerable<string> | required | One or multiple Id's to identify requested data bundle | None at the moment |
  
<hr>
<br>
  
<a name=".DataBundleResponseDto"></a>

### DataBundleResponseDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| DataAvailableNotificationIds | IEnumerable<string> | required | One or multiple Id's to identify requested data bundle | None at the moment |
| ContentUri | Uri | optional | Uri to get requested data | Must be a valid Uri to data storage |
| IsErrorResponse | bool | required | Flag to indicate if response is error | N/A |
| ResponseError | DataBundleResponseError | optional | One or multiple Id's to identify requested data bundle | N/A |

<hr>
<br>

<a name=".DequeueNotificationDto"></a>

### DequeueNotificationDto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| DataAvailableNotificationIds | ICollection<string> | required | One or multiple Id's to identify data to dequeue | None at the moment |
| GlobalLocationNumber | GlobalLocationNumber | required | The Market Operator to receive the data | Must be a known GLN number |

<hr>

<a name=".OtherUsers"></a>

<br>
<br>
    
## Other users
    
<a name=".DataAvailableNotificationContract.proto"></a>

### DataAvailableNotificationContract.proto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| UUID | string | required | Unique dataset identification | Must be a valid Guid in string format |
| recipient | string | required | Market Operator to receive dataset | Must be a known GLN number |
| messageType | string | required | Message RSM type | Must be a known RSM type |
| origin | string | required | The sub domain which sends the Data Available Notification | Must be a known sub domain within DataHub/GreenEnergyHub |
| supportsBundling | bool | required | Flag to indicate if message is capable of being bundled with similar messages | N/A |
| relativeWeight | int32 | required | The relative weight of the dataset | Must be a number between 0 and 2147483647 (Int32.MaxValue) |

    
<hr>
<br>
    
<a name=".RequestBundleRequest.proto"></a>

### RequestBundleRequest.proto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| IdempotencyId | string | required | An Id for sub domains to check whether or not it has received the same message multiple times | None at the moment |
| UUID | repeated string | required | Unique dataset identification | Must be a valid Guid in string format |
    
<hr>
<br>
    
<a name=".RequestBundleResponse.proto"></a>

### RequestBundleResponse.proto

RequestBundleResponse consists of four components. Below are five tables which describe each component in the message. The first component is the RequestBundleResponse itself. This component contains the four components in the inner layer of RequestBundleResponse.

<b>RequestBundleResponse</b>
    
| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Reply | oneof | Required | Signals if the request was a success or a failure |
| FileResource | Optional | message | Uri to get requested data along with dataset identifications |
| RequestFailure | Optional | message | Failure reason and description along with dataset identifications |
| Reason | enum | Optional | Multiple failure reasons to choose from |
    
<b>Reply</b>
    
| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Success | FileResource | optional | Successful request |
| Failure | RequestFailure | optional | Failed request |
    
<b>FileResource</b>
    
| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| UUID | repeated string | required | Unique dataset identification | Must be a valid Guid in string format |
| uri | string | requried | Uri to get requested data | Must be a valid Uri to data storage |
    
<b>RequestFailure</b>
    
| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| UUID | repeated string | required | Unique dataset identification | Must be a valid Guid in string format |
| reason | Reason | requried | Failure reason | Must be a constant from Reason |
| failureDescription | string | optional | Description of the failure | N/A |
    
<b>Reason</b>
    
| Field | Value |
| ----- | ---- |
| DatasetNotFound | 0 |
| DatasetNotAvailable | 1 |
| InternalError | 15 |
    
<hr>
<br>
    
<a name=".DequeueContract.proto"></a>

### DequeueContract.proto

| Field | Type | Label | Description | Limits |
| ----- | ---- | ----- | ----------- | ------ |
| dataAvailableIds | repeated string | required | One or multiple Id's to identify data to dequeue | None at the moment |
| recipient | string | required | Market Operator who sent the dequeue command | None at the moment |
