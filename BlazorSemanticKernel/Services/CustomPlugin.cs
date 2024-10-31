using System.ComponentModel;
using Microsoft.SemanticKernel;
using SimpleFeedReader;

namespace BlazorSemanticKernel.Plugins;


public class CustomPlugin
{
    //Give it the tag Kernel Function
    // Give it a description in english that the model can use to describe the method.
    [KernelFunction("get_current_time"), Description("Get the current time when the function is called")]
    public string GetCurrentTime() => DateTime.Now.ToString("HH:mm:ss");

    [KernelFunction("console_log"), Description("Used to log something to the console only when requested by the user")]
    [return: Description("Returns confirmation that the action was completed with the item that was printed to the log.")]
    public void LogToConsoleOnRequest(Kernel kernel,
     [Description("this is the phrase or word that is to be printed to the console")]string something){
        Console.WriteLine(something);
    }

    [KernelFunction("get_random_number"), Description("Get a random number between min and max")]
    public int GetRandomNumber(int min, int max) => new Random().Next(min, max + 1);

    [KernelFunction("get_news")]
    [Description("Get the current news from today's date")]
    [return: Description("Returns a list of five news articles from today with their links and a blurb about them.")]
    public List<FeedItem> GetRecentNewsArticles(Kernel kernel, string category){
        var reader = new FeedReader();
        return reader.RetrieveFeed($"https://rss.nytimes.com/services/xml/rss/nyt/{category}.xml")
                .Take(5)
                .ToList();
    }
}

// semantically sort documents automatically by scanning a document and asking it what kind of document it is
// and calling the appropriate function.
