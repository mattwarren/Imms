module Imms.Tests.Performance.TestCode.General
#nowarn "66"
#nowarn "20"
open Imms.Tests
open ExtraFunctional
open System.Collections.Immutable
open System.Linq
open Imms.FSharp.Implementation
open Imms.Tests.Performance

let inline Iter iters = 
    let inline test col = 
        let s = col |> Ops.asSeq
        use mutable iterator = s.GetEnumerator()
        for i = 1 to iters do
            if (iterator.MoveNext() |> not) then 
                iterator <- s.GetEnumerator()
    simpleTest("IEnumerator", iters, test)
    
let mutable count = 0
let iterFor n v = 
    if count >= n then
        false
    else
        count <- count + 1
        true

let inline IterDirect iters = 
    let inline test col = 
        for i = 1 to iters do
            col |> Ops.iter
    simpleTest("Iterate", iters, test)
    



