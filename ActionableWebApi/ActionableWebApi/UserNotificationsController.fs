namespace ActionableWebApi

open System
open System.Net
open System.Net.Http
open System.Web.Http

open System.Security.Principal
open Microsoft.AspNet.Identity

open Actionable.Data

//open Composition
open Actionable.Domain.UserNotificationsModule
open Actionable.Domain.Infrastructure

open Actionable.Domain.Persistance.EventSourcing.UserNotificationEF
open System.Web.SessionState


type UserNotificationsQueryResponse () =
    [<DefaultValue>] val mutable Results : UserNotificationReadModel list option
    [<DefaultValue>] val mutable Time : string
                
type UserNotificationsController 
        ( post:Envelope<UserNotificationsCommand>->unit, 
          fetchNotifications:string->UserNotificationReadModel list option) =
    inherit ApiController ()
    
    let (|GuidPattern|_|) guid = 
        match Guid.TryParse guid with 
        | (true, id) -> Some(id)
        | _ -> Option.None
    interface IRequiresSessionState
//    member this.Post (item: AddActionItemRendition) =
//        // TODO Parse Int16.Parse(item.Status)
//        let envelope = 
//            match item.Id with 
//            | GuidPattern id ->
//                envelopWithDefaults 
//                    (UserId.box <| this.User.Identity.GetUserId()) 
//                    (TransId.create ()) 
//                    (StreamId.box id) 
//                    (Version.box 0s)
//                    (Update(item.Fields))
//            | _ ->
//                envelopWithDefaults 
//                    (UserId.box <| this.User.Identity.GetUserId()) 
//                    (TransId.create ()) 
//                    (StreamId.create ())
//                    (Version.box 0s)
//                    (Create(item.Fields))
//
//        post envelope
//        this.Request.CreateResponse(
//            HttpStatusCode.OK,
//            ResponseCode(
//                Message = "TODO: Respond with transaction Id",
//                Time = DateTimeOffset.Now.ToString("o")))
//
//    member this.Delete (item: DeleteActionItemRendition) =
//        let envelope =
//            envelopWithDefaults
//                (UserId.box <| this.User.Identity.GetUserId())        
//                (TransId.create ()) 
//                (StreamId.box <| Guid.Parse(item.ActionItemId))
//                (Version.box 0s)
//                (Delete)
//
//        post envelope
//        this.Request.CreateResponse(
//            HttpStatusCode.OK,
//            ResponseCode(
//                Message = "TODO: Respond with transaction Id",
//                Time = DateTimeOffset.Now.ToString("o")))


    member this.Get () =
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            UserNotificationsQueryResponse (
                Results = fetchNotifications (this.User.Identity.GetUserId()),
                Time = DateTimeOffset.Now.ToString("o")))
       
