using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.Web.Areas.App.Models.Common;

namespace DispatcherWeb.Web.Areas.App.Models.Editions
{
    [AutoMapFrom(typeof(GetEditionEditOutput))]
    public class CreateOrEditEditionModalViewModel : GetEditionEditOutput, IFeatureEditViewModel
    {
        public bool IsEditMode => Edition.Id.HasValue;

        public IReadOnlyList<ComboboxItemDto> EditionItems { get; set; }

        public IReadOnlyList<ComboboxItemDto> FreeEditionItems { get; set; }

        public CreateOrEditEditionModalViewModel(GetEditionEditOutput output, IReadOnlyList<ComboboxItemDto> editionItems, IReadOnlyList<ComboboxItemDto> freeEditionItems)
        {
            this.EditionItems = editionItems;
            this.FreeEditionItems = freeEditionItems;
            this.Edition = output.Edition;
            this.FeatureValues = output.FeatureValues;
            this.Features = output.Features;
            //output.MapTo(this);
        }
    }
}