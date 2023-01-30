#r "Newtonsoft.Json"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("SendGridEventHandler is processing a HTTP request.");

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    var inputList = JsonConvert.DeserializeObject<List<TrackEventInput>>(requestBody);
    
    log.LogInformation($"Received {inputList.Count} events");

    var httpClient = new HttpClient();

    foreach (var group in inputList.GroupBy(x => x.TrackableEmailCallbackUrl))
    {
        if (string.IsNullOrEmpty(group.Key))
        {
            log.LogInformation($"Skipping a group of {group.Count()} emails with empty CallbackUrl");
            continue;
        }

        log.LogInformation($"Sending a group of {group.Count()} emails to {group.Key}");
        var contentString = JsonConvert.SerializeObject(group.ToList());
        log.LogInformation("Content to send: " + contentString);
        var content = new StringContent(contentString);
        try
        {
            var response = await httpClient.PostAsync(group.Key, content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            log.LogInformation("Response string: " + responseString);
            log.LogInformation("Response status code: " + (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            log.LogInformation("Caught exception " + ex.Message);

            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var queue = CloudStorageAccount
                .Parse(storageConnectionString)
                .CreateCloudQueueClient()
                .GetQueueReference("failedrequestsqueue");

            log.LogInformation($"Adding a message to the failedrequestsqueue");

            await queue.AddMessageAsync(new CloudQueueMessage(
                JsonConvert.SerializeObject(new FailedTrackEventRequest
                {
                    Url = group.Key,
                    Content = contentString,
                    ExceptionMessage = ex.Message
                })));

            log.LogInformation($"Successfully added a message to the queue");
        }
    }

    log.LogInformation("Finished processing events");
    return new OkObjectResult("ok");
}

public class FailedTrackEventRequest
{
    public string Url { get; set; }
    public string Content { get; set; }
    public string ExceptionMessage { get; set; }
}

public class TrackEventInput
{
    [JsonProperty("trackableEmailId")]
    public Guid? TrackableEmailId { get; set; }

    public int? TrackableEmailTenantId { get; set; }

    public string TrackableEmailCallbackUrl { get; set; }

    public string Event { get; set; } //One of: bounce, deferred, delivered, dropped, processed

    [JsonProperty("email")]
    public string Email { get; set; } //Email address of the intended recipient

    public long Timestamp { get; set; } //UNIX timestamp

    [JsonProperty("sg_event_id")]
    public string SendGridEventId { get; set; }

    public string Reason { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> OtherFields;

    //public EmailDeliveryStatus DeliveryStatus
    //{
    //    get
    //    {
    //        switch (Event)
    //        {
    //            case "processed": return EmailDeliveryStatus.Processed;
    //            case "dropped": return EmailDeliveryStatus.Dropped;
    //            case "deferred": return EmailDeliveryStatus.Deferred;
    //            case "delivered": return EmailDeliveryStatus.Delivered;
    //            case "bounce": return EmailDeliveryStatus.Bounced;
    //            case "open": return EmailDeliveryStatus.Opened;
    //            default: return EmailDeliveryStatus.NotProcessed;
    //        }
    //    }
    //}
}

