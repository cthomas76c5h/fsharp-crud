module Handlers

open Giraffe
open Microsoft.AspNetCore.Http
open Models
open Storage
open Database

let private jsonResponse (statusCode: int) (obj: obj) =
    setStatusCode statusCode >=> json obj

let getAll : HttpHandler =
    fun next ctx ->
        task {
            let connectionString = ctx.Items.["ConnectionString"] :?> string
            return! jsonResponse 200 (getUsers connectionString) next ctx
        }

let getById (id: int) : HttpHandler =
    fun next ctx ->
        match getById id with
        | Some item -> jsonResponse 200 item next ctx
        | None -> jsonResponse 404 {| message = "Item not found" |} next ctx

let create : HttpHandler =
    fun next ctx ->
        task {
            let! item = ctx.BindJsonAsync<User>()
            let createdItem = create item
            return! jsonResponse 201 createdItem next ctx
        }

let update (id: int) : HttpHandler =
    fun next ctx ->
        task {
            let! item = ctx.BindJsonAsync<User>()
            match update id item with
            | Some updatedItem -> return! jsonResponse 200 updatedItem next ctx
            | None -> return! jsonResponse 404 {| message = "Item not found" |} next ctx
        }

let delete (id: int) : HttpHandler =
    fun next ctx ->
        match delete id with
        | Some deletedItem -> jsonResponse 200 deletedItem next ctx
        | None -> jsonResponse 404 {| message = "Item not found" |} next ctx
