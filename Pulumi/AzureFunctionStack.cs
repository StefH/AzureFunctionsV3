using System;
using System.IO;
using Pulumi.Azure.AppInsights;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Constants;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;

namespace Pulumi.Azure.Function
{
    class AzureFunctionStack : Stack
    {
        public AzureFunctionStack()
        {
            bool newResourceGroup = false;
            bool newappServicePlan = false;

            ResourceGroup resourceGroup;
            if (newResourceGroup)
            {
                resourceGroup = new ResourceGroup("linux-app-service");
            }
            else
            {
                resourceGroup = ResourceGroup.Get("linux-app-service", "/subscriptions/ae9255af-d099-4cdc-90a7-241ccb29df68/resourceGroups/linux-app-service");
            }

            var storageAccount = new Account("stefsapulumi", new AccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountReplicationType = StorageAccountReplicationTypes.LocallyRedundantStorage,
                AccountTier = StorageAccountTiers.Standard
            });

            Plan appServicePlan;
            if (newappServicePlan)
            {
                appServicePlan = new Plan("linux-app-service-plan", new PlanArgs
                {
                    ResourceGroupName = resourceGroup.Name,

                    // Possible values are `Windows` (also available as `App`), `Linux`, `elastic` (for Premium Consumption) and `FunctionApp` (for a Consumption Plan).
                    Kind = AppServicePlanSkuKinds.Linux,

                    Sku = new PlanSkuArgs
                    {
                        Tier = AppServicePlanTiers.PremiumV2,
                        Size = AppServicePlanSkuSizes.PremiumV2Small
                    },

                    // For Linux, you need to change the plan to have Reserved = true property.
                    Reserved = true
                });
            }
            else
            {
                appServicePlan = Plan.Get("linux-app-service-plan", "/subscriptions/ae9255af-d099-4cdc-90a7-241ccb29df68/resourceGroups/linux-app-service/providers/Microsoft.Web/serverfarms/linux-app-service-plan");

            }

            var container = new Container("azure-function-zips", new ContainerArgs
            {
                StorageAccountName = storageAccount.Name,
                ContainerAccessType = ContainerAccessTypes.Private
            });

            string currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine($"currentDirectory = {currentDirectory}");

            var rootDirectory = Directory.GetParent(currentDirectory);
            Console.WriteLine($"rootDirectory = {rootDirectory}");

            string functionsAppPublishDirectory = Path.Combine(rootDirectory.FullName, "publish");
            Console.WriteLine($"functionsAppPublishDirectory = {functionsAppPublishDirectory}");

            var blob = new Blob("azure-function-zip", new BlobArgs
            {
                StorageAccountName = storageAccount.Name,
                StorageContainerName = container.Name,
                Type = BlobTypes.Block,

                // The published folder contains a 'zip' file
                Source = new FileArchive(functionsAppPublishDirectory)
            });

            var codeBlobUrl = SharedAccessSignature.SignedBlobReadUrl(blob, storageAccount);

            var insights = new Insights("stef-ai-fa-l-v3p", new InsightsArgs
            {
                ResourceGroupName = resourceGroup.Name,
                ApplicationType = InsightsApplicationTypes.Web
            });

            var functionApp = new FunctionApp("stef-function-app-linux-v3p", new FunctionAppArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AppServicePlanId = appServicePlan.Id,
                AppSettings =
                {
                    { "runtime", FunctionAppWorkerRuntimes.DotNet },
                    { "DOCKER_REGISTRY_SERVER_URL", "https://index.docker.io" },
                    { "WEBSITE_ENABLE_SYNC_UPDATE_SITE", "true" },
                    { "WEBSITES_ENABLE_APP_SERVICE_STORAGE", "true" },
                    { "WEBSITE_RUN_FROM_PACKAGE", codeBlobUrl },

                    /*
                     * App Insights configuration will use the APPLICATIONINSIGHTS_CONNECTION_STRING app setting if it is set.
                     * APPINSIGHTS_INSTRUMENTATIONKEY is the fallback and continues to work as-is.
                     */
                    { "APPLICATIONINSIGHTS_CONNECTION_STRING", Output.Format($"InstrumentationKey={insights.InstrumentationKey}") }
                },

                // "storage_connection_string": [DEPRECATED] Deprecated in favor of `storage_account_name` and `storage_account_access_key`
                StorageAccountName = storageAccount.Name,
                StorageAccountAccessKey = storageAccount.PrimaryAccessKey,

                // StorageConnectionString = storageAccount.PrimaryConnectionString,

                // Make sure a version 3 functionApp is created based on a 3.0 docker image
                Version = FunctionAppVersions.V3,
                OsType = FunctionAppOsTypes.Linux,
                SiteConfig = new FunctionAppSiteConfigArgs
                {
                    LinuxFxVersion = FunctionAppSiteConfigLinuxFxVersions.DotNetV3,
                    AlwaysOn = false,
                    WebsocketsEnabled = false
                },
            });

            Endpoint = Output.Format($"https://{functionApp.DefaultHostname}/api/MyHttpTriggerFunction");
        }

        [Output]
        public Output<string> Endpoint { get; private set; }
    }
}