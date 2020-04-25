using System.Threading.Tasks;
using Pulumi;
using Pulumi.Azure.Function;

class Program
{
    static Task<int> Main(string[] args)
    {
        // System.Diagnostics.Debugger.Launch();

        return Deployment.RunAsync<AzureFunctionStack>();
    }
}