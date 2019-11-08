using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SQLServer;

namespace Billing
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "Billing";

            var endpointConfiguration = new EndpointConfiguration("Billing");

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=RetailDemo;Integrated Security=True");

            transport.DefaultSchema("transport");

            transport.UseCatalogForEndpoint("Billing", "RetailDemo-NSB");
            transport.UseCatalogForEndpoint("Sales", "RetailDemo-NSB");
            transport.UseCatalogForEndpoint("Shipping", "RetailDemo-NSB");

            var routing = transport.Routing();
            routing.RegisterPublisher(eventType: typeof(OrderPlaced), publisherEndpoint: "Sales");

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=RetailDemo;Integrated Security=True"));
            persistence.SubscriptionSettings().DisableCache();

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}