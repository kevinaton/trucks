using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public interface IInactiveTrucksTable : IAddColumnHeaders
    {
		void AddRow(string divisionName, string inactiveTrucks);
    }
}
