namespace ActionableWebApi

open System
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
    