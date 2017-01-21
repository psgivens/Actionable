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

type RegisterBindingModel () = 
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
