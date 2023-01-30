using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Locations.Dto
{
    public class GetLocationForEditInput : NullableIdDto
    {
        public bool Temporary { get; set; }

        public bool MergeWithDuplicateSilently { get; set; } // will be set to true for quick location adds, so that app.createOrEditLocationModalSaved event is fired with a found duplicate info and no warning is shown to the user

        public GetLocationForEditInput(int? id, bool temporary = false, bool mergeWithDuplicateSilently = false)
        {
            Id = id;
            Temporary = temporary;
            MergeWithDuplicateSilently = mergeWithDuplicateSilently;
        }
    }
}
