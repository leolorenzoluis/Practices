open Suave                 // always open suave
open Suave.Successful      // for OK-result
open Suave.Web             // for config

let a = 5 + 5
let test = [1..100]
printfn "%A" a

let aa = ['a'..'z']

for i in aa do
    printfn "%A" i

startWebServer defaultConfig (OK "Hello World!")
