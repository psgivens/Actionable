module Actionable.Actors.IntegrationTests.NotificationActorsTests

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

open InMemoryPersistance

let system = Configuration.defaultConfig () |> System.create "ActionableSystem"
let actionable = 
    composeSystem 
        (system, 
         MemoryStore<ActionItemEvent> (), 
         MemoryStore<UserNotificationsEvent> (),
         persistActionItem,
         persistUserNotification)
         // Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)

open Actionable.Actors.Infrastructure

type SignalWaiter (name, system) = 
    let signal = new System.Threading.AutoResetEvent false    

    let actor = 
        actorOf (fun msg -> signal.Set () |> ignore) 
        |> spawn system name

    member this.Actor 
        with get () = actor

    member this.Wait seconds = 
        System.TimeSpan.FromSeconds seconds 
        |> signal.WaitOne 
        |> Assert.True
        
    interface System.IDisposable  with 
        member x.Dispose() = signal.Dispose ()
        
[<Fact>]
let ``Create, retrieve, update, and delete an item`` () =  
    use waiter = new SignalWaiter ("crudWaiter", system)    
    actionable.userNotificationsPersisterEventBroadcaster <! Subscribe waiter.Actor

    let title = "Hoobada Da Jubada Jistaliee"
    let description = "hiplity fublin"
    let description' = "hiplity dw mitibly fublin"
    let streamId = StreamId.create ()
    actionable.actionItemAggregateProcessor 
        <! envelopWithDefaults 
            (UserId.box "sampleuserid")
            (TransId.create ())
            (streamId) 
            (Version.box 0s) 
            (["actionable.title",title;
                "actionable.description", description] 
                |> Map.ofList
                |> ActionItemCommand.Create)

    waiter.Wait 10.0

    let notifications = fetchUserNotifications "sampleuserid"
    let notifications = fetchUserNotifications "sampleuserid"
    let notification = 
        match notifications with 
        | Some ([n]) -> n
        | _ -> failwith <| "Expected notifications to have a value"
    
    actionable.userNotificationsAggregateProcessor
        <! envelopWithDefaults 
            (UserId.box "sampleuserid")
            (TransId.create ())
            (streamId) 
            (Version.box 0s) 
            (UserNotificationsCommand.AcknowledgeMessage(notification.Id))
    
    waiter.Wait 10.0

    let notifications = fetchUserNotifications "sampleuserid"
    match notifications with 
    | Some ([n]) when n.Status = 1 -> () 
    | _ -> failwith <| "Expected notifications to be cleared"
    
//    actionable.actionItemAggregateProcessor 
//        <! envelopWithDefaults 
//            (UserId.box "sampleuserid")
//            (TransId.create ())
//            (streamId) 
//            (Version.box 1s) 
//            (["actionable.title",title;
//                "actionable.description", description'] 
//                |> Map.ofList
//                |> ActionItemCommand.Update)
//
//    waiter.Wait 60.0
//    let results' = fetchActionItem "sampleuserid"
//    let item' = 
//        match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
//            with
//            | None -> failwith "Could not find item"
//            | Some item' -> item'
//
//    Assert.Equal (item.Id, item'.Id)
//    Assert.Equal (item'.Fields.["actionable.description"], description')
//
//    actionable.actionItemAggregateProcessor 
//        <! envelopWithDefaults 
//            (UserId.box "sampleuserid")
//            (TransId.create ())
//            (streamId) 
//            (Version.box 1s) 
//            (ActionItemCommand.Delete)
//                
//    waiter.Wait 60.0
//    let results'' = fetchActionItem "sampleuserid"
//    let item''' = results'' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
//    
//    Assert.Equal (item''', None)



