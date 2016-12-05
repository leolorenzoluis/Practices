module App.Main

open System.Collections
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser

//let dataset = [|1;10;20;60;40;30;500;1;10;20;60;40;30;500|]

let random = new System.Random()

let dataset = Array.init 25 (fun _ -> (random.Next(3,25)))

let barHeight x = x * 5 

let barPadding = 1.
let dataSetLength = float dataset.Length

let svg = D3.Globals.select("body")
                    .append("svg")
                    .attr("width", U3.Case1 500.)
                    .attr("height", U3.Case1 500.)

svg.selectAll("rect")
    .data(dataset)
|> fun x -> (unbox<D3.Selection.Update<int>> x).enter()
|> fun x -> x.append("rect")
|> fun x -> x.attr("width", fun _ _ _ -> U3.Case1 (System.Math.Abs(500. / dataSetLength - barPadding)))
                .attr("height", fun data _ _ -> U3.Case1 (float data * 4.))
                .attr("x", fun _ x y -> printfn "X: %A Y: %A" x y
                                        U3.Case1 (x * (500./dataSetLength))) 
                .attr("y", fun data _ _ -> U3.Case1 (100. - float data * 4.))
                .attr("fill", fun data _ _ -> U3.Case2 (sprintf "rgb(25,75,%A)" (data * 10))) |> ignore
            

svg.selectAll("text")
    .data(dataset)
|> fun x -> (unbox<D3.Selection.Update<int>> x).enter()
|> fun x -> x.append("text")
|> fun x -> x.text(fun data _ _ -> U3.Case2 (string data))
             .attr("x", fun _ x _ -> U3.Case1 (x * (500./dataSetLength))) 
             .attr("y", fun data _ _ -> U3.Case1 (100. - (float data * 4.)))
|> ignore
               
let data = D3.Globals.select("body").selectAll("div").data(dataset)


D3.Globals.select "body" 
|> fun x -> x.selectAll "div" 
|> fun x -> x.data dataset 
|> fun x -> (unbox<D3.Selection.Update<int>> data).enter() 
|> fun x -> x.append "div"
|> fun x -> x.attr("class", U3.Case2 "bar")
|> fun x -> x.style("height", fun y _ _ -> U3.Case2 (string (barHeight y)+"px"))
|> ignore

printfn "%A" dataset