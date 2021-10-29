using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.SharePoint.Client;

namespace CMA.OIM.AzureFunctions
{
    public static class API
    {
        [FunctionName("API")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Request body: {requestBody}");

            Response response = new Response();
            try
            {
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    Request request = new Request(requestBody);
                    var payload = request.Payload;
                    switch (request.Method)
                    {
                        case "Issue.Submit":
                            Issue.Submit(response, log, payload.record, payload.documents);
                            break;
                        case "Test.List":
                            Test.List(response, log);
                            break;
                        default:
                            response.success = false;
                            response.error = "Unknown method";
                            break;
                    }
                }
                else
                {
                    response.success = false;
                    response.error = null;
                }
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error = ex.ToString();
            }
            log.LogInformation($"response.success: {response.success}");
            log.LogInformation($"response.error: {response.error}");
            return new OkObjectResult(response);
        }
    }
}
