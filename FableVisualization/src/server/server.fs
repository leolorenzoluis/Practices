open System.IO
open Suave
open Suave.Filters
open Suave.Operators
let app : WebPart =
  choose [
    GET >=> path "/" >=> Files.file "./src/client/dist/index.html"
    GET >=> Files.browseHome
    RequestErrors.NOT_FOUND "Page not found." 
  ]
let config =
  { defaultConfig with homeFolder = Some (Path.GetFullPath "./src/client/dist") }

startWebServer config app