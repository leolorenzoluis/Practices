#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open System.Globalization
open FSharp.Data


let getLinks (htmlDocument : HtmlDocument) = 
            htmlDocument.Descendants["a"]
            |> Seq.choose (fun x -> x.TryGetAttribute("href")
                                    |> Option.map(fun y -> x.InnerText(), y.Value()))

let filteredLinks (links : seq<string * string>) = 
            let x = links
                    |> Seq.filter(fun (name, link) -> (link.StartsWith("/catalogue") && not (link.Contains("/catalogue/api")) && not (link.Contains("dataset/?"))) || link.Contains("googleapis"))
            
            x
            |> Seq.toList
            |> printfn "HELLO %A"
            x


let test = "http://storage.googleapis.com/amt-dgph.appspot.com/uploads/FUQbsTdH0yKGigoLpIwV/budget_operation_statement_municipality_2000.csv"

let y = (test.StartsWith("/catalogue") && not (test.Contains("/catalogue/api")) && not (test.Contains("dataset/?"))) || test.Contains("googleapis")

let loadData (item : string * string)  = 
  printfn "======= LOADING http://data.gov.ph/%A ========" (snd item)
  HtmlDocument.Load("http://data.gov.ph" + (snd item))
  |> getLinks
  |> filteredLinks
                                                    

let rec getData (item : string * string) = 
  match item with
  | (x, y) -> for y in loadData(x,y) do
                getData(y)
  | _ -> printfn "DONE"
    

// Configure the type provider
type DataGovType = 
  HtmlProvider<"http://data.gov.ph/catalogue/dataset">


let dataGov = DataGovType.GetSample()

let interestingLinks = 
  getLinks(dataGov.Html) 
  |> filteredLinks

for a in interestingLinks do
  getData(a) |> ignore