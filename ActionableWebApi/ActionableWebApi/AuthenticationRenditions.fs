namespace ActionableWebApi

open System
open System.ComponentModel.DataAnnotations 

type RegisterBindingRendition () = 
    [<Required>]
    [<Display(Name = "Email")>]
    member val Email = String.Empty with get, set 

    [<Required>]
    [<StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)>]
    [<DataType(DataType.Password)>]
    [<Display(Name = "Password")>]
    member val Password = String.Empty with get, set

    [<DataType(DataType.Password)>]
    [<Display(Name = "Confirm password")>]
    [<Compare("Password", ErrorMessage = "The password and confirmation password do not match.")>]
    member val ConfirmPassword = String.Empty with get, set

open System.Web.Http
open System.Net.Http
open System.Net

open System.Threading
open System.Threading.Tasks


type ChallengeResult (loginProvider:String, controller:ApiController) = 
    member val LoginProvider = loginProvider with get, set
    member val Request = controller.Request with get, set
    interface IHttpActionResult with
        member this.ExecuteAsync (cancelationToken:CancellationToken) :Task<HttpResponseMessage> = 
            this.Request.GetOwinContext().Authentication.Challenge(this.LoginProvider)
            let response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            response.RequestMessage <- this.Request
            Task.FromResult response