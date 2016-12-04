module App.Main

open System.Collections.Generic
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser

let dataset = [|1;10;20;60;40;30|]


let data = D3.Globals.select("body").selectAll("div").data(dataset)

D3.Globals.select "body" 
|> fun x -> x.selectAll "div" 
|> fun x -> x.data dataset 
|> fun x -> (unbox<D3.Selection.Update<int>> data).enter() 
|> fun x -> x.append "div"
|> fun x -> x.attr("class", U3.Case2 "bar")
|> fun x -> x.style("height", fun y _ _ -> U3.Case2 (string y+"px"))
|> ignore
