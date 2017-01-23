module Actionable.Domain.Infrastructure

open System
type StreamId = Guid

type IEventStore<'T> = 
    abstract member GetEvents: StreamId -> 'T list
    abstract member AppendEventAsync: StreamId -> 'T -> Async<unit>

[<AutoOpen>]
module Envelope =
    [<CLIMutable>]
    type Envelope<'T> = {
        Id: Guid
        UserId: String
        DeviceId: String
        StreamId: Guid
        TransactionId: Guid
        Version: Int16
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
    let envelopWithDefaults userId deviceId transId streamId version item =
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

