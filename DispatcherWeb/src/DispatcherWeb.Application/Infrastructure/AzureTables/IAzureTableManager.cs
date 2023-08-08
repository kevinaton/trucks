using System.Threading.Tasks;
using Azure.Data.Tables;

namespace DispatcherWeb.Infrastructure.AzureTables
{
    public interface IAzureTableManager
    {
        Task<TableClient> GetTruckPositionTableClient();
    }
}
