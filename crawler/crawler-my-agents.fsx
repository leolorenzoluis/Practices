open System
open System.Net
open Microsoft.FSharp.Control
open System.IO
open System.Collections.Concurrent
open System.Text.RegularExpressions
open System.Collections.Generic

let urls = File.ReadLines("urls-to-process.txt")

//let queue = new ConcurrentQueue<string>(urls)

let urlsOutsideHost = new HashSet<string>()
[<Literal>]
let RegexPattern = @"<a\s+(?:[^>]*?\s+)?href=""([^#""]*)"
let client = new WebClient()

type Message(id, url) =
    static let mutable count = 0
    member this.ID = id
    member this.Url = url
    static member CreateMessage(url) =
        count <- count + 1
        Message(count, url)


let crawlPage (client : WebClient) (pageUrl : string) =
  try
    let pageContent = client.DownloadString(pageUrl)
    let pageLinkMatches =
      Regex.Matches(
        pageContent,
        RegexPattern,
        RegexOptions.Compiled)

    pageLinkMatches
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Groups.Item(1).Value)

  with | message -> printfn "ERROR: %A" message
                    Seq.empty

// let agent = new MailboxProcessor<Message>(fun inbox ->
//     let rec loop count =
//         async { let! msg = inbox.Receive()
                
//                 printfn "Message received. ID: %d.\nTrying to dequeue... %A" msg.ID msg.Url
//                 let urls = crawlPage client ("http://data.gov.ph" + (msg.Url))
//                 for x in urls do
//                     urlsOutsideHost.Add x |> ignore
//                 return! loop( count + 1) }
//     loop 0)

// agent.Start()

// let agents = 
//         [
//             for i in 1 .. 23000 do
//                 yield agent
//         ]

let concurrentBag = new ConcurrentBag<string>()

let crawlPage2 url =
     async { 
            try 
                let req = WebRequest.Create(Uri("http://data.gov.ph" + url)) 
                printfn "Downloading %A" req.RequestUri
                use! resp = req.AsyncGetResponse()  // new keyword "use!"  
                use stream = resp.GetResponseStream() 
                use reader = new IO.StreamReader(stream) 
                let html = reader.ReadToEnd() 
                let pageLinkMatches = Regex.Matches(html,
                                                    RegexPattern,
                                                    RegexOptions.Compiled)
                                                    
                let matches = pageLinkMatches
                                |> Seq.cast<Match>
                                |> Seq.map (fun m -> (m.Groups.Item(1).Value))

                for x in matches do
                    printfn "Urls outside host count: %A" concurrentBag.Count
                    concurrentBag.Add x |> ignore 
                
            with | message -> printfn "ERROR: %A" message 
     }
                

// let fetchLinks url = 
//     async { 
//         let urls = crawlPage client ("http://data.gov.ph" + (url))
//         for x in urls do
//             urlsOutsideHost.Add x |> ignore
//     }

let test = new HashSet<string>(urls)
#time
test
|> Seq.toList
|> List.map crawlPage2
|> Async.Parallel
|> Async.RunSynchronously
#time


// agents 
// |> List.iter (fun agent ->  let value = queue.TryDequeue()
//                             match value with
//                             | true, url -> agent.Post(Message.CreateMessage(url))
//                             | _ -> printfn "Done")
    

let f = new FileInfo("urls-to-process-using-concurrent-bag-distinct.txt")
f.Length


let fileWriter2 = new StreamWriter("urls-to-process-using-concurrent-bag-distinct-interesting.txt")
fileWriter2.AutoFlush <- true


let interestingFiles (url : string) = 
 url.EndsWith("json") || url.EndsWith("csv") || url.EndsWith("pdf") || url.EndsWith("htm") || url.EndsWith("html")

#time
concurrentBag
  |> Seq.distinct
  |> Seq.filter(fun x -> not (x.Contains("share")) && interestingFiles(x))
  |> Seq.map fileWriter2.WriteLine
  |> Seq.toList
#time
urlsOutsideHost.Count

agent.Post(Message.CreateMessage("ABC"))
agent.Post(Message.CreateMessage("XYZ"))

urlsOutsideHost.Count
queue.Count

Seq.length urls
