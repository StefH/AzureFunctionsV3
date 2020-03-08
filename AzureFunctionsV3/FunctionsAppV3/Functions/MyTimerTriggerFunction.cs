using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionsAppV3.Functions
{
    public sealed class MyTimerTriggerFunction
    {
        private readonly ILogger<MyTimerTriggerFunction> _logger;

        public MyTimerTriggerFunction(ILogger<MyTimerTriggerFunction> logger)
        {
            _logger = logger;
        }

        [FunctionName("MyTimerTriggerFunction")]
        public Task RunAsync([TimerTrigger("%MyOptions:Cron%")] TimerInfo myTimer)
        {
            _logger.LogInformation("MyTimerTriggerFunction function executed at: {now}", DateTime.Now);

            return Task.CompletedTask;
        }
    }
}