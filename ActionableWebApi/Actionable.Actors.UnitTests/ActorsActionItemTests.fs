module Actors_ActionItems.UnitTests2

open Xunit

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.UserNotificationsModule

open Actionable.Actors 
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
      

    [<Fact>]
    member this.``Sanity test`` () =
        let processorRef = spawn this.Sys "processor" processor
        processorRef <! "Bob"
        let result = this.ExpectMsgFrom processorRef
        Xunit.Assert.Equal ("Hello Bob", result)
        ()

    [<Fact>]
    member this.``booya`` () = 
        let evtBC = spawn this.Sys "eventbroadcaster" processor
        let errBC = spawn this.Sys "errorbroadcaster" processor
        let inMemoryPersistence = InMemoryPersistence ()
        let f2 = 
            PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                evtBC,
                errBC,
                ActionItemState.DoesNotExist,
                MemoryStore<ActionItemEvent> (),
                ActionItemModule.buildState,
                inMemoryPersistence.PersistActionItem)
        let act = spawn this.Sys "actorfan" f2
        act.Tell ("Bob")
        ()

//    [<Fact>]
//    member this.``Create, retrieve, update, and delete an item`` () =  
//        use waiter = new SignalWaiter ("crudWaiter", system)    
//        actionable.ActionItemPersisterEventBroadcaster <! Subscribe waiter.Actor
//
//        let title = "Hoobada Da Jubada Jistaliee"
//        let description = "hiplity fublin"
//        let description' = "hiplity dw mitibly fublin"
//        let streamId = StreamId.create ()
//        actionable.ActionItemAggregateProcessor 
//            <! envelopWithDefaults 
//                (UserId.box "sampleuserid")
//                (TransId.create ())
//                (streamId) 
//                (Version.box 0s) 
//                (("sampleuserid", StreamId.unbox streamId, 
//                  ["actionable.title",title;
//                    "actionable.description", description] 
//                    |> Map.ofList)
//                 |> ActionItemCommand.Create)
//
//        waiter.Wait 60.0
//
//        let results = inMemoryPersistence.GetActionItems "sampleuserid"
//        let item = 
//            match results |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
//                with
//                    | None -> failwith <| sprintf "item '%s' was not found" title
//                    | Some item -> item
//    
//        Assert.Equal (item.Fields.["actionable.description"], description)
//
//        actionable.ActionItemAggregateProcessor 
//            <! envelopWithDefaults 
//                (UserId.box "sampleuserid")
//                (TransId.create ())
//                (streamId) 
//                (Version.box 1s) 
//                (["actionable.title",title;
//                    "actionable.description", description'] 
//                    |> Map.ofList
//                    |> ActionItemCommand.Update)
//
//        waiter.Wait 60.0
//        let results' = inMemoryPersistence.GetActionItems "sampleuserid"
//        let item' = 
//            match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
//                with
//                | None -> failwith "Could not find item"
//                | Some item' -> item'
//
//        Assert.Equal (item.Id, item'.Id)
//        Assert.Equal (item'.Fields.["actionable.description"], description')
//
//        actionable.ActionItemAggregateProcessor 
//            <! envelopWithDefaults 
//                (UserId.box "sampleuserid")
//                (TransId.create ())
//                (streamId) 
//                (Version.box 1s) 
//                (ActionItemCommand.Delete)
//                
//        waiter.Wait 60.0
//        let results'' = inMemoryPersistence.GetActionItems "sampleuserid"
//        let item''' = results'' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
//    
//        Assert.Equal (item''', None)
//
//
