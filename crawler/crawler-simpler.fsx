open System.Text.RegularExpressions
open System.Net
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

let test = crawlPage client "http://data.gov.ph/catalogue/dataset"
Seq.toArray test