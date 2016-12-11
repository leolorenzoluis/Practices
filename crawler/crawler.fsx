#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open System.Globalization
open FSharp.Data
open System.Collections.Generic

let urls = new HashSet<string>()

let urlsThatHaveBeenLoaded = new HashSet<string>()



let getLinks (htmlDocument : HtmlDocument) = 
            htmlDocument.Descendants["a"]
            |> Seq.choose (fun x -> x.TryGetAttribute("href")
                                    |> Option.map(fun y -> x.InnerText(), y.Value()))

let filteredLinks (links : seq<string * string>) = 
            let x = links
                    |> Seq.filter(fun (name, link) -> (link.StartsWith("/catalogue") && not (link.Contains("/catalogue/api")) && not (link.Contains("dataset/?")) && not (link.EndsWith("/"))) || link.Contains("googleapis"))
            
            x
            |> Seq.map (fun (name, link) -> urls.Add(link))
            |> Seq.toList
            |> printfn "HELLO %A"
            x


// let test = "http://storage.googleapis.com/amt-dgph.appspot.com/uploads/FUQbsTdH0yKGigoLpIwV/budget_operation_statement_municipality_2000.csv"

// let y = (test.StartsWith("/catalogue") && not (test.Contains("/catalogue/api")) && not (test.Contains("dataset/?"))) || test.Contains("googleapis")

let loadData (item : string * string)  = 
  let url = "http://data.gov.ph" + (snd item)
  printfn "======= LOADING %A ========" url
  HtmlDocument.Load(url)
  |> getLinks
  |> filteredLinks
                                                    

let rec getData (item : string * string) = 
  match item with
  | (x , y) when x = "CSV" || x = "PDF" || x = "XML" || x = "JSON"
                -> for y in loadData(x,y) do
                              getData(y)
  | _ -> printfn "Ignore"
    

// Configure the type provider
type DataGovType = 
  HtmlProvider<"http://data.gov.ph/catalogue/dataset">


let dataGov = DataGovType.GetSample()



let rec getData2 (item : string * string) =
  let url = snd item
  let isLoaded = urlsThatHaveBeenLoaded.Add(url)
  match isLoaded with
  | true -> printfn "Skipping, %A is already loaded" url
  | false -> HtmlDocument.Load(url) |> getLinks |> Seq.map (getData2) 

let interestingLinks = 
  getLinks(dataGov.Html)
  |> getData2 
  //|> filteredLinks

for a in interestingLinks do
  getData(a) |> ignore

for url in urls do
  // if url.Contains("google") then
    printfn "%A" url