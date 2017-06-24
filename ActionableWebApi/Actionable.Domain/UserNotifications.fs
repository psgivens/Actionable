module Actionable.Domain.UserNotificationsModule

type UserNotification = { code:int; message:string; status:int }
type UserNotifications = { userId:string; items:Map<int, UserNotification>}

type UserNotificationsCommand = 
    | AppendMessage of string * int * string
    | AcknowledgeMessage of int

type UserNotificationsEvent =
    | MessageCreated of string * int * int * string
    | MessageAcknowledged of int
    | MessageRemoved of int

type UserNotificationsState = 
    | DoesNotExist
    | State of UserNotifications
    
type InvalidCommand (state:UserNotificationsState, command:UserNotificationsCommand) =
    inherit System.Exception(sprintf "Invalid command.\n\tcommand: %A\n\tstate: %A" command state)
   
type InvalidEvent (state:UserNotificationsState, event:UserNotificationsEvent) =
    inherit System.Exception(sprintf "Invalid event.\n\event: %A\n\tstate: %A" event state)

let handle state command =
    match state, command with
    | DoesNotExist, AppendMessage (userId, code, message) -> MessageCreated (userId, 1, code, message) 
    | State (notifications), AppendMessage (userId, code, message) -> 
        let key = notifications.items |> Map.toSeq |> Seq.map fst |> Seq.max 
        MessageCreated (userId, key+1, code, message)
    | State (notifications), AcknowledgeMessage (key) ->
        //let message = notifications.messages |> Map.find key 
        //notifications |> Map.add key message 
        MessageAcknowledged key
    | s, c -> raise <| InvalidCommand (s, c)

module Map =
    let merge map1 map2 = Map.fold (fun acc key value -> Map.add key value acc) map2 map1

let evolveState state event =
    match state, event with
    | UserNotificationsState.DoesNotExist, MessageCreated (userId, id, code, message) ->
        State (
            {   UserNotifications.userId = userId;
                items=[(id,{UserNotification.code=code; message=message; status=0})] |> Map.ofList }
        )
    | State (notifications), MessageCreated (userId, id, code, message) -> 
        State (
            { notifications with 
                items = notifications.items |> Map.add id {UserNotification.code=code; message=message; status=0}
            }
        )
    | State (notifications), MessageAcknowledged (idx) -> 
        let notification = notifications.items |> Map.find idx
        State (
            { notifications with
                items = 
                    notifications.items 
                    |> Map.add idx {notification with status=1}
            }
        )
    | State (notifications), MessageRemoved (idx) -> 
        let notification = notifications.items |> Map.find idx
        State (
            { notifications with
                items = 
                    notifications.items 
                    |> Map.remove idx 
            }
        )

    | s, e -> raise <| InvalidEvent (s,e)

let buildState =
    List.fold evolveState

