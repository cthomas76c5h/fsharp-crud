module Storage

open Models

let mutable private items = Map.empty<int, User>
let mutable private nextId = 1

let getAll () = items |> Map.toList |> List.map snd

let getById id = items |> Map.tryFind id

let create item =
    let newItem = { item with Id = nextId }
    items <- items |> Map.add nextId newItem
    nextId <- nextId + 1
    newItem

let update id item =
    match getById id with
    | Some _ ->
        let updatedItem = { item with Id = id }
        items <- items |> Map.add id updatedItem
        Some updatedItem
    | None -> None

let delete id =
    match getById id with
    | Some item ->
        items <- items |> Map.remove id
        Some item
    | None -> None
