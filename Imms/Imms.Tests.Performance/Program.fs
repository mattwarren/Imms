// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module Imms.Tests.Performance.Main
open Imms.Tests.Performance
open Imms
open System
open ExtraFunctional
open System.Diagnostics
open LINQtoCSV
open System.Collections.Immutable
open System.IO
open System.Linq
open System.Windows.Forms
open System.Drawing
open System.Drawing.Imaging
open System.CodeDom.Compiler
open Imms.Tests
open Imms.FSharp.Implementation
///Power over integers.
let inline (^*) (a : int) (b : int) = pown a b
let inline (++) (a : _ list) (b : _ list) = b |> List.append a
let rec containsAny (lst) (trgt : string) =
    match lst with
    | [] -> false
    | h::t -> trgt.Contains(h) || trgt |> containsAny t
    
let getFreeDir path dir = 
    let rec tryNum n = 
        let tryName = sprintf "%s\%s%03d" path dir n
        if Directory.Exists tryName then
            tryNum (n+1)
        else
            Directory.CreateDirectory tryName |> ignore
            tryName, n
    tryNum 0

let runTest (writer : IndentedTextWriter) (tag : ErasedTest) = 
    fprintfn writer "Beginning Test '%s' with metadata:" tag.Test.Name
    writer.Push()
    fprintfn writer "%O" tag.Test
    fprintfn writer "Target: '%s' with metadata:" tag.Target.Name
    fprintfn writer "%O" tag.Target
    let time = Bench.invoke (tag.RunTest)
    fprintfn writer "Ending Test: '%s' for '%s' with time, '%A'\n" tag.Test.Name tag.Target.Name time
    tag.Time <- time
    writer.Pop()
    tag :> TestInstanceMeta

let runTests (tests : ErasedTest list) = 
    use writer = new IndentedTextWriter(Console.Out)
    let mutable results = []

    let results = tests |> List.map (runTest writer)
    let mutable i = 0;
    let basePath = "..\..\Benchmarks\Results"
    let resultFolder, n = getFreeDir basePath "benchmark"
    let logsFolder = resultFolder
    
    
    let table = results |> Report.toTable
    File.WriteAllText(sprintf "%s\\%03d.table.csv" logsFolder n, table)
    let log = results |> Report.toLog
    File.WriteAllText(sprintf "%s\\%03d.log.csv" logsFolder n, log)

open Scripts
[<EntryPoint>]
let  main argv =  
    
    let args = 
        Scripts.AdvancedArgs<_>(
           Simple_Iterations = 10000,
           Target_Size = 100000,
           DataSource_Size = 1000,
           Full_Iterations = 1,
           DataSource_Iterations = 3,
           Generator1 = Seqs.Numbers.length(1, 10),
           Generator2 =  Seqs.Numbers.length(1, 10),
           RemoveRatio = 0.6
        )

    let a = Scripts.sequential args
   // let b = Scripts.setLike args
   // let c = Scripts.mapLike args
    let tests = a // @ b @ c
    
    let tests = tests // |> List.filter (fun x -> x.Test.Name |> String.containsAny false ["Complex"])
    runTests tests
    0
        