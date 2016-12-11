open System
open System.Threading
open System.Diagnostics

// a utility function
type Utility() = 
    static let rand = new Random()
    
    static member RandomSleep() = 
        let ms = rand.Next(100,1000)
        Thread.Sleep ms

// an implementation of a shared counter using locks
type LockedCounter () = 

    static let _lock = new Object()

    static let mutable count = 0
    static let mutable sum = 0

    static let updateState i = 
        // increment the counters and...
        sum <- sum + i
        count <- count + 1
        printfn "Count is: %i. Sum is: %i" count sum 

        // ...emulate a short delay
        Utility.RandomSleep()


    // public interface to hide the state
    static member Add i = 
        // see how long a client has to wait
        let stopwatch = new Stopwatch()
        stopwatch.Start() 

        // start lock. Same as C# lock{...}
        lock _lock (fun () ->
        
            // see how long the wait was
            stopwatch.Stop()
            printfn "Client waited %i" stopwatch.ElapsedMilliseconds

            // do the core logic
            updateState i 
            )
        // release lock

let makeCountingTask addFunction taskId  = async {
    let name = sprintf "Task%i" taskId
    for i in [1..3] do 
        addFunction i
    }

// test in isolation
let task = makeCountingTask LockedCounter.Add 1
Async.RunSynchronously task

let lockedExample5 = 
    [1..1000]
        |> List.map (fun i -> makeCountingTask LockedCounter.Add i)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore