module Database

open Models

open Npgsql.FSharp

let getUsers (connectionString: string) =
    connectionString
    |> Sql.connect
    |> Sql.query "SELECT * FROM api_user"
    |> Sql.execute (fun read ->
        {
            Id = read.int "id"
            Username = read.string "username"
            Email = read.string "email"
            Password = read.string "password"
            Bio = read.string "bio"
            Image = read.string "image"
        })
