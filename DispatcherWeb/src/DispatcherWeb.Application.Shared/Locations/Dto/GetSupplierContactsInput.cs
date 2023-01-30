﻿using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Locations.Dto
{
    public class GetSupplierContactsInput : SortedInputDto, IShouldNormalize
    {
        public int LocationId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
