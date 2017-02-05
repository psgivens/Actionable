module NewtonsoftHack

open System
let resolveAnyVersion assemblyName = 
    let resolveA (o:Object) (a:ResolveEventArgs) :Reflection.Assembly =
        let name = a.Name.Split(',').[0]
        match name with
        | x when x = assemblyName ->
            match AppDomain.CurrentDomain.GetAssemblies()
                |> Seq.where (fun assembly ->
                    assembly.GetName().Name = name)
                |> Seq.tryHead
                with
                | None -> failwith <| sprintf "Cannot find assembly %s" name
                | Some assembly -> assembly
        | _ -> null

    AppDomain.CurrentDomain.add_AssemblyResolve 
        (ResolveEventHandler resolveA)

let resolveNewtonsoft () = 
    // This seems a Hack. I spent over 8 hours permutating through solutions
    // including bindingRedirect before settling on this. I do not know why
    // I could not do this in the App.Config file of the executing project. I 
    // suspect that there are projects using '.net core' and that '.net core' 
    // does not support this. 

    // Specifics
    // Expected assembly: 6.0.0.0 - Used by some of the asp.net assemblies 
    // Actual assembly: 9.0.0.0 - Used by Akka.net (meta 9.0.1.19813)
    resolveAnyVersion "Newtonsoft.Json"