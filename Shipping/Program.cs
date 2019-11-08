using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Messages;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SQLServer;

namespace Shipping
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "Shipping";

            var endpointConfiguration = new EndpointConfiguration("Shipping");

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=RetailDemo;Integrated Security=True");

            transport.DefaultSchema("transport");

            var routing = transport.Routing();
            routing.RegisterPublisher(eventType: typeof(OrderPlaced), publisherEndpoint: "Sales");
            routing.RegisterPublisher(eventType: typeof(OrderBilled), publisherEndpoint: "Billing");

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