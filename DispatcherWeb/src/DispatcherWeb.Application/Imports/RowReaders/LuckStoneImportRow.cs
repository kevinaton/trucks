using CsvHelper;
using DispatcherWeb.Infrastructure;
using System;
using System.Linq;

namespace DispatcherWeb.Imports.RowReaders
{
    public class LuckStoneImportRow : ImportRow
    {
        private const string ColumnPrefix = "Haultickets_";

        public LuckStoneImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }
        
        public DateTime? TicketDateTime => GetDate(ColumnPrefix + "TicketDateTime", true);
        public string HaulerRef => GetString(ColumnPrefix + "HaulerRef", EntityStringFieldLengths.LuckStoneEarnings.HaulerRef);
        public string Site => GetString(ColumnPrefix + "Site", EntityStringFieldLengths.LuckStoneEarnings.Site);
        public string CustomerName => GetString(ColumnPrefix + "CustomerName", EntityStringFieldLengths.LuckStoneEarnings.CustomerName);
        public string LicensePlate => GetString(ColumnPrefix + "Licenseplate", EntityStringFieldLengths.LuckStoneEarnings.LicensePlate);
        public decimal? HaulPaymentRate => GetDecimal(ColumnPrefix + "HaulPaymentRate", true);
        public string HaulPaymentRateUom => GetString(ColumnPrefix + "HaulPaymentRateUOM", EntityStringFieldLengths.LuckStoneEarnings.Uom);
        public decimal? NetTons => GetDecimal(ColumnPrefix + "NetTons", true);
        public decimal? FscAmount => GetDecimal(ColumnPrefix + "FSCAmount", true); //not included in HaulPayment
        public decimal? HaulPayment => GetDecimal(ColumnPrefix + "HaulPayment", true); //HaulPaymentRate * NetTons
        public string ProductDescription => GetString(ColumnPrefix + "ProductDescription", EntityStringFieldLengths.LuckStoneEarnings.ProductDescription);
        public int? LuckStoneTicketId => GetInt(ColumnPrefix + "TicketID", true);
    }
}
