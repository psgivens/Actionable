module Actionable.Actors.Aggregates

open Akka.Actor
open Akka.FSharp
open Actionable.Actors.Infrastructure

open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure.Envelope

type AggregateAgent<'TState, 'TCommand, 'TEvent> =
    static member Create
        (eventSubject:IActorRef,
         invalidMessageSubject:IActorRef,
         uninitialized:'TState,
         store:IEventStore<Envelope<'TEvent>>, 
         buildState:'TState -> 'TEvent list -> 'TState,
         handle:'TState -> 'TCommand -> 'TEvent) =         
    
        let processMessage (mailbox:Actor<Envelope<'TCommand>>) cmdenv=
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
            
            try
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

                store.AppendEvent cmdenv.StreamId envelope 
                eventSubject <! envelope

            with
            | :? InvalidEvent as ex -> invalidMessageSubject <! ex
            | :? InvalidCommand as ex -> invalidMessageSubject <! ex
        actorOf2 processMessage

