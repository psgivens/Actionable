﻿module Actors_ActionItems.UnitTests2

open Xunit

open Akka
open Akka.Actor
open Akka.FSharp

open System

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.UserNotificationsModule
open Actionable.Domain.ClientCommands

open Actionable.Actors 
open Actionable.Actors.Infrastructure
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

open Actionable.Actors.UnitTests.Perisistance
open Actionable.Actors.Composition
//
//let system = Configuration.defaultConfig () |> System.create (sprintf "%s-%A" "ActionableSystem" (System.Guid.NewGuid ()))
//let testUserStreamId = StreamId.create ()
//let getUserNotificationStreamId userId = testUserStreamId
//let inMemoryPersistence = InMemoryPersistence ()
//let actionable = 
//    composeSystem 
//        (system, 
//         MemoryStore<ActionItemEvent> (), 
//         MemoryStore<UserNotificationsEvent> (),
//         getUserNotificationStreamId,
//         inMemoryPersistence.PersistActionItem,
//         inMemoryPersistence.PersistUserNotification
//         )
//          // Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)
//
//open Actionable.Actors.Infrastructure
//
//type SignalWaiter (name, system) = 
//    let signal = new System.Threading.AutoResetEvent false    
//
//    let actor = 
//        actorOf (fun msg -> signal.Set () |> ignore) 
//        |> spawn system name
//
//    member this.Actor 
//        with get () = actor
//
//    member this.Wait seconds = 
//        System.TimeSpan.FromSeconds seconds 
//        |> signal.WaitOne 
//        |> Assert.True
//        
//    interface System.IDisposable  with 
//        member x.Dispose() = signal.Dispose ()
//

//let processorRef = spawn system "processor" processor

open Akka.TestKit.Xunit2
open Actionable.Actors.Persistance
open Actionable.Actors.Aggregates
open Microsoft.FSharp.Linq

