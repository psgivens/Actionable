namespace ActionableWebApi

open System
open System.Net
open System.Net.Http
open System.Web.Http

open System.Security.Claims

open Microsoft.Owin.Security
open Microsoft.AspNet.Identity.Owin
open Microsoft.AspNet.Identity
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.OAuth
open System.Threading.Tasks

[<Authorize>]
[<RoutePrefix("api/v1/Account")>]
type AccountController (userManager:ApplicationUserManager, accessTokenFormat:ISecureDataFormat<AuthenticationTicket>) = 
    inherit ApiController ()

    [<Literal>]
    let LocalLoginProvider = "Local"
    
    
    let mutable _userManager = userManager 
    member this.UserManager
        with get () :ApplicationUserManager = 
            if _userManager = null
            then _userManager <- this.Request.GetOwinContext().GetUserManager<ApplicationUserManager>()
                 _userManager
            else _userManager
        and set (value) = _userManager <- value

    member val AccessTokenFormat:ISecureDataFormat<AuthenticationTicket> = accessTokenFormat with get, set
    
    member this.Authentication 
        with get () =
            (this.Request.GetOwinContext()).Authentication

    new () = new AccountController(null, null)

    [<HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)>]
    [<Route("UserInfo")>]
    member this.GetUserInfo () : UserInfoViewModel =
        let externalLogin = ExternalLoginData.FromIdentity(this.User.Identity :?> ClaimsIdentity)
        let hasExernalLogin = externalLogin <> null
        let loginProvider = if (externalLogin = null) then null else externalLogin.LoginProvider
        UserInfoViewModel (this.User.Identity.GetUserName(), hasExernalLogin, loginProvider)

    // POST api/Account/Logout
    [<Route("Logout")>]
    member this.Logout () = 
        this.Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        this.Ok() :> IHttpActionResult


    member private this.internalServerError () = this.InternalServerError () :> IHttpActionResult
    member private this.badRequestModel () = this.BadRequest this.ModelState
    member private this.ok () = this.Ok () :> IHttpActionResult
            
    member private this.GetErrorResult (result:IdentityResult) :IHttpActionResult =
        if result = null then this.InternalServerError () :> IHttpActionResult
        else if (not result.Succeeded) then
            if result.Errors <> null then 
                result.Errors |> Seq.iter (fun error -> this.ModelState.AddModelError("", error))

            if this.ModelState.IsValid then this.BadRequest () :> IHttpActionResult
            else this.badRequestModel () :> IHttpActionResult
        else null :> IHttpActionResult

    // POST api/Account/Register
    [<AllowAnonymous>]
    [<Route("Register")>]
    member this.Register (model:RegisterBindingRendition) :Task<IHttpActionResult> =         
        
        async {
            if (not this.ModelState.IsValid) 
            then return this.badRequestModel () :> IHttpActionResult
            else 
                let user = new ApplicationUser( UserName = model.Email, Email = model.Email)
                let! result = this.UserManager.CreateAsync(user, model.Password) |> Async.AwaitTask 
                if (not result.Succeeded) 
                then return this.GetErrorResult (result) 
                else return this.ok() 
                    
            } |> Async.StartAsTask
        
    member private this.redirectError error =
        this.Redirect(this.Url.Content("~/") + "#error=" + Uri.EscapeDataString(error))

    // GET api/Account/ExternalLogin
    [<OverrideAuthentication>]
    [<HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)>]
    [<AllowAnonymous>]
    [<Route("ExternalLogin", Name = "ExternalLogin")>]
    member this.GetExternalLogin (provider:string, error:string) :Task<IHttpActionResult> = 
        async {        
            if error = null 
            then return this.redirectError error :> IHttpActionResult
            elif (not this.User.Identity.IsAuthenticated) 
            then return ChallengeResult (provider, this) :> IHttpActionResult
            else
                let externalLogin = ExternalLoginData.FromIdentity (this.User.Identity :?> System.Security.Claims.ClaimsIdentity) 
                if externalLogin = null 
                then return this.internalServerError () 
                elif (externalLogin.LoginProvider <> provider) 
                then
                    this.Authentication.SignOut DefaultAuthenticationTypes.ExternalCookie
                    return ChallengeResult (provider, this) :> IHttpActionResult
                else
                    let! user = 
                        (externalLogin.LoginProvider, externalLogin.ProviderKey)
                        |> UserLoginInfo
                        |> this.UserManager.FindAsync 
                        |> Async.AwaitTask
                    let hasRegistered = user <> null
                    if hasRegistered 
                    then 
                        this.Authentication.SignOut DefaultAuthenticationTypes.ExternalCookie
                        let! oAuthIdentity = 
                            (this.UserManager, OAuthDefaults.AuthenticationType)
                            |> user.GenerateUserIdentityAsync  
                            |> Async.AwaitTask
                        let! cookieIdentity = 
                            (this.UserManager, CookieAuthenticationDefaults.AuthenticationType) 
                            |> user.GenerateUserIdentityAsync 
                            |> Async.AwaitTask
                        let properties = ApplicationOAuthProvider.CreateProperties user.UserName
                        this.Authentication.SignIn (properties, oAuthIdentity, cookieIdentity)
                    else 
                        let claims = externalLogin.GetClaims ()
                        let identity = ClaimsIdentity (claims, OAuthDefaults.AuthenticationType)
                        this.Authentication.SignIn identity
                    return this.ok ()
                
        } |> Async.StartAsTask
