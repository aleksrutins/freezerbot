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

let mutable client: DiscordSocketClient = null
let guildId = ""

let slashOpt name t desc required = (new SlashCommandOptionBuilder()).WithName(name).WithType(t).WithDescription(desc).WithRequired(required)

let registerCommands () =
    Async.Parallel ([|
        (new SlashCommandBuilder())
            .WithName("freeze")
            .WithDescription("Freeze the codebase.")
            .Build()

        
        (new SlashCommandBuilder())
            .WithName("thaw")
            .WithDescription("Unfreeze the codebase.")
            .Build()

        (new SlashCommandBuilder())
            .WithName("request-unfreeze")
            .WithDescription("Request a freeze exception for a branch.")
            .AddOption(slashOpt "branch" ApplicationCommandOptionType.String "The branch to create a request for" true)
            .Build()
        
        (new SlashCommandBuilder())
            .WithName("status")
            .WithDescription("View the current status of the codebase.")
            .Build()
        
        (new SlashCommandBuilder())
            .WithName("review")
            .WithDescription("Review an unfreeze request.")
            .AddOption(slashOpt "branch" ApplicationCommandOptionType.String "The branch to review for" true)
            .AddOption(
                (new SlashCommandOptionBuilder())
                    .WithName("review")
                    .WithDescription("Your review")
                    .WithRequired(true)
                    .AddChoice("reject", 0)
                    .AddChoice("accept", 0)
                    .WithType(ApplicationCommandOptionType.Integer)
            )
            .Build()
    |] |> Array.map client.CreateGlobalApplicationCommandAsync |> Array.map Async.AwaitTask)

let clientReady () =
    registerCommands () |> Async.Ignore |> Async.StartAsTask

let mainAsync () =
    client <- new DiscordSocketClient()
    client.add_Log log
    client.add_Ready (fun () -> clientReady())
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