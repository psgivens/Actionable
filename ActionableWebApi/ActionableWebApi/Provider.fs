namespace ActionableWebApi
 
open Owin

open System
open System.Web.Http
open System.Web.Routing

open Microsoft.Owin
open Microsoft.Owin.Builder
open Microsoft.Owin.FileSystems
open Microsoft.Owin.StaticFiles

open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.Security.OAuth
open Microsoft.Owin.Security.Cookies

open System.Threading.Tasks
[<AutoOpen>]
module Async =
    let inline awaitPlainTask (task: Task) = 
        // rethrow exception from preceding task if it fauled
        let continuation (t : Task) : unit =
            match t.IsFaulted with
            | true -> raise t.Exception
            | arg -> ()
        task.ContinueWith continuation |> Async.AwaitTask

    let inline startAsPlainTask (work : Async<unit>) = 
        work |> Async.StartAsTask :> Task
        //Task.Factory.StartNew (
        //    fun () -> work |> Async.StartAsTask :> Task)

type ApplicationOAuthProvider (publicClientId:string) =
    inherit OAuthAuthorizationServerProvider ()

    do if publicClientId = null then raise <| new System.ArgumentNullException ("publicClientId")
    let _publicClientId = publicClientId

    static member CreateProperties (userName:string) =
        dict["userName", userName] 
        |> System.Collections.Generic.Dictionary<String,String>
        |> AuthenticationProperties
        
    override this.GrantResourceOwnerCredentials (context:OAuthGrantResourceOwnerCredentialsContext) : System.Threading.Tasks.Task =
        async {            
            let userManager = context.OwinContext.GetUserManager<ApplicationUserManager>()
            let! user = 
                userManager.FindAsync(context.UserName, context.Password) 
                |> Async.AwaitTask

            if (user = Unchecked.defaultof<ApplicationUser>) then
                context.SetError("invalid_grant", "The user name or password is incorrect.");
            
            // Start both identity tasks
            let oAuthIdentityTask = user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType)
            let cookiesIdentityTask = user.GenerateUserIdentityAsync(userManager, CookieAuthenticationDefaults.AuthenticationType)

            // await both identity tasks
            let! oAuthIdentity = oAuthIdentityTask |> Async.AwaitTask
            let! cookiesIdentity = cookiesIdentityTask |> Async.AwaitTask

            let properties = ApplicationOAuthProvider.CreateProperties (user.UserName)
            let ticket = AuthenticationTicket (oAuthIdentity, properties)
            context.Validated ticket |> ignore
            context.Request.Context.Authentication.SignIn cookiesIdentity |> ignore
                            
        } |> Async.startAsPlainTask

    override this.TokenEndpoint (context:OAuthTokenEndpointContext) =
        context.Properties.Dictionary
        |> Seq.iter (fun pair ->
            context.AdditionalResponseParameters.Add (pair.Key, pair.Value))
        Task.FromResult<Object>(null) :> Task

    override this.ValidateClientAuthentication (context:OAuthValidateClientAuthenticationContext) =
        if (context.ClientId = Unchecked.defaultof<string>) then 
            context.Validated () |> ignore
        Task.FromResult<Object> null :> Task

    override this.ValidateClientRedirectUri (context:OAuthValidateClientRedirectUriContext) =
        if (context.ClientId = _publicClientId) then
            let expectedRootUri = Uri (context.Request.Uri, "/") 
            if (expectedRootUri.AbsoluteUri = context.RedirectUri) then 
                context.Validated () |> ignore
        Task.FromResult<Object> null :> Task
