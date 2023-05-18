module RepositoryModel

open System
open System.ComponentModel.DataAnnotations
open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions


[<CLIMutable>]
type Repository = {
    [<Key>] Name: string
    mutable GuildID: uint64
    mutable Frozen: bool
}

[<CLIMutable>]
type UnfreezeRequest = {
    [<Key>] Id: int
    mutable Repository: Repository
    mutable Branch: string
}

[<CLIMutable>]
type Approval = {
    [<Key>] Id: int
    mutable User: string
    mutable Request: UnfreezeRequest
}

type RepositoryContext() =
    inherit DbContext()

    [<DefaultValue>] val mutable repositories: Repository DbSet
    member this.Repositories with get() = this.repositories and set v = this.repositories <- v

    [<DefaultValue>] val mutable unfreezeRequests: UnfreezeRequest DbSet
    member this.UnfreezeRequests with get() = this.unfreezeRequests and set v = this.unfreezeRequests <- v

    [<DefaultValue>] val mutable approvals: Approval DbSet
    member this.Approvals with get() = this.approvals and set v = this.approvals <- v

    override _.OnModelCreating builder =
        builder.RegisterOptionTypes()
    
    override _.OnConfiguring options =
        let host = Environment.GetEnvironmentVariable("PGHOST")
        let port = Environment.GetEnvironmentVariable("PGPORT")
        let user = Environment.GetEnvironmentVariable("PHUSER")
        let password = Environment.GetEnvironmentVariable("PGPASSWORD")
        let database = Environment.GetEnvironmentVariable("PGDATABASE")
        let connStr = $"Host={host};Port={port};Username={user};Password={password};Database={database}"
        options.UseNpgsql(connStr) |> ignore