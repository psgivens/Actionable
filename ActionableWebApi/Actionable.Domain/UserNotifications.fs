module Actionable.Domain.UserNotificationsModule

type UserNotification = { message:string; status:int }
type UserNotifications = { messages:Map<int,UserNotification > }

type UserNotificationsCommand = 
    | AppendMessage of string
    | AcknowledgeMessage of int

type UserNotificationsEvent =
    | MessageAppended of int * UserNotification
    | MessageAcknowledged of int

type UserNotificationsState = 
    | DoesNotExist
    | State of UserNotifications
    
type InvalidCommand (state:UserNotificationsState, command:UserNotificationsCommand) =
    inherit System.Exception(sprintf "Invalid command.\n\tcommand: %A\n\tstate: %A" command state)
   
type InvalidEvent (state:UserNotificationsState, event:UserNotificationsEvent) =
    inherit System.Exception(sprintf "Invalid event.\n\event: %A\n\tstate: %A" event state)

let handle state command =
    match state, command with
    | DoesNotExist, AppendMessage (message) -> MessageAppended (1, {message=message; status=0}) 
    | State (notifications), AppendMessage (message) -> 
        let key = notifications.messages |> Map.toSeq |> Seq.map fst |> Seq.max 
        MessageAppended (key+1, {message=message; status=0})
    | State (notifications), AcknowledgeMessage (key) ->
        //let message = notifications.messages |> Map.find key 
        //notifications |> Map.add key message 
        MessageAcknowledged key
    | s, c -> raise <| InvalidCommand (s, c)

module Map =
    let merge map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map2 map1

let evolveState state event =
    match state, event with
    | State (notifications), MessageAppended (idx, notification) -> 
        State (
            { notifications with 
                messages = notifications.messages |> Map.add idx notification 
            }
        )
    | State (notifications), MessageAcknowledged (idx) -> 
        let notification = notifications.messages |> Map.find idx
        State (
            { notifications with
                messages = 
                    notifications.messages 
                    |> Map.add idx {message=notification.message; status=1}
            }
        )
    | s, e -> raise <| InvalidEvent (s,e)

let buildState =
    List.fold evolveState

