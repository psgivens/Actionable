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
          getUserNotificationStreamId:UserId -> StreamId,
          getNotifications:string->UserNotificationReadModel list option) =
    inherit ApiController ()
    
    let (|GuidPattern|_|) guid = 
        match Guid.TryParse guid with 
        | (true, id) -> Some(id)
        | _ -> Option.None
    interface IRequiresSessionState

    member this.Post (item: UserNotificationIdRendition) =
        async {
            let itemId = item.GetId ()
            let userId = this.User.Identity.GetUserId() |> UserId.box
            use context = new ActionableDbContext()
            let! streamId = async {
                return getUserNotificationStreamId userId }
            
            (UserNotificationsCommand.AcknowledgeMessage itemId)
            |> envelopWithDefaults
                (userId)        
                (TransId.create ()) 
                (streamId)
                (Version.box 0s)                    
            |> post

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                ResponseCode(
                    Message = "TODO: Respond with transaction Id",
                    Time = DateTimeOffset.Now.ToString("o")))
        } |> Async.StartAsTask

    member this.Get () =
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            UserNotificationsQueryResponse (
                Results = getNotifications (this.User.Identity.GetUserId()),
                Time = DateTimeOffset.Now.ToString("o")))
       
