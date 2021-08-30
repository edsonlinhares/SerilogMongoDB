using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TesteSeriLogMongo.Filters
{
    public class CustomActionFilter : IActionFilter
    {
        private Stopwatch stopWatch = new Stopwatch();
        private IDictionary<string, object> _ActionArguments;

        private readonly ILogger<CustomActionFilter> _logger;

        public CustomActionFilter(ILogger<CustomActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            var result = context.Result as Microsoft.AspNetCore.Mvc.ObjectResult;

            _logger.LogInformation(
                new EventId(23, "ApiLogs"),
                "{Metodo}{Tempo} {Request} {Response}",
                context.HttpContext.Request.Path,
                elapsedTime,
                JsonConvert.SerializeObject(_ActionArguments),
                result != null ? JsonConvert.SerializeObject(result.Value) : string.Empty);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            stopWatch.Start();

            _ActionArguments = context.ActionArguments;
        }
    }
}
