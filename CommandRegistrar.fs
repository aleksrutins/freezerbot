module Freezer.Commands.Registrar

open Discord
open Discord.WebSocket
open Freezer.Util

let registerCommands (client: DiscordSocketClient) =
    Async.Sequential([|
        async {
            let! cmds = client.GetGlobalApplicationCommandsAsync() |> Async.AwaitTask
            Async.Parallel [| 
                for cmd in cmds do
                    cmd.DeleteAsync() |> Async.AwaitTask
            |] |> ignore
        }
        Async.Parallel ([|
            (slashCommand "link-repo" "Link a repository to this server" [|
                slashOpt "repo" ApplicationCommandOptionType.String "The name of the repository to link" true
            |]).Build()

            (slashCommand "freeze" "Freeze the codebase" [||]).Build()

            
            (slashCommand "thaw" "Unfreeze the codebase" [||]).Build()

            (slashCommand "request-unfreeze" "Request a code freeze exception" [| 
                slashOpt "branch" ApplicationCommandOptionType.String "The branch to create a request for" true 
            |]).Build()
            
            (slashCommand "status" "View the current status of the codebase." [||]).Build()

            (slashCommand "review" "Review an unfreeze request" [|
                slashOpt "branch" ApplicationCommandOptionType.String "The branch to review for" true
                slashOptSelect "review" "Your review" true [| ("reject", 0); ("accept", 1) |]
            |]).Build()
        |] |> Array.map client.CreateGlobalApplicationCommandAsync |> Array.map Async.AwaitTask) |> Async.Ignore
    |])