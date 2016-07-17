open System
open System.Net
open System.Text
open System.IO

let siteRoot = @"./web"
let host = "http://localhost:8080/"

let listener (handler:(HttpListenerRequest->HttpListenerResponse->Async<unit>)) =
    let hl = new HttpListener()
    hl.Prefixes.Add host
    hl.Start()
    let task = Async.FromBeginEnd(hl.BeginGetContext, hl.EndGetContext)
    async {
        while true do
            let! context = task
            Async.Start(handler context.Request context.Response)
    } |> Async.Start

let output (req:HttpListenerRequest) =
    let file = Path.Combine(siteRoot,
                            Uri(host).MakeRelativeUri(req.Url).OriginalString)
    printfn "Requested : '%s'" file
    if (File.Exists file)
        then File.ReadAllText(file)
        else "File does not exist!"

let response (req : HttpListenerRequest) (resp : HttpListenerResponse) =
                      async {
                            let item = Encoding.UTF8.GetBytes (output req)
                            resp.ContentType <- "text/html"
                            resp.OutputStream.Write(item, 0, item.Length)
                            resp.OutputStream.Close()
                        }

listener (response)
