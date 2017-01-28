﻿module Actionable.Actors.Aggregates

open Akka.Actor
open Akka.FSharp
open Actionable.Actors.Infrastructure

open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure.Envelope

type AggregateAgent<'TState, 'TCommand, 'TEvent> =
    static member Create
        (eventSubject:IActorRef,
         invalidCommandSubject:IActorRef,
         uninitialized:'TState,
         store:IEventStore<Envelope<'TEvent>>, 
         buildState:'TState -> 'TEvent list -> 'TState,
         handle:'TState*'TCommand -> 'TEvent) =         
    
        let processMessage (mailbox:Actor<Envelope<'TCommand>>) cmdenv=
            let events = 
                store.GetEvents cmdenv.StreamId 
                // Crudely remove concurrency errors
                // TODO: Devise error correction mechanism
                |> List.distinctBy (fun e -> e.Version)
                
            let version = 
                if events |> List.isEmpty then 0s
                else events |> List.last |> (fun e -> unbox e.Version)

            // Build current state
            let state = buildState uninitialized (events |> List.map unpack)
            
            try
                // 'handle' current cmd
                let nevent = handle (state, cmdenv.Item)

                // publish new event
                let envelope = 
                    envelopWithDefaults 
                        cmdenv.UserId 
                        cmdenv.DeviceId 
                        cmdenv.TransactionId 
                        cmdenv.StreamId 
                        (Version (version + 1s)) 
                        nevent
                mailbox.Self <!| store.AppendEventAsync cmdenv.StreamId envelope 

                eventSubject.Tell <| Msg envelope
            with
            | :? InvalidEvent as ex -> invalidCommandSubject <! Msg ex
            | :? InvalidCommand as ex -> invalidCommandSubject <! Msg ex
        actorOf2 processMessage
