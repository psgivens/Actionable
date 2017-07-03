module Actionable.Domain.ClientCommands

open System

type GetActionItem = { Id:Guid }

open Newtonsoft.Json

type ClientPayload = { command:string; data:obj }
// Utility method
let serializeClientCommand (cmd:System.Object) =    
    JsonConvert.SerializeObject { 
        ClientPayload.command = (cmd.GetType ()).Name
        data = cmd
        }

let deserializeClientCommand (serialized:string) =    
    let payload = JsonConvert.DeserializeObject<ClientPayload> serialized
    match payload.command with
    | "GetActionItem" -> 
        { payload with
            data=JsonConvert.DeserializeObject<GetActionItem> (payload.data.ToString ()) }
    | _ -> failwith <| sprintf "Unknown command: %s" payload.command
