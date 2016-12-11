open System.Text.RegularExpressions
open System.Net
open System.Collections.Generic
open System.IO

(*
1. Load data.gov.ph
2. Get all links (recursive)
   a) If link is relative to the domain -> Add to queue process for getting links
   b) Else add to hashset
*)

let client = new WebClient()

let crawlPage (client : WebClient) (pageUrl : string) =
  try
    printfn "Crawling: %A" pageUrl
    let pageContent = client.DownloadString(pageUrl)
    let pageLinkMatches =
      Regex.Matches(
        pageContent,
        "a href=['\"](.[^'\"]+)['\"]",
        RegexOptions.Compiled)

    pageLinkMatches
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Groups.Item(1).Value)

  with | message -> printfn "ERROR: %A" message
                    Seq.empty

let urlsOutsideHost = new HashSet<string>()
let urlsToProcess = new HashSet<string>()
let processLink (url:string) = 
    printfn "URL to Process Count: %A & URL Outside Host Count: %A" urlsToProcess.Count urlsOutsideHost.Count
    if url.StartsWith("/") then
        urlsToProcess.Add url
    else
        urlsOutsideHost.Add url 

let test = crawlPage client "http://data.gov.ph/catalogue/dataset"


let rec processLinks (links : seq<string>) (fileWriter : StreamWriter) =
    for url in links do
        if url.StartsWith("/") then
            if (urlsToProcess.Add url) then
                crawlPage client ("http://data.gov.ph" + url)
                |> fun urls -> processLinks urls fileWriter
        else
            let isUrlNew = urlsOutsideHost.Add url
            if isUrlNew then
                async {
                    printfn "=== writing === %A" url
                    let task = fileWriter.WriteLineAsync (url)
                    do! task |> Async.AwaitIAsyncResult |> Async.Ignore
                    if task.IsFaulted then raise task.Exception
                    return ()
                }
                |> ignore
                
                

File.Delete("urls.txt")
let fileWriter = new StreamWriter("urls.txt")
fileWriter.AutoFlush <- true
#time
processLinks test fileWriter
#time
urlsToProcess.Count
urlsOutsideHost.Count

urlsOutsideHost|>Seq.toList
test