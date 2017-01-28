module Actionable.Actors.Persistance

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure
open Actionable.Data

type PersistingAgent<'TState, 'TCommand, 'TEvent> =
    static member Create 
        (eventSubject:IActorRef,
         errorSubject:IActorRef,
         uninitialized:'TState,
         store:IEventStore<Envelope<'TEvent>>, 
         buildState:'TState -> 'TEvent list -> 'TState,
         persist:UserId -> StreamId -> 'TState -> Async<unit>) =

        let persistEntity (mailbox:Actor<Envelope<'TEvent>>) envelope = 
            try
                // Retrieve existing events
                let events = 
                    store.GetEvents envelope.StreamId
                    // Crudely remove concurrency errors
                    |> List.distinctBy (fun e -> e.Version)
                
                // Build current state
                let state = buildState uninitialized (events |> List.map unpack)

                mailbox.Self <!| persist envelope.UserId envelope.StreamId state

            with
                | ex -> 
                    errorSubject.Tell <| Msg ex
        actorOf2 persistEntity
        


