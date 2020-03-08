using System;
using FunctionsAppV3.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunctionsAppV3.Functions
{
    public sealed class MyHttpTriggerFunction
    {
        private readonly ILogger<MyHttpTriggerFunction> _logger;
        private readonly IOptions<MyOptions> _options;

        public MyHttpTriggerFunction(ILogger<MyHttpTriggerFunction> logger, IOptions<MyOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        [FunctionName("MyHttpTriggerFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("RunAsync HttpTrigger executed at: {now}", DateTime.Now);

            _logger.LogInformation($"Request : {req.Method} / options : {_options.Value.A}");

            return new OkObjectResult("test");
        }
    }
}