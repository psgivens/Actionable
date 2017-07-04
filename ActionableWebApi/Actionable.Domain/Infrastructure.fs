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
        StreamId: StreamId
        TransactionId: TransId
        Version: Version
        Created: DateTimeOffset
        Item: 'T 
        }

    let envelope 
            userId 
            transId 
            id 
            version 
            created 
            item 
            streamId = {
        Id = id
        UserId = userId
        StreamId = streamId
        Version = version
        Created = created
        Item = item
        TransactionId = transId 
        }
        
    let envelopWithDefaults 
            (userId:UserId) 
            (transId:TransId) 
            (streamId:StreamId) 
            (version:Version) item =
        streamId 
        |> envelope 
            userId 
            transId 
            (Guid.NewGuid()) 
            version 
            (DateTimeOffset.Now) 
            item

    let repackage<'a,'b> streamId (func:'a->'b) (envelope:Envelope<'a>) ={
        Id = envelope.Id
        UserId = envelope.UserId
        StreamId = streamId
        Version = envelope.Version
        Created = envelope.Created
        Item = func envelope.Item
        TransactionId = envelope.TransactionId 
        }
        
    let unpack envelope = envelope.Item

