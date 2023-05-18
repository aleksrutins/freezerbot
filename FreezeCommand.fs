module Freezer.Commands.Freeze
open Discord.WebSocket

let freeze (cmd: SocketSlashCommand) =
    cmd.RespondAsync "Freezing code"