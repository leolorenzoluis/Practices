open System
open System.Net
open System.Text
open System.IO
open System.Security.Cryptography

let aesCSP = new AesCryptoServiceProvider()
aesCSP.GenerateKey()
aesCSP.GenerateIV()

// AES Encryption ////
let Encrypt (text : string) =
    let textBytes = Encoding.UTF8.GetBytes (text)
    let encryptor = aesCSP.CreateEncryptor()
    encryptor.TransformFinalBlock (textBytes, 0, textBytes.Length)

let Decrypt (value : byte[]) =
    let decryptor = aesCSP.CreateDecryptor()
    let decryptBytes = decryptor.TransformFinalBlock (value, 0, value.Length)
    Encoding.UTF8.GetString decryptBytes

let siteRoot = @"./web"
let host = "http://localhost:8080/"

let listener (handler:(HttpListenerRequest->HttpListenerResponse->Async<unit>)) =
    // Understand HttpListener
    // http://referencesource.microsoft.com/#System/net/System/Net/HttpListener.cs
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
                            let cookies : seq<Cookie> = Seq.cast req.Cookies
                            printfn "Cookies count: %i" (Seq.length cookies)

                            for cookieInRequest in cookies do
                              let decryptedValue =
                                Convert.FromBase64String cookieInRequest.Value
                                |> Decrypt

                              printfn "The value is %s" decryptedValue

                            let sampleValue = Encrypt "HALO"
                            let cookie = new Cookie()
                            cookie.Expires <- DateTime.Now.AddSeconds(5.)
                            cookie.Value <- Convert.ToBase64String sampleValue
                            cookie.Name <- "Puta"
                            cookie.Secure <- true
                            cookie.Comment <- "Leche"

                            resp.Cookies.Add cookie
                            resp.ContentType <- "text/html"
                            resp.OutputStream.Write(item, 0, item.Length)
                            resp.OutputStream.Close()
                        }

listener (response)
