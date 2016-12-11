#r "packages/Abot/lib/net40/Abot.dll"


open System

open Abot.Crawler
open Abot.Poco

let Fetch (url: string) =

    let crawler_ProcessPageCrawlCompleted (e:PageCrawlCompletedArgs) = 
        let crawledPage = e.CrawledPage
        let sc = crawledPage.HttpWebResponse.StatusCode
        if (crawledPage.WebException<>null || sc<>Net.HttpStatusCode.OK)
          then (Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri)) 
          else (Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri))
        if (String.IsNullOrEmpty(crawledPage.Content.Text))
          then (Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri))
        Console.Out.Write("hoge")

    let crawler_ProcessPageCrawlStarting (e:PageCrawlStartingArgs) = 
        let pageToCrawl = e.PageToCrawl 
        Console.WriteLine("About to crawl link {0} which was found on page {1}", 
            pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri)

    let crawler_PageLinksCrawlDisallowed(e:PageLinksCrawlDisallowedArgs) =
        let crawledPage = e.CrawledPage
        Console.WriteLine("Did not crawl the links on page {0} due to {1}", 
            crawledPage.Uri.AbsoluteUri, e.DisallowedReason)

    let crawler_PageCrawlDisallowed(e:PageCrawlDisallowedArgs) =
        let pageToCrawl = e.PageToCrawl
        Console.WriteLine("Did not crawl page {0} due to {1}", 
            pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason)

    let crawlConfig = new CrawlConfiguration();
    crawlConfig.CrawlTimeoutSeconds <- 100
    crawlConfig.MaxConcurrentThreads <- 10
    crawlConfig.MaxPagesToCrawl <- 1000
    crawlConfig.UserAgentString <- "abot v1.0 http://code.google.com/p/abot"
    crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue1", "1111")
    crawlConfig.ConfigurationExtensions.Add("SomeCustomConfigValue2", "2222")

    let crawler = new PoliteWebCrawler(crawlConfig, null, null, null, null, null, null, null, null);

    crawler.PageCrawlStartingAsync.Add (crawler_ProcessPageCrawlStarting >> ignore)
    crawler.PageCrawlCompletedAsync.Add (crawler_ProcessPageCrawlCompleted >> ignore)
    crawler.PageCrawlDisallowedAsync.Add (crawler_PageCrawlDisallowed >> ignore)
    crawler.PageLinksCrawlDisallowedAsync.Add (crawler_PageLinksCrawlDisallowed >> ignore)

    let result = crawler.Crawl(new Uri(url))
    if (result.ErrorOccurred)
      then Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
      else Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

// Normal one
let Fetch1 (url: string) =
    let read (response: Net.WebResponse) =
        use reader = new IO.StreamReader(response.GetResponseStream())
        reader.ReadToEnd()
    async {
        let request = Net.WebRequest.Create url
        let! response = Async.FromBeginEnd(request.BeginGetResponse, request.EndGetResponse)
        return read response
    }

[<EntryPoint>]
let Main argv = 
    //printfn "%A" <| Async.RunSynchronously(Fetch1 "http://www.yahoo.co.jp")
    Fetch "http://www.yahoo.co.jp"
    0