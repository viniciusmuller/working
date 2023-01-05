namespace Core

module Database =
    module Infrastructure =
        open DbUp
        open Microsoft.Data.SqlClient
        open System.Reflection

        let private buildEngine (connectionString: string) =
            DeployChanges
                .To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .WithTransaction()
                .Build()

        let migrate connectionString =
            let engine = buildEngine connectionString

            let result = engine.PerformUpgrade()

            if result.Successful then Ok() else Error result.Error

    module Provider =
        open FSharp.Data.Sql

        let [<Literal>] dbVendor = Common.DatabaseProviderTypes.POSTGRESQL

        [<Literal>]
        // TODO: use environment variables for database, username, host and password
        let connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres"

        type Schema = SqlDataProvider<dbVendor, connectionString>

        let createContext (conn: string) =
            Schema.GetDataContext(conn)

module Domain =
    open Database.Provider

    type Project = { ID: int; Name: string; Description: string }

module Repository =
    open Database.Provider
    open FSharp.Data.Sql
    open Domain

    module Project =
        type ProjectParams = {
            Name: string;
            Description: string 
        }

        let readAll (ctx: Schema.dataContext) = 
            query {
                for project in ctx.Public.Project do
                    select project
            }

        let read id (ctx: Schema.dataContext) = 
            query {
                for p in ctx.Public.Project do 
                where (p.Id = id)
                select (Some p)
                exactlyOneOrDefault
            }

        let create id (data: ProjectParams) (ctx: Schema.dataContext) =  task {
            let res = ctx.Public.Project.``Create(description, name)``(data.Description, data.Name)
            do! ctx.SubmitUpdatesAsync()
            return res
        }

        let update id (data: ProjectParams) (ctx: Schema.dataContext) = task {
            let entity = read id ctx
            match entity with
            | Some p -> 
                p.Name <- data.Name
                p.Description <- data.Description
                do! ctx.SubmitUpdatesAsync()
                return Ok(entity)
            | None -> return Error("could not update entity")
        }

        let delete id (ctx: Schema.dataContext) =
            let res = read id ctx
            match res with
            | Some res -> res.Delete()
            | None -> ()
            ctx.SubmitUpdatesAsync()

module Web =
    module Entry =
        open Giraffe
        open Saturn
        open Domain
        open Repository

        let router = 
          choose [
            route "/projects/{ID}"   >=> text "pong"
            route "/"       >=> htmlFile "/pages/index.html" ]

        let api = application {
            url "http://0.0.0.0:8080"
            use_router router
            memory_cache
            use_static "public"
            // use_json_serializer (ThothSerializer())
            use_gzip
        }

module Entry =
    open FsToolkit.ErrorHandling
    open Saturn

    [<EntryPoint>]
    let main _ =
        result {
            // do! Database.Infrastructure.migrate ""
            return Web.Entry.api
        }
        |> function
           | Ok api -> 
            run api
            0
           | Error e ->
            //    printfn "%A" e.Message
               1 
