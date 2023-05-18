module Freezer.Util

open Discord

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