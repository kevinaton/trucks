using System;
using System.IO;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Events.Bus;
using Abp.Timing;
using DispatcherWeb.Imports.Services;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.EventBus.Events;
using DispatcherWeb.Infrastructure.Utilities;

namespace DispatcherWeb.Infrastructure.BackgroundJobs
{
    public class ImportJob : AsyncBackgroundJob<ImportJobArgs>, ITransientDependency
    {
        private readonly IIocResolver _iocResolver;
        private readonly ISecureFileBlobService _secureFileBlobService;

        public ImportJob(
            IIocResolver iocResolver,
            ISecureFileBlobService secureFileBlobService
        )
        {
            _iocResolver = iocResolver;
            _secureFileBlobService = secureFileBlobService;

            EventBus = NullEventBus.Instance;
        }
        public IEventBus EventBus { get; set; }

        public async override Task ExecuteAsync(ImportJobArgs args)
        {
            try
            {
                using (var fileStream = AttachmentHelper.GetStreamFromAzureBlob(args.File))
                using (TextReader textReader = new StreamReader(fileStream))
                {
                    ImportServiceFactory.OfficeResolverType officeResolverType = args.JacobusEnergy ? ImportServiceFactory.OfficeResolverType.ByFuelId : ImportServiceFactory.OfficeResolverType.ByName;
                    IImportDataBaseAppService importAppService = ImportServiceFactory.GetImportAppService(_iocResolver, args.ImportType, officeResolverType);
                    try
                    {
                        DateTime startDateTime = Clock.Now;
                        var result = importAppService.Import(
                            textReader,
                            args);
                        DateTime endDateTime = Clock.Now;

                        string resultJsonString = Utility.Serialize(result);
                        await _secureFileBlobService.AddChildBlob(args.File, SecureFileChildFileNames.ImportResult, resultJsonString);

                        try
                        {
                            EventBus.Trigger(new ImportCompletedEventData(args));
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error when triggering the ImportCompletedEventData event: {e}");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error in the main try block: {e}");
                        EventBus.Trigger(new ImportFailedEventData(args));
                    }
                    finally
                    {
                        _iocResolver.Release(importAppService);
                    }
                }

            }
            catch (Exception e)
            {

                Logger.Error($"Error in the ImportJob.Execute method: {e}");
            }
        }
    }
}
