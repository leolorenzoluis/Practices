#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open System.Globalization
open FSharp.Data

// Configure the type provider
type DataGov = 
  HtmlProvider<"http://google.com">

let sample = DataGov.GetSample()
