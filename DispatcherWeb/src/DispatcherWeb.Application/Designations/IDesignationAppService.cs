using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Designations
{
    public interface IDesignationAppService
    {
        Task<IEnumerable<SelectListItem>> GetDesignationSelectListItemsAsync(DesignationEnum? selectedDesignation = null);
    }
}
