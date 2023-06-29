
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapters.filters
{
    public class LoggingMiddleware : IFunctionsWorkerMiddleware
    {

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {

            var correlationId = new Guid().ToString();
            var businessProcess = "Unknown";

            //Extract correlation id and businessProcess from http input
            if (context.FunctionDefinition.InputBindings.Values.First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger")
            {
                var httpRequestData = await context.GetHttpRequestDataAsync();
                var corrId = httpRequestData.Headers.GetValues("correlation-id").FirstOrDefault();
                if (corrId != null)
                {
                    correlationId = corrId;
                }
                var businessProcessHeader = httpRequestData.Headers.GetValues("business-process").FirstOrDefault();
                if (businessProcessHeader != null)
                {
                    businessProcess = businessProcessHeader;
                }
            }
            else   //Extract correlation id and businessProcess from EventData
            {
                /*
                    var eventData = "";
                    if (eventData.Properties["traceparent"] != null)
                    {
                        traceId = ((string)eventData.Properties["traceparent"]).Split("-")[0];
                        parrentId = ((string)eventData.Properties["traceparent"]).Split("-")[1];
                    }
                    if (eventData.Properties["businessProcess"] != null)
                    {
                        businessProcess = ((string)eventData.Properties["business-process"]);
                    }
                    */
            }

            var log = context.GetLogger<LoggingMiddleware>();
            using (log.BeginScope(new Dictionary<string, object> { ["correlationId"] = correlationId ,["businessProcess"] = businessProcess }))
            {
                try
                {
                    log.LogInformation($"{context.FunctionDefinition.Name} started");
                    await next(context);
                    log.LogInformation($"{context.FunctionDefinition.Name} completed");
                }
                catch (Exception ex)
                {
                    log.LogError($"{context.FunctionDefinition.Name} failed", ex.Message);
                    throw ex;
                }
            }




        }


    }
}
