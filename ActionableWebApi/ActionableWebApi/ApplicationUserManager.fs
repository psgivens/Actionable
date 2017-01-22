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
type ApplicationUserManager (store:IUserStore<ApplicationUser>) =
    inherit UserManager<ApplicationUser> (store)

    static member Create (options:IdentityFactoryOptions<ApplicationUserManager>, context:IOwinContext) =
        let manager = 
            new ApplicationUserManager(
                new UserStore<ApplicationUser>(context.Get<ApplicationDbContext> ()),
                    PasswordValidator = PasswordValidator(
                        RequiredLength = 6,
                        RequireNonLetterOrDigit = true,
                        RequireDigit = true,
                        RequireLowercase = true,
                        RequireUppercase = true
                        ))
        manager.UserValidator <- UserValidator<ApplicationUser> 
            (manager, 
            AllowOnlyAlphanumericUserNames = false,
            RequireUniqueEmail = true)
        let dataProtectionProvider = options.DataProtectionProvider in
        if (dataProtectionProvider <> null) then
            manager.UserTokenProvider <- DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
        manager