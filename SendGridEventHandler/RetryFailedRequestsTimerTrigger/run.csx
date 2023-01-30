#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

public static void Run(TimerInfo myTimer, ILogger log)
{
    RunAsync(myTimer, log).GetAwaiter().GetResult();
}

public static async Task RunAsync(TimerInfo myTimer, ILogger log)
{
    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
    var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    var queue = CloudStorageAccount
        .Parse(storageConnectionString)
        .CreateCloudQueueClient()
        .GetQueueReference("failedrequestsqueue");
    
    var message = await queue.GetMessageAsync();
    
    if (message == null)
    {
        log.LogInformation("No new messages in the queue");
        return;
    }

    var httpClient = new HttpClient();

    while (message != null)
    {
        log.LogInformation(message.AsString);
        log.LogInformation("DequeueCount: " + message.DequeueCount);
        try
        {
            var request = JsonConvert.DeserializeObject<FailedTrackEventRequest>(message.AsString);
            var content = new StringContent(request.Content);
            var response = await httpClient.PostAsync(request.Url, content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            log.LogInformation("Response string: " + responseString);

            await queue.DeleteMessageAsync(message);
        }
        catch
        {
        }
        
        message = await queue.GetMessageAsync();
    }
}

public class FailedTrackEventRequest
{
    public string Url { get; set; }
    public string Content { get; set; }
    public string ExceptionMessage { get; set; }
}