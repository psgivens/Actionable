namespace ActionableWebApi

open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin
open Microsoft.FSharp.Control
open System.Threading.Tasks
open System
open System.ComponentModel.DataAnnotations 

open System.Security.Claims

[<AllowNullLiteral>]
type ApplicationUser () =
    inherit IdentityUser ()
    member this.GenerateUserIdentityAsync(manager:UserManager<ApplicationUser>, authenticationType:string): Task<ClaimsIdentity> =        
        manager.CreateIdentityAsync(this, authenticationType)

type ApplicationDbContext () = 
    inherit IdentityDbContext<ApplicationUser> ("DefaultConnection")
    static member Create () =
        new ApplicationDbContext ()

