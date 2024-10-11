module Program

open Token
open Parser
open Evaluator
open System.IO

let execute filename =
    let lines = File.ReadAllLines(filename)
    let source = String.concat "" lines
    try
        let tokens = Token.tokenize (List.ofSeq source)
        let expr = Parser.parse tokens
        let evaluated, _ = Evaluator.evaluate Map.empty expr
        printfn "Processed: %A\n" evaluated
    with ex ->
        printfn "Exception: %s" ex.Message

[<EntryPoint>]
let main argv =
    let filename = argv.[0]
    execute filename
    0