open System
open System.IO
open System.Threading.Tasks
open Discord
open Discord.WebSocket
open dotenv.net
open Freezer.Commands.Registrar
open Freezer.Commands.Freeze
// For more information see https://aka.ms/fsharp-console-apps

let log msg =
    printfn "%s" (msg.ToString())
    Task.CompletedTask

let mutable client: DiscordSocketClient = null
let slashCommandHandler (cmd: SocketSlashCommand) =
    cmd |> 
        match cmd.Data.Name with
            | "freeze" -> freeze
            | a -> (fun _ -> cmd.RespondAsync($"Unknown command: {a}"))

let clientReady () =
    //registerCommands client
    Task.CompletedTask

let mainAsync () =
    client <- new DiscordSocketClient()
    client.add_Log log
    client.add_Ready (fun () -> clientReady())
    client.add_SlashCommandExecuted slashCommandHandler
    let token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")
    Async.Sequential ([|
        client.LoginAsync(TokenType.Bot, token)
        client.StartAsync()
        Task.Delay -1
    |] |> Array.map(Async.AwaitTask))

[<EntryPoint>]
let main argv =
    DotEnv.Load()
    mainAsync () |> Async.RunSynchronously |> ignore
    0