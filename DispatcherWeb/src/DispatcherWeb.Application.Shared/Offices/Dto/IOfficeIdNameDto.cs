using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Offices.Dto
{
    public interface IOfficeIdNameDto
    {
		int OfficeId { get; set; }
		string OfficeName { get; set; }
		bool IsSingleOffice { get; set; }
	}
}
