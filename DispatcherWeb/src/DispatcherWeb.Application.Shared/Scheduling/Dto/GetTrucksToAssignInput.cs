using System;
using System.Collections.Generic;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetTrucksToAssignInput : SortedInputDto, IGetScheduleInput, IShouldNormalize
    {
        public int OrderLineId { get; set; }
        public List<int> VehicleCategoryIds { get; set; }
        public string DriverName { get; set; }
        public BedConstructionEnum? BedConstruction { get; set; }
        public bool? IsApportioned { get; set; }

        public int OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TruckCode";
            }
        }
    }
}
