﻿module FunDomain.CommandHandler

let create 
        (play:
            (('state option->'event->'state option)->'state option->'event seq->'state option) // Any XX.fold function
            ->'state option->'event seq->'state option)
        (handle:'state option->'command->('event list))
        (read:'streamId->int->int->Async<EncodedEvent seq*'token*int option>)
        (append:'streamId->'token->EncodedEvent list->Async<'r>) =
    let load stream =
        let rec fold state version = async {
            let sliceSize = 500
            let! events, sliceLastToken, nextMinEventNumber = read stream version sliceSize
            let newState = 
                events 
                |> Seq.choose EncodedEvent.deserializeUnionByCaseItemTypeName<'event>
                |> play Seq.fold state
            match nextMinEventNumber with
            | None -> return sliceLastToken, newState
            | Some minStreamIndex -> return! fold newState minStreamIndex }
        fold None 0

    fun topicId command -> async {
        let! initialVersion, state = load topicId 
        do! handle state command
            |> List.map EncodedEvent.serializeUnionByCaseItemTypeName
            |> append topicId initialVersion
            |> Async.Ignore }