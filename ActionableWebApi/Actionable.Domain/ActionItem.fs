module Actionable.Domain.ActionItemModule

type ActionItem = { UserId:string; Id: System.Guid; Fields: Map<string,string> }
    
type ActionItemCommand = 
    | Create of string * System.Guid * Map<string,string>
    | Update of Map<string,string>
    | Delete
    | Complete

type ActionItemEvent =
    | Created of ActionItem
    | Updated of Map<string,string>
    | Deleted
    | Completed

type ActionItemState = 
    | DoesNotExist
    | State of ActionItem
    
type InvalidCommand (state:ActionItemState, command:ActionItemCommand) =
    inherit System.Exception(sprintf "Invalid command.\n\tcommand: %A\n\tstate: %A" command state)
   
type InvalidEvent (state:ActionItemState, event: ActionItemEvent) =
    inherit System.Exception(sprintf "Invalid event.\n\event: %A\n\tstate: %A" event state)

let handle state command = 
    match state, command with
    | DoesNotExist, Create (userId, id, fields) -> 
        {ActionItem.UserId=userId; 
         Id=id; 
         Fields=fields|>Map.add "actionable.status" "0"}
        |> Created
    | State(item), Update (fields) -> 
        // TODO: Verify that key/value make sense

        // TODO: Get type information from the database for validation.

        Updated (fields)
    | State(item), Complete -> Completed
    | State(item), Delete -> Deleted
    | s, c -> raise <| InvalidCommand (s, c)

module Map =
    let merge map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map2 map1

let evolveState state event =
    match state, event with
    | DoesNotExist, Created (item) -> State (item)
    | State (item), Updated (fields) -> 
        State (
            {item with 
                Fields = item.Fields |> Map.merge fields})
    | State (item), Completed -> 
        State (
            {item with Fields = item.Fields |> Map.add "actionable.status" "1"})
    | State (item), Deleted -> DoesNotExist
    | s, e -> raise <| InvalidEvent (s,e)

let buildState =
    List.fold evolveState

