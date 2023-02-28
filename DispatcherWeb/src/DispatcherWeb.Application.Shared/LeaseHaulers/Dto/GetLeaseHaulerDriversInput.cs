﻿using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class GetLeaseHaulerDriversInput : SortedInputDto, IShouldNormalize
    {
        public int LeaseHaulerId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "LastName,FirstName";
            }
        }
    }
}
