using Abp;
using DispatcherWeb.Dispatching.Dto;

namespace DispatcherWeb.BackgroundJobs
{
    public class BatchDispatchSmsSenderBackgroundJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public SendSmsInput[] SendSmsInputs { get; set; }
    }
}
