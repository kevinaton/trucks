using System.Threading.Tasks;
using Azure.Data.Tables;
using DispatcherWeb.Configuration;

namespace DispatcherWeb.Infrastructure.AzureTables
{
    public class AzureTableManager : DispatcherWebDomainServiceBase, IAzureTableManager
    {
        private readonly IAppConfigurationAccessor _configurationAccessor;

        public AzureTableManager(IAppConfigurationAccessor configurationAccessor)
        {
            _configurationAccessor = configurationAccessor;
        }

        private string GetConnectionString()
        {
            var connectionString = _configurationAccessor.Configuration["Abp:AzureTableConnectionString"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }
            return _configurationAccessor.Configuration["Abp:StorageConnectionString"];
        }

        private TableServiceClient GetTableServiceClient()
        {
            return new TableServiceClient(
                connectionString: GetConnectionString()
            );
        }

        private TableClient _truckLocationTableClient = null;
        public async Task<TableClient> GetTruckPositionTableClient()
        {
            if (_truckLocationTableClient != null)
            {
                return _truckLocationTableClient;
            }

            var tableServiceClient = GetTableServiceClient();
            TableClient tableClient = tableServiceClient.GetTableClient(
                tableName: "TruckPosition"
            );

            await tableClient.CreateIfNotExistsAsync(); //todo move to startup or module postinitialize so that it doesn't needlessly run on each request

            return _truckLocationTableClient = tableClient;
        }
    }
}
