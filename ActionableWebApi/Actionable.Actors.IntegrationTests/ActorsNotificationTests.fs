module Actors_UserNotifications 

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

let system = Configuration.defaultConfig () |> System.create (sprintf "%s-%A" "ActionableSystem" (System.Guid.NewGuid ()))
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
let ``Create item, get noti., ack noti, no noti.`` () =  
    use waiter = new SignalWaiter ("crudWaiter", system)    
    actionable.UserNotificationsPersisterEventBroadcaster <! Subscribe waiter.Actor

    let title = "Hoobada Da Jubada Jistaliee"
    let description = "hiplity fublin"
    let description' = "hiplity dw mitibly fublin"
    let streamId = StreamId.create ()
    actionable.ActionItemAggregateProcessor 
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

    waiter.Wait 10.0

    let notifications = fetchUserNotifications "sampleuserid"
    let notifications = fetchUserNotifications "sampleuserid"
    let notification = 
        match notifications with 
        | Some ([n]) -> n
        | _ -> failwith <| "Expected notifications to have a value"
    
    actionable.UserNotificationsAggregateProcessor
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
    
