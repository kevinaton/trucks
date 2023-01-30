using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public interface IOutOfServiceTrucksTable : IAddColumnHeaders
    {
		void AddRow(
			string truckNumber,
			string outOfServiceDate,
			string numberOfDaysOutOfService,
			string reason
		);
    }
}
