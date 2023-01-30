using Abp;
using DispatcherWeb.Receipts.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.BackgroundJobs
{
    public class CopyReceiptsFromActualAmountsBackgroundJobArgs : SeedReceiptsFromActualAmountsInput
    {
        public UserIdentifier RequestorUser { get; set; }
    }
}
