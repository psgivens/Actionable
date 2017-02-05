namespace ActionableWebApi.IntegrationTests

open Xunit
open ActionableWebApi
open System.Web.Http.Dispatcher

//[<Fact>]
//let ``I can list the items in the database`` () =
//    Assert.True true

module HttpTest = 
    let unpack<'TType> (result:System.Net.Http.HttpResponseMessage) :'TType = 
        let content = result.Content :?> System.Net.Http.ObjectContent<'TType>
        content.Value :?> 'TType

// 
// Some help was found through this: http://stackoverflow.com/questions/22762338/how-do-i-mock-user-identity-getuserid
type TestUser () =
    let claim = System.Security.Claims.Claim ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", 
                                              "dc85790d-2678-407b-800a-5690c0004497")

    interface System.Security.Principal.IPrincipal with
        member this.Identity 
            with get () = System.Security.Claims.ClaimsIdentity([claim]) :> System.Security.Principal.IIdentity
        member this.IsInRole roleName = true

type ActionsControllerTests() = 
    let compositRoot = CompositRoot ()
    let requestMessage = new System.Net.Http.HttpRequestMessage ()
    let actionController = (compositRoot :> IHttpControllerActivator).Create (
        (requestMessage), 
        (null :> System.Web.Http.Controllers.HttpControllerDescriptor), 
        typedefof<ActionsController>) :?> ActionsController
    do actionController.Request <- requestMessage
    do actionController.Configuration <- new System.Web.Http.HttpConfiguration ()
    do actionController.User <- TestUser ()
    
            //((ActionableWebApi.ResponseToQuery)().Value).Results[0].Fields

    [<Fact>]
    member this.``Retrieve all items for the authenticated user`` () =
        let result = actionController.Get () 
        let response = HttpTest.unpack<ActionableWebApi.ResponseToQuery> result
        Assert.True (response.Results.Length > 0)
        Assert.True true
        //((ActionableWebApi.ResponseToQuery)((System.Net.Http.ObjectContent<ActionableWebApi.ResponseToQuery>)result.Content).Value).Results[0].Fields

    [<Fact>]
    member this.``Create an item, retrieve it, update it, and delete it`` () =
        let title = "Hibidy jibity " + (System.Guid.NewGuid ()).ToString()
        let description = "have fun"
        let description' = "have the most fun"
        ignore <| actionController.Post {
            Id = ""
            Fields = [("actionable.title", title);("actionable.description",description)] |> Map.ofList
            Date = System.DateTimeOffset.Now.ToString ()
        }
        System.Threading.Thread.Sleep 20000
        let result = actionController.Get ()
        let response = HttpTest.unpack<ActionableWebApi.ResponseToQuery> result
        match response.Results |> List.tryFind (fun r -> r.Fields.["actionable.title"] = title)
            with
            | None -> failwith <| sprintf "item '%s' was not found" title
            | Some item -> 
                let ident = item.Id
                Assert.True (item.Fields.["actionable.description"] = description)
        
                ignore <| actionController.Post {
                    Id = item.Id.ToString ()
                    Fields = 
                        [("actionable.title", title);("actionable.description",description')] 
                        |> Map.ofList
                    Date = System.DateTimeOffset.Now.ToString ()
                } 

                System.Threading.Thread.Sleep 20000
                let result' = actionController.Get ()
                let response' = HttpTest.unpack<ActionableWebApi.ResponseToQuery> result'
                match response'.Results |> List.tryFind (fun r -> r.Fields.["actionable.title"] = title)
                    with 
                    | None -> failwith "Cannot find item"
                    | Some item' ->
                        Assert.Equal (ident, item'.Id)
                        Assert.Equal (description', item'.Fields.["actionable.description"])
        
        Assert.True true





