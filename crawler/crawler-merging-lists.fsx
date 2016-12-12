let a = ['a';'b';'c']
let b = ['d';'e';'f']

let merge a b =
    (b,a)
    ||> List.fold (fun acc elem ->
        let alreadyExists = acc |> List.exists (fun item -> item = elem)
        if alreadyExists then acc
        else elem :: acc)
    |> List.rev
let merge a b =
  (b,a) 
  ||> List.fold (fun acc elem -> 
        let alreadyContains = acc |> List.exists (fun item -> item = elem)
        if alreadyContains then acc
        else elem :: acc)   
  |> List.rev
  
merge a b