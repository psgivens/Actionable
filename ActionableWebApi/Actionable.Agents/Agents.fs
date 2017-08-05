namespace Actionable.Agents

open System
open System.Reactive

open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure.Envelope

type Agent<'T> = Microsoft.FSharp.Control.MailboxProcessor<'T>

type ProcessDirective<'TMessage> = 
    | Execute of 'TMessage
    | Shutdown

type SupervisorDirective<'TMessage> =
    | Execute of 'TMessage
    | ProcessNext

type AgentCommunication<'TMessage> =
    | Message of 'TMessage
    | Error of System.Exception

type AggregateAgent<'TType, 'TState, 'TCommand, 'TEvent> 
        (uninitialized:'TState,
         store:IEventStore<Envelope<'TEvent>>, 
         buildState:'TState -> 'TEvent list -> 'TState,
         handle:'TState -> 'TCommand -> 'TEvent) =
         
    let eventSubject = new Subjects.Subject<Envelope<'TEvent>> ()
    let communicationAgent = new Agent<AgentCommunication<Envelope<'TEvent>>>(fun inbox ->
        let rec loop () =
            async {
                let! comm = inbox.Receive ()
                match comm with
                | Message (envelope) -> eventSubject.OnNext envelope
                | Error (ex) -> eventSubject.OnError ex
                return! loop ()
            }
        loop ())
    do communicationAgent.Start ()
    let agent = new Agent<Envelope<'TCommand>>(fun inbox ->
        let rec loop () =
            async {
                let! cmdenv = inbox.Receive ()

                async {
                    // Retrieve existing events
                    let events = 
                        store.GetEvents cmdenv.StreamId 
                        // Crudely remove concurrency errors
                        // TODO: Devise error correction mechanism
                        |> List.distinctBy (fun e -> e.Version)
                
                    let version = 
                        if events |> List.isEmpty then 0s
                        else events |> List.last |> (fun e -> Version.unbox e.Version)

                    // Build current state
                    let state = buildState uninitialized (events |> List.map unpack)

                    // 'handle' current cmd
                    let nevent = handle state cmdenv.Item

                    // publish new event
                    let envelope = 
                        envelopWithDefaults 
                            cmdenv.UserId 
                            cmdenv.TransactionId 
                            cmdenv.StreamId 
                            (Version.box (version + 1s)) 
                            nevent
                    do! store.AppendEventAsync cmdenv.StreamId envelope 

                    communicationAgent.Post <| Message envelope
                } |> Async.Start 

                return! loop ()
            }
        loop ())
    do agent.Start ()
    member this.Post envelope =
        agent.Post envelope
    interface IDisposable with
        member this.Dispose () = 
            eventSubject.Dispose ()
    interface IObservable<Envelope<'TEvent>> with
        member this.Subscribe observer = 
            eventSubject.Subscribe observer

open Actionable.Data

type PersistingAgent<'TType, 'TState, 'TCommand, 'TEvent> 
        (uninitialized:'TState,
         store:IEventStore<Envelope<'TEvent>>, 
         buildState:'TState -> 'TEvent list -> 'TState,
         persist:UserId -> StreamId -> 'TState -> unit) =

    let eventSubject = new Subjects.Subject<Envelope<'TEvent>> ()
    let communicationAgent = new Agent<AgentCommunication<Envelope<'TEvent>>>(fun inbox ->
        let rec loop () =
            async {
                let! comm = inbox.Receive ()
                match comm with
                | Message (envelope) -> eventSubject.OnNext envelope
                | Error (ex) ->         eventSubject.OnError ex
                return! loop ()
            }
        loop ())
    do communicationAgent.Start ()

    let createCsp () = 
        let csp = new Agent<Envelope<'TEvent>>(fun inbox ->
            let rec iloop () = 
                async {
                    let! envelope = inbox.Receive ()
                    try
                        // Retrieve existing events
                        let events = 
                            store.GetEvents envelope.StreamId
                            // Crudely remove concurrency errors
                            |> List.distinctBy (fun e -> e.Version)
                
                        // Build current state
                        let state = buildState uninitialized (events |> List.map unpack)

                        persist envelope.UserId envelope.StreamId state
                    with
                        | ex -> communicationAgent.Post <| Error ex
                    return! iloop ()
                }
            iloop ())
        csp.Start ()
        csp

    let agent = new Agent<Envelope<'TEvent>>(fun inbox ->
        let rec loop (cspMap:Map<Guid,Agent<Envelope<'TEvent>>>) =
            async {
                let! evtenv = inbox.Receive ()

                // Get the CSP
                let (cspMap', csp) = 
                    match cspMap.TryFind <| StreamId.unbox evtenv.StreamId with
                    | Option.Some (csp') -> (cspMap, csp')
                    | Option.None -> 
                        let csp' = createCsp ()
                        (cspMap.Add (StreamId.unbox evtenv.StreamId, csp'), csp')

                // Post to it 
                csp.Post evtenv

                // Continue the loop
                return! loop <| cspMap'
            }
        loop Map.empty<Guid, Agent<Envelope<'TEvent>>>)  
    do agent.Start ()

    member this.Post envelope =
        agent.Post envelope
    interface IDisposable with
        member this.Dispose () = 
            eventSubject.Dispose ()
    interface IObservable<Envelope<'TEvent>> with
        member this.Subscribe observer = 
            eventSubject.Subscribe observer

