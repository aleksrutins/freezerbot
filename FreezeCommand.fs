module Freezer.Commands.Freeze
open Discord.WebSocket
open RepositoryModel

let private db = new RepositoryContext()

let freeze (cmd: SocketSlashCommand) =
    match query {
        for repo in db.Repositories do
            where (repo.GuildID = cmd.GuildId.GetValueOrDefault())
            select repo
    } with
    | repos when Seq.length repos > 0 ->
        let repo = Seq.head repos
        repo.Frozen <- true
        db.SaveChanges() |> ignore
        cmd.RespondAsync(":snowflake: Frozen.")
    | _ -> cmd.RespondAsync(":bangbang: No repository found for this server.")