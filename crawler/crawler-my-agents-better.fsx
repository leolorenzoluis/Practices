open System
open System.Net
open Microsoft.FSharp.Control
open System.IO
open System.Collections.Concurrent
open System.Text.RegularExpressions
open System.Collections.Generic


[<Literal>]
let RegexPattern = @"<a\s+(?:[^>]*?\s+)?href=""([^#""]*)"
let urls = File.ReadLines("urls-to-process.txt")
let urlsOutsideHost = new ConcurrentDictionary<string,byte>()

let crawlPage url =
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
                
                printfn "Urls to add :%A" (Seq.length matches)
                
                for urlToAdd in matches do
                    urlsOutsideHost.TryAdd(urlToAdd, new byte()) |> ignore 
                
            with | message -> printfn "ERROR: %A" message 
     }

type UrlAgent() = 

    // create the mailbox processor
    let agent = MailboxProcessor.Start(fun inbox -> 

        // the message processing function
        let rec messageLoop () = async{

            // read a message
            let! url = inbox.Receive()

            // write it to the log
            do! crawlPage url

            // loop to top
            return! messageLoop ()
            }

        // start the loop
        messageLoop ()
        )

    // public interface
    member this.ProcessUrl url = agent.Post url

// test in isolation
let urlAgent = UrlAgent()
urlAgent.ProcessUrl "hello"


let makeTask (urlAgent : UrlAgent) url = async {
        urlAgent.ProcessUrl url
    }

let processUrls = 
    let urlAgent = new UrlAgent()

    let urlTasks = 
        new HashSet<string>(urls)
        |> Seq.map(fun x -> makeTask urlAgent x)

    urlTasks
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore
