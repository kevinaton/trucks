using System.IO;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Infrastructure.BackgroundJobs;

namespace DispatcherWeb.Imports.Services
{
    public interface IImportDataBaseAppService
    {
        ImportResultDto Import(TextReader textReader,
            ImportJobArgs args);
    }
}