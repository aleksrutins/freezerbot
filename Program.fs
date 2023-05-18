open System
open System.IO
open System.Threading.Tasks
open Discord
open Discord.WebSocket
open dotenv.net
// For more information see https://aka.ms/fsharp-console-apps

let log msg =
    printfn "%s" (msg.ToString())
    Task.CompletedTask

let mutable client = null

let mainAsync () =
    async {
        client <- new DiscordSocketClient()
        client.add_Log log
        let token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")
        client.LoginAsync(TokenType.Bot, token) |> Async.AwaitTask |> ignore
        client.StartAsync() |> Async.AwaitTask |> ignore
        Task.Delay -1 |> Async.AwaitTask |> ignore
    }

[<EntryPoint>]
let main argv =
    DotEnv.Load()
    mainAsync () |> Async.RunSynchronously
    0