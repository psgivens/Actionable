namespace ActionableWebApi

open System
open System.Collections.Generic
open System.Security.Claims
open Microsoft.AspNet.Identity

type UserInfoViewModel (email:String, hasRegistered:bool, loginProvider:string) =
    member val Email = email with get, set
    member val HasRegistered = hasRegistered with get, set
    member val LoginProvider = loginProvider with get, set
    new () = UserInfoViewModel (null, false, null)

[<AllowNullLiteral>] 
type ExternalLoginData (loginProvider:string, providerKey:string, userName:string) =
    member val LoginProvider = loginProvider with get, set
    member val ProviderKey = providerKey with get, set
    member val UserName = userName with get, set

    member this.GetClaims () : IList<Claim> =
        let claims = List<Claim> ();
        claims.Add (Claim(ClaimTypes.NameIdentifier, this.ProviderKey, null, this.LoginProvider))
        if (not <| String.IsNullOrEmpty(this.UserName)) 
        then claims.Add (Claim(ClaimTypes.Name, this.UserName, null, this.LoginProvider))
        claims :> IList<Claim>
    
    static member FromIdentity (identity:ClaimsIdentity) :ExternalLoginData = 
        if (identity = null) then null
        else
            let providerKeyClaim = identity.FindFirst (ClaimTypes.NameIdentifier)
            if (providerKeyClaim = null || String.IsNullOrEmpty(providerKeyClaim.Issuer) || String.IsNullOrEmpty (providerKeyClaim.Value)) 
            then null
            elif (providerKeyClaim.Issuer = ClaimsIdentity.DefaultIssuer) 
            then null
            else ExternalLoginData 
                    (providerKeyClaim.Issuer, 
                    providerKeyClaim.Value, 
                    identity.FindFirstValue(ClaimTypes.Name))

    