using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using DispatcherWeb.Infrastructure.Reports;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public class InactiveTrucksTableCsv : TableCsvBase, IInactiveTrucksTable
	{
		public InactiveTrucksTableCsv(CsvWriter csv) : base(csv)
		{
		}

		public void AddRow(string divisionName, string inactiveTrucks)
		{
			_csv.WriteField(divisionName);
			_csv.WriteField(inactiveTrucks);

			_csv.NextRecord();
		}
	}
}
