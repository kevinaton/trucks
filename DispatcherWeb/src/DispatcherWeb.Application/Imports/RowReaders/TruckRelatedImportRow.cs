﻿using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Trucks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherWeb.Imports.RowReaders
{
    public class TruckRelatedImportRow : ImportRow, ITruckImportRow
    {
        public TruckRelatedImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public string Office => GetString(FuelUsageColumn.Office, Offices.Office.MaxNameLength);
        public string TruckNumber => GetString(FuelUsageColumn.TruckNumber, Truck.MaxTruckCodeLength);
    }
}