type ActionItemActorSpecs () =
    inherit TestKit ()

    let processor (mailbox: Actor<_>) = 
        let rec loop () = actor {        
            let! name = mailbox.Receive ()
            printfn "message %s" name
            (mailbox.Sender () <! sprintf "Hello %s" name) |> ignore
            return! loop ()
        }
        loop ()
      
    let echoProcessor (mailbox: Actor<_>) = 
        let rec loop () = actor {        
            let! name = mailbox.Receive ()
            printfn "message %s" name
            (mailbox.Sender () <! sprintf "Hello %s" name) |> ignore
            return! loop ()
        }
        loop ()

    [<Fact>]
    member this.``Sanity test`` () =
        let processorRef = spawn this.Sys "processor" processor
        processorRef <! "Bob"
        let result = this.ExpectMsgFrom processorRef
        Xunit.Assert.Equal ("Hello Bob", result)
        ()

    [<Fact>]    
    member this.``Create, retrieve, update, and delete an item`` () =  
        let evtProbe = this.CreateTestProbe "eventProbe"
        let errProbe = this.CreateTestProbe "errorProbe"
        let inMemoryPersistence = InMemoryPersistence ()
        let memoryStore = MemoryStore<ActionItemEvent> ()
        
        let actionItemPersistingProcessor =             
            (PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                evtProbe,
                errProbe,
                ActionItemState.DoesNotExist,
                memoryStore,
                ActionItemModule.buildState,
                inMemoryPersistence.PersistActionItem))
            |> spawn this.Sys "actionItemPersistor"            

        let actionItemAggregateProcessor =             
            (AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                actionItemPersistingProcessor,
                errProbe,
                ActionItemState.DoesNotExist,
                memoryStore,
                ActionItemModule.buildState,
                ActionItemModule.handle))
            |> spawn this.Sys "actionItemAggregate" 

        let title = "Hoobada Da Jubada Jistaliee"
        let description = "hiplity fublin"
        let description' = "hiplity dw mitibly fublin"
        let streamId = StreamId.create ()
        actionItemAggregateProcessor
            <! envelopWithDefaults 
                (UserId.box "sampleuserid")
                (TransId.create ())
                (streamId) 
                (Version.box 0s) 
                (("sampleuserid", StreamId.unbox streamId, 
                  ["actionable.title",title;
                    "actionable.description", description] 
                    |> Map.ofList)
                 |> ActionItemCommand.Create)

        let result = 
            evtProbe.ExpectMsgFrom<Envelope<ActionItemEvent>> 
                (   actionItemPersistingProcessor, 
                    TimeSpan.FromSeconds 3.0 |> Nullable,
                    "Expecting a created event") 
        let item = 
            match result.Item with
            | ActionItemEvent.Created item -> item
            | _ -> failwith "wrong event type"
        
        Assert.Equal (item.Fields.["actionable.description"], description)

        actionItemAggregateProcessor
            <! envelopWithDefaults 
                (UserId.box "sampleuserid")
                (TransId.create ())
                (streamId) 
                (Version.box 1s) 
                (["actionable.title",title;
                    "actionable.description", description'] 
                 |> Map.ofList
                 |> ActionItemCommand.Update)

        ignore <|
            evtProbe.ExpectMsgFrom<Envelope<ActionItemEvent>> 
                (   actionItemPersistingProcessor, 
                    TimeSpan.FromSeconds 3.0 |> Nullable,
                    "Expecting an updated event") 

        let results' = inMemoryPersistence.GetActionItems "sampleuserid"
        let item' = 
            match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
                with
                | None -> failwith "Could not find item"
                | Some item' -> item'

        Assert.Equal (item.Id, item'.Id)
        Assert.Equal (item'.Fields.["actionable.description"], description')

        actionItemAggregateProcessor
            <! envelopWithDefaults 
                (UserId.box "sampleuserid")
                (TransId.create ())
                (streamId) 
                (Version.box 1s) 
                (ActionItemCommand.Delete)
                
        ignore <|
            evtProbe.ExpectMsgFrom<Envelope<ActionItemEvent>> 
                (   actionItemPersistingProcessor, 
                    System.Nullable<System.TimeSpan> <| System.TimeSpan.FromSeconds 3.0,
                    "Expecting a deleted event") 

        let results'' = inMemoryPersistence.GetActionItems "sampleuserid"
        let item''' = results'' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
    
        Assert.Equal (item''', None)



    [<Fact>]    
    member this.``Notify: Create, retrieve, update, and delete an item`` () =  
        let evtProbe = this.CreateTestProbe "eventProbe"
        let evtProbe2 = this.CreateTestProbe "eventProbe2"
        let errProbe = this.CreateTestProbe "errorProbe"
        let inMemoryPersistence = InMemoryPersistence ()
        let actionItemMemoryStory = MemoryStore<ActionItemEvent> ()
        let notificationMemoryStory = MemoryStore<UserNotificationsEvent> ()
        let actionItemEventSubject, actionItemEventPoster = 
            subject this.Sys "actionItemEventBroadcaster" 
        let testUserStreamId = StreamId.create ()
        let getUserNotificationStreamId userId = testUserStreamId

        (* 
        Given: we have a user-notification-aggregater, an action-item-persistor,
        an action-item-to-user-notification-translator, and an action-item-aggregator,
        wired together.
        *)
                
        let actionItemPersistor =             
            (PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                evtProbe,
                errProbe,
                ActionItemState.DoesNotExist,
                actionItemMemoryStory,
                ActionItemModule.buildState,
                inMemoryPersistence.PersistActionItem))
            |> spawn this.Sys "actionItemPersistor"            
        actionItemEventSubject <! Subscribe actionItemPersistor 
        
        let userNotificationsAggregater =
            AggregateAgent<UserNotificationsState, UserNotificationsCommand, UserNotificationsEvent>.Create (
                evtProbe2,
                errProbe,
                UserNotificationsState.DoesNotExist,                
                notificationMemoryStory,
                UserNotificationsModule.buildState, 
                UserNotificationsModule.handle)
            |> spawn this.Sys "userNotificationsAggregater"
               
        let actionItemToNotificationTranslator =
            spawn this.Sys "actionItemToNotificationTranslator" (fun (mailbox:Actor<Envelope<ActionItemEvent>>) ->
                let rec loop () = actor {
                    let! envelope = mailbox.Receive ()
                    let clientCmd = serializeClientCommand { 
                        Actionable.Domain.ClientCommands.ActionItemUpdated.Id = envelope.StreamId |> StreamId.unbox }
                    let streamId = getUserNotificationStreamId envelope.UserId
                    let cmd = 
                        envelope
                        |> repackage streamId (fun actionItemEvent ->  
                            (UserId.unbox envelope.UserId, 0, clientCmd)
                            |> UserNotificationsCommand.AppendMessage)
                    cmd |> userNotificationsAggregater.Tell
                   
                    return! loop () }
                loop ())        
        actionItemEventSubject <! Subscribe actionItemToNotificationTranslator
                        
        let actionItemAggregator =             
            (AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                actionItemEventPoster,
                errProbe,
                ActionItemState.DoesNotExist,
                actionItemMemoryStory,
                ActionItemModule.buildState,
                ActionItemModule.handle))
            |> spawn this.Sys "actionItemAggregator" 


        (* 
        When: we send an action-item command to the action-item-aggregator
        *)
        let title = "Hoobada Da Jubada Jistaliee"
        let description = "hiplity fublin"
        let description' = "hiplity dw mitibly fublin"
        let streamId = StreamId.create ()
        actionItemAggregator
            <! envelopWithDefaults 
                (UserId.box "sampleuserid")
                (TransId.create ())
                (streamId) 
                (Version.box 0s) 
                (("sampleuserid", StreamId.unbox streamId, 
                  ["actionable.title",title;
                    "actionable.description", description] 
                    |> Map.ofList)
                 |> ActionItemCommand.Create)

        (* 
        Then: we receive a MessageCreated notification event from the 
        user-notifications-aggregator
        *)
        let resultx = 
            evtProbe2.ExpectMsgFrom<Envelope<UserNotificationsEvent>>
                (   userNotificationsAggregater,
                    TimeSpan.FromSeconds 3.0 |> Nullable,
                    "Expecting a notification event")

        let itemx = 
            match resultx.Item with
            | MessageCreated (_, _, _, _) -> resultx.Item
            | _ -> failwith "incorrect return"





        let result = 
            evtProbe.ExpectMsgFrom<Envelope<ActionItemEvent>> 
                (   actionItemPersistor, 
                    TimeSpan.FromSeconds 3.0 |> Nullable,
                    "Expecting a created event") 
        let item = 
            match result.Item with
            | ActionItemEvent.Created item -> item
            | _ -> failwith "wrong event type"
        
        Assert.Equal (item.Fields.["actionable.description"], description)

        actionItemAggregator
            <! envelopWithDefaults 
                (UserId.box "sampleuserid")
                (TransId.create ())
                (streamId) 
                (Version.box 1s) 
                (["actionable.title",title;
                    "actionable.description", description'] 
                 |> Map.ofList
                 |> ActionItemCommand.Update)

        ignore <|
            evtProbe.ExpectMsgFrom<Envelope<ActionItemEvent>> 
                (   actionItemPersistor, 
                    TimeSpan.FromSeconds 3.0 |> Nullable,
                    "Expecting an updated event") 

        let results' = inMemoryPersistence.GetActionItems "sampleuserid"
        let item' = 
            match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
                with
                | None -> failwith "Could not find item"
                | Some item' -> item'

        Assert.Equal (item.Id, item'.Id)
        Assert.Equal (item'.Fields.["actionable.description"], description')

        actionItemAggregator
            <! envelopWithDefaults 
                (UserId.box "sampleuserid")
                (TransId.create ())
                (streamId) 
                (Version.box 1s) 
                (ActionItemCommand.Delete)
                
        ignore <|
            evtProbe.ExpectMsgFrom<Envelope<ActionItemEvent>> 
                (   actionItemPersistor, 
                    System.Nullable<System.TimeSpan> <| System.TimeSpan.FromSeconds 3.0,
                    "Expecting a deleted event") 

        let results'' = inMemoryPersistence.GetActionItems "sampleuserid"
        let item''' = results'' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
    
        Assert.Equal (item''', None)
