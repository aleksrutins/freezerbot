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

let slashCommand name desc options =
    let cmd =
        (new SlashCommandBuilder())
            .WithName(name)
            .WithDescription(desc)
    
    options |> (Array.fold (fun cmd opt -> (cmd :> SlashCommandBuilder).AddOption(opt)) cmd)

let slashOpt name t desc required = (new SlashCommandOptionBuilder()).WithName(name).WithType(t).WithDescription(desc).WithRequired(required)

let slashOptSelect<'t> name desc required (choices: (string * int) array) = 
    let opt = (slashOpt name ApplicationCommandOptionType.Integer desc required)
    choices |> (Array.fold 
        (fun opt choice -> 
            let (name, value) = choice
            (opt :> SlashCommandOptionBuilder).AddChoice(name, value)) opt)

let registerCommands () =
    Async.Parallel ([|
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