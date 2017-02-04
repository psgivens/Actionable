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
open Microsoft.Owin.Security.OAuth

open System.Web.Http.Cors
open Microsoft.Owin.Cors
open System.Web.Cors

open System.Threading
open System.Threading.Tasks

type HttpRouteDefaults = { Controller : string; Id : obj }
type TemplateRouteDefaults = { Id: obj }

type Startup () =
    let configureRoutes (config:HttpConfiguration) = 
         
        config.MapHttpAttributeRoutes();

        let routes = config.Routes        

        routes.MapHttpRoute(
            "DefaultRoute",
            "") |> ignore

        routes.MapHttpRoute(
            "DefaultAPI",
            "api/v1/{controller}/{id}",
             { Controller = "Home"; Id = RouteParameter.Optional }            
            ) |> ignore

    let  configureAuth (config:HttpConfiguration, app:IAppBuilder) =
        app.CreatePerOwinContext (ApplicationDbContext.Create) |> ignore
        app.CreatePerOwinContext<ApplicationUserManager> (fun options context -> ApplicationUserManager.Create(options,context)) |> ignore

        // By default, request authorization
        config.Filters.Add (AuthorizeAttribute())

        let publicClientId = "self";
        let OAuthOptions = 
            new OAuthAuthorizationServerOptions (
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(publicClientId),
                AuthorizeEndpointPath = new PathString("/api/v1/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14.0),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            )
        app.UseOAuthBearerTokens OAuthOptions
        ()

    let configureServices (config:HttpConfiguration) =
        config.Services.Replace(
            typeof<System.Web.Http.Dispatcher.IHttpControllerActivator>,
            CompositRoot())

    member this.Configuration (app:IAppBuilder) =
        //System.Diagnostics.Debugger.Break ()   
//        failwith "Testing my errors"

        let config = GlobalConfiguration.Configuration

        let cors = new EnableCorsAttribute(origins= "http://localhost:3001", headers= "*", methods= "*")
        app.UseCors (
            CorsOptions(
                PolicyProvider = CorsPolicyProvider(
                    PolicyResolver = (
                        fun request -> 
                            let x = request.Path.Value
                            if request.Path.Value = "/Token" then cors.GetCorsPolicyAsync (null, CancellationToken.None)
                            else Task.FromResult<CorsPolicy> null)))) |> ignore

        config.EnableCors(cors)

        configureRoutes config
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
            Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

        // ** May be necessary when implementing security check on file routes. ** 
        // ** Do not know yet. If not, uninstall StaticFiles and FileSystems packages. **
        // ** If so, move static files to a 'static' directory. ** 
        let fileSystem = PhysicalFileSystem (".")
        app.UseFileServer(
            FileServerOptions(
                RequestPath = PathString(""),
                FileSystem = fileSystem,
                EnableDirectoryBrowsing = false)) |> ignore
        GlobalConfiguration.Configuration.EnsureInitialized();
        configureAuth (config, app)

        configureServices (config)
