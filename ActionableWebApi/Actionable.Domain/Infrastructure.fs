module Actionable.Domain.Infrastructure

open System

type StreamId = StreamId of Guid
    with static member create () = StreamId (Guid.NewGuid ())
type UserId = UserId of String
type DeviceId = DeviceId of string
type TransId = TransId of Guid
    with static member create () = TransId (Guid.NewGuid ())
type Version = Version of int16 

type IEventStore<'T> = 
    abstract member GetEvents: StreamId -> 'T list
    abstract member AppendEventAsync: StreamId -> 'T -> Async<unit>


[<AutoOpen>]
module Envelope =

    [<CLIMutable>]
    type Envelope<'T> = {
        Id: Guid
        UserId: UserId
        DeviceId: DeviceId
        StreamId: StreamId
        TransactionId: TransId
        Version: Version
        Created: DateTimeOffset
        Item: 'T }
    let envelope userId deviceId transId id version created item streamId = {
        Id = id
        UserId = userId
        DeviceId = deviceId
        StreamId = streamId
        Version = version
        Created = created
        Item = item
        TransactionId = transId }
    let envelopWithDefaults (userId:UserId) (deviceId:DeviceId) (transId:TransId) (streamId:StreamId) (version:Version) item =
        streamId |> envelope userId deviceId transId (Guid.NewGuid()) version (DateTimeOffset.Now) item
    let repackage<'a,'b> (func:'a->'b) (envelope:Envelope<'a>) ={
        Id = envelope.Id
        UserId = envelope.UserId
        DeviceId = envelope.DeviceId
        StreamId = envelope.StreamId
        Version = envelope.Version
        Created = envelope.Created
        Item = func envelope.Item
        TransactionId = envelope.TransactionId }
        
    let unpack envelope = envelope.Item

