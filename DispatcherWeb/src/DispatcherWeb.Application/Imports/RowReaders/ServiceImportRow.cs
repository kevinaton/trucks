using Abp.Extensions;
using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherWeb.Imports.RowReaders
{
    public class ServiceImportRow : ImportRow
    {
        public ServiceImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public bool IsActive => GetString(ServiceColumn.IsActive, 30) == "Active" || !HasField(ServiceColumn.IsActive);
        public string Type => GetString(ServiceColumn.Type, 100);
        public string Service1 => GetString(ServiceColumn.Service1, EntityStringFieldLengths.Service.Service1);
        public string Description => GetString(ServiceColumn.Description, EntityStringFieldLengths.Service.Description);
        public string Uom => GetString(ServiceColumn.Uom, 100);
        public decimal? Price => GetDecimal(ServiceColumn.Price);

        public bool IsTaxable => GetString(ServiceColumn.IsTaxable, 20)?.ToLower().IsIn("tax", "true", "1", "y", "yes") == true;

        public string IncomeAccount => GetString(ServiceColumn.IncomeAccount, EntityStringFieldLengths.Service.IncomeAccount);
    }
}
