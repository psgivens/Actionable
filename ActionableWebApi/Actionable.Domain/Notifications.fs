module Actionable.Domain.NotificationsModule

type Notification = { message:string; status:int }
type Notifications = { messages:Map<int,Notification> }

type NotificationsCommand = 
    | Create of string
    | Acknowledge of int

type NotificationsEvent =
    | Created of int * string
    | Acknowledged of int

type NotificationsState = 
    | DoesNotExist
    | State of Notifications
    
type InvalidCommand (state:NotificationsState, command:NotificationsCommand) =
    inherit System.Exception(sprintf "Invalid command.\n\tcommand: %A\n\tstate: %A" command state)
   
type InvalidEvent (state:NotificationsState, event:NotificationsEvent) =
    inherit System.Exception(sprintf "Invalid event.\n\event: %A\n\tstate: %A" event state)

let handle = function    
    | DoesNotExist, Create (message) -> Created (1,message) 
    | State (notifications), Create (message) -> 
        let key = notifications.messages |> Map.toSeq |> Seq.map fst |> Seq.max 
        Created (key+1, message)
    | State (notifications), Acknowledge (key) ->
        //let message = notifications.messages |> Map.find key 
        //notifications |> Map.add key message 
        Acknowledged key
    | s, c -> raise <| InvalidCommand (s, c)

module Map =
    let merge map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map2 map1

//let evolveState state event =
//    match state, event with
//    | DoesNotExist, Created (item) -> State (item)
//    | State (item), Updated (fields) -> 
//        let fields' = item.Fields |> Map.merge fields
//        let fields'' = fields |> Map.merge item.Fields
//        State (
//            {item with 
//                Fields = item.Fields |> Map.merge fields})
//    | State (item), Completed -> 
//        State (
//            {item with Fields = item.Fields |> Map.add "actionable.status" "1"})
//    | State (item), Deleted -> DoesNotExist
//    | s, e -> raise <| InvalidEvent (s,e)
//
//let buildState =
//    List.fold evolveState