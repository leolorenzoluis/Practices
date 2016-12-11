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
        let sanitizedUrl = url.Replace("amp=&amp;","")
        if sanitizedUrl.StartsWith("/") then
            if (urlsToProcess.Add sanitizedUrl) then
                printfn "Url to process count: %A\nCrawling: %A" urlsToProcess.Count sanitizedUrl
                crawlPage client ("http://data.gov.ph" + sanitizedUrl)
                |> fun urls -> processLinks urls fileWriter
        else
            let isUrlNew = urlsOutsideHost.Add sanitizedUrl
            if isUrlNew then
                // async {
                //     printfn "=== writing === %A" sanitizedUrl
                //     let task = fileWriter.WriteLineAsync (sanitizedUrl)
                //     do! task |> Async.AwaitIAsyncResult |> Async.Ignore
                //     if task.IsFaulted then raise task.Exception
                //     return ()
                // }
                // |> Async.Start
                fileWriter.WriteLine (sanitizedUrl) |> ignore
                
                

File.Delete("urls.txt")
let fileWriter = new StreamWriter("urls.txt")
fileWriter.AutoFlush <- true
#time