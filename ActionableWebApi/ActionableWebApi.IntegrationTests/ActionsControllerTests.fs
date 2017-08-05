namespace ActionableWebApi.IntegrationTests

open Xunit
open ActionableWebApi
open System.Web.Http.Dispatcher

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Composition
open Actionable.Actors.IntegrationTests.Perisistance

open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
open Actionable.Domain.UserNotificationsModule

open Actionable.Domain.ClientCommands
//open Actionable.Domain.Persistance.EventSourcing.ActionItemEF

module HttpTest = 
    let unpack<'TType> (result:System.Net.Http.HttpResponseMessage) :'TType = 
        let content = result.Content :?> System.Net.Http.ObjectContent<'TType>
        content.Value :?> 'TType

    let system = Configuration.defaultConfig () |> System.create (sprintf "%s-%A" "ActionableSystem" (System.Guid.NewGuid ()))
    let testUserStreamId = StreamId.create ()
    let getUserNotificationStreamId userId = testUserStreamId
    let inMemoryPersistence = InMemoryPersistence ()
    let actionable = 
        composeSystem 
            (system, 
             MemoryStore<ActionItemEvent> (), 
             MemoryStore<UserNotificationsEvent> (),
             getUserNotificationStreamId,
             inMemoryPersistence.PersistActionItem,
             inMemoryPersistence.PersistUserNotification
             )

// 
// Some help was found through this: http://stackoverflow.com/questions/22762338/how-do-i-mock-user-identity-getuserid
type TestUser () =
    let claim = System.Security.Claims.Claim ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", 
                                              "dc85790d-2678-407b-800a-5690c0004497")

    interface System.Security.Principal.IPrincipal with
        member this.Identity 
            with get () = System.Security.Claims.ClaimsIdentity([claim]) :> System.Security.Principal.IIdentity
        member this.IsInRole roleName = true

type ``Web - Integration Tests``() = 
    let testUser = TestUser()
    let compositRoot = 
        CompositRoot 
            (HttpTest.actionable, 
             HttpTest.getUserNotificationStreamId,
             HttpTest.inMemoryPersistence.GetActionItem, 
             HttpTest.inMemoryPersistence.GetActionItems, 
             HttpTest.inMemoryPersistence.GetUserNotifications)
    let requestMessage = new System.Net.Http.HttpRequestMessage ()
    let actionController = 
        (compositRoot :> IHttpControllerActivator).Create (
            (requestMessage), 
            (null :> System.Web.Http.Controllers.HttpControllerDescriptor), 
            typedefof<ActionsController>) :?> ActionsController
    do actionController.Request <- requestMessage
    do actionController.Configuration <- new System.Web.Http.HttpConfiguration ()
    do actionController.User <- testUser
    let notificationsController = 
        (compositRoot :> IHttpControllerActivator).Create (
            (requestMessage), 
            (null :> System.Web.Http.Controllers.HttpControllerDescriptor), 
            typedefof<UserNotificationsController>) :?> UserNotificationsController
    let requestMessage = new System.Net.Http.HttpRequestMessage ()
    do notificationsController.Request <- requestMessage
    do notificationsController.Configuration <- new System.Web.Http.HttpConfiguration ()
    do notificationsController.User <- testUser
            //((ActionableWebApi.ResponseToQuery)().Value).Results[0].Fields

//    [<Fact>]
//    member this.``Retrieve all items for the authenticated user`` () =
//        let result = actionController.Get () 
//        let response = HttpTest.unpack<ActionableWebApi.ResponseToQuery> result
//        Assert.True (response.Results.Length > 0)
//        Assert.True true
//        //((ActionableWebApi.ResponseToQuery)((System.Net.Http.ObjectContent<ActionableWebApi.ResponseToQuery>)result.Content).Value).Results[0].Fields

    [<Fact>]
    member this.``Test Discoverable`` () =
        Assert.True true

    [<Fact>]
    member this.``Create item, get noti, ack noit, no noti`` () =
        (***** Create item *****)
        let title = "Hibidy jibity " + (System.Guid.NewGuid ()).ToString()
        let description = "have fun"
        let description' = "have the most fun"
        ignore <| actionController.Post {
            Id = ""
            Fields = [("actionable.title", title);("actionable.description",description)] |> Map.ofList
            Date = System.DateTimeOffset.Now.ToString ()
        }
        System.Threading.Thread.Sleep 2000       
        
        (***** Get notification ****)
        let nresult = notificationsController.Get ()
        let nresponse = HttpTest.unpack<ActionableWebApi.UserNotificationsQueryResponse> nresult
        let notification = nresponse.Results.Value.Head
        let payload = deserializeClientCommand <| notification.Message
        let actionItemId = (payload.data :?> ActionItemUpdated).Id

        Assert.Equal (0, notification.Status)

        (***** Acknowledge notification *****)
        let npostResult = 
            notificationsController.Post 
                { UserNotificationIdRendition.UserNotificationId = notification.Id.ToString ()}
            |> fun task -> task.Result

        System.Threading.Thread.Sleep 2000  
        
        (***** Verify notification acknowledgement *****)
        let nresult2 = notificationsController.Get ()
        let nresponse2 = HttpTest.unpack<ActionableWebApi.UserNotificationsQueryResponse> nresult2
        let notification2 = nresponse2.Results.Value.Head

        Assert.Equal (1, notification2.Status)

