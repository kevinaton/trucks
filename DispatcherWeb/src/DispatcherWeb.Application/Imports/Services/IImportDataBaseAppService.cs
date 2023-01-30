using System.IO;
using System.Threading.Tasks;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure.BackgroundJobs;

namespace DispatcherWeb.Imports.Services
{
    public interface IImportDataBaseAppService
    {
        ImportResultDto Import(TextReader textReader,
            ImportJobArgs args);
    }
}