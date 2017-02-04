module Actionable.Domain.Infrastructure

open System

type FsGuidType = Id of Guid with
    static member create () = Id (Guid.NewGuid ())
    static member unbox (Id(id)) = id
    static member box id = Id(id)

type FsType<'T> = Val of 'T with
    static member box value = Val(value)
    static member unbox (Val(value)) = value

type StreamId = FsGuidType
type TransId = FsGuidType
type UserId = FsType<string>
//type DeviceId = FsType<string>
type Version = FsType<int16>

type IEventStore<'T> = 
    abstract member GetEvents: StreamId -> 'T list
    abstract member AppendEventAsync: StreamId -> 'T -> Async<unit>


[<AutoOpen>]
module Envelope =

    [<CLIMutable>]
    type Envelope<'T> = {
        Id: Guid
        UserId: UserId
//        DeviceId: DeviceId
        StreamId: StreamId
        TransactionId: TransId
        Version: Version
        Created: DateTimeOffset
        Item: 'T 
        }

    let envelope 
            userId 
//            deviceId 
            transId 
            id 
            version 
            created 
            item 
            streamId = {
        Id = id
        UserId = userId
//        DeviceId = deviceId
        StreamId = streamId
        Version = version
        Created = created
        Item = item
        TransactionId = transId 
        }

    let envelopWithDefaults 
            (userId:UserId) 
//            (deviceId:DeviceId) 
            (transId:TransId) 
            (streamId:StreamId) 
            (version:Version) item =
        streamId 
        |> envelope 
            userId 
//            deviceId 
            transId 
            (Guid.NewGuid()) 
            version 
            (DateTimeOffset.Now) 
            item

    let repackage<'a,'b> (func:'a->'b) (envelope:Envelope<'a>) ={
        Id = envelope.Id
        UserId = envelope.UserId
//        DeviceId = envelope.DeviceId
        StreamId = envelope.StreamId
        Version = envelope.Version
        Created = envelope.Created
        Item = func envelope.Item
        TransactionId = envelope.TransactionId 
        }
        
    let unpack envelope = envelope.Item

