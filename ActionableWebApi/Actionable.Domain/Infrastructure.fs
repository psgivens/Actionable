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
        StreamId: Guid
        TransactionId: Guid
        Version: Int16
        Created: DateTimeOffset
        Item: 'T }
    let envelope userId transId id version created item streamId = {
        Id = id
        UserId = userId
        StreamId = streamId
        Version = version
        Created = created
        Item = item
        TransactionId = transId }
    let envelopWithDefaults userId transId streamId version item =
        streamId |> envelope userId transId (Guid.NewGuid()) version (DateTimeOffset.Now) item
    let unpack envelope = envelope.Item

