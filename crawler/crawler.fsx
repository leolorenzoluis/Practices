#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open System.Globalization
open FSharp.Data

// Configure the type provider
type dataGovType = 
  HtmlProvider<"http://data.gov.ph/catalogue/dataset?q=&sort=score+desc%2C+metadata_modified+desc&page=4">

let dataGov = dataGovType.GetSample()

let getLinks (htmlDocument : HtmlDocument) = 
            htmlDocument.Descendants["a"]
            |> Seq.choose (fun x -> x.TryGetAttribute("href")
                                    |> Option.map(fun y -> x.InnerText(), y.Value()))


let filteredLinks (links : seq<string * string>) = 
            links 
            |> Seq.filter(fun (name, link) -> link.StartsWith("/catalogue") || not (link.Contains("/catalogue/api")))

let interestingLinks = getLinks(dataGov.Html) 
                        |> filteredLinks

for a in interestingLinks do
  printfn "======= LOADING %A ========" (snd a)
  HtmlDocument.Load("http://data.gov.ph" + (snd a))
  |> getLinks
  |> filteredLinks
  |> Seq.toList
  |> printfn "%A"