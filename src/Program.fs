module GiraffeCrudApp.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Handlers
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open Npgsql.FSharp

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:5000")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> text "Hello World from Giraffe"
                route "/api/items" >=> getAll
                routef "/api/items/%i" getById
            ]
        POST >=> route "/api/items" >=> create
        PUT >=> routef "/api/items/%i" update
        DELETE >=> routef "/api/items/%i" delete
    ]

let configureApp (app : IApplicationBuilder) =
    app.Use(fun (context : HttpContext) (next : RequestDelegate) ->
        task {
            let connectionString : string =
                Sql.host "localhost"
                |> Sql.database "example"
                |> Sql.username "postgres"
                |> Sql.password "secret1234"
                |> Sql.port 5433
                |> Sql.formatConnectionString
            context.Items.["ConnectionString"] <- connectionString
            return! next.Invoke(context)
        } :> Task)
    |> ignore
    
    app.UseCors(configureCors)
       .UseGiraffe webApp

[<EntryPoint>]
let main _ =
    let port = 
        Environment.GetEnvironmentVariable("PORT")
        |> Option.ofObj
        |> Option.map int
        |> Option.defaultValue 5000

    let host = 
        Environment.GetEnvironmentVariable("HOST")
        |> Option.ofObj
        |> Option.defaultValue "127.0.0.1"

    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseKestrel(fun options ->
                        options.Listen(System.Net.IPAddress.Parse(host), port)
                    )
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0
