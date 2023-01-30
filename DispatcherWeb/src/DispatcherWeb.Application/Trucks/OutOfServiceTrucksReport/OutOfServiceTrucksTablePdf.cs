﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Reports;
using MigraDoc.DocumentObjectModel;

namespace DispatcherWeb.Trucks.OutOfServiceTrucksReport
{
    public class OutOfServiceTrucksTablePdf : StandardTablePdf, IOutOfServiceTrucksTable
    {
		public OutOfServiceTrucksTablePdf(Section section) : base(section, new []{ 3.0, 3.0, 3.0, 9.5 })
		{
		}

		public void AddRow(
			string truckNumber, 
			string outOfServiceDate, 
			string numberOfDaysOutOfService, 
			string reason
		)
		{
            //truckNumber = String.Join((char) 0x00AD, truckNumber.ToCharArray());
            truckNumber = Regex.Replace(truckNumber, ".{17}", "$0 ");

            base.AddRow(truckNumber, outOfServiceDate, numberOfDaysOutOfService, reason);
		}
	}
}
