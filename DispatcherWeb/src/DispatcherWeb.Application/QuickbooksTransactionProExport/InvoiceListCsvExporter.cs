﻿using Abp.Configuration;
using DispatcherWeb.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.QuickbooksOnline.Dto;
using DispatcherWeb.Storage;
using System.Linq;

namespace DispatcherWeb.QuickbooksTransactionProExport
{
    public class InvoiceListCsvExporter : CsvExporterBase, IInvoiceListCsvExporter
    {
        private readonly ISettingManager _settingManager;

        public InvoiceListCsvExporter(
            ISettingManager settingManager,
            ITempFileCacheManager tempFileCacheManager
            ) : base(tempFileCacheManager)
        {
            _settingManager = settingManager;
        }

        public async Task<FileDto> ExportToFileAsync<T>(List<InvoiceToUploadDto<T>> invoiceList, string filename)
        {
            var invoiceNumberPrefix = await _settingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);

            return CreateCsvFile(
                filename + ".csv",
                () =>
                {
                    var flatData = invoiceList.SelectMany(x => x.InvoiceLines, (invoice, invoiceLine) => new
                    {
                        //IsFirstRow = invoice.InvoiceLines.IndexOf(invoiceLine) == 0,
                        Invoice = invoice,
                        InvoiceLine = invoiceLine
                    }).ToList();

                    AddHeaderAndData(flatData,
                        ("RefNumber", x => invoiceNumberPrefix + x.Invoice.InvoiceId),
                        ("Customer", x => x.Invoice.Customer.Name),
                        ("TxnDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("DueDate", x => x.Invoice.DueDate?.ToString("d")),
                        ("ShipDate", x => ""),
                        ("ShipMethodName", x => ""),
                        ("TrackingNum", x => ""),
                        ("SalesTerm", x => x.Invoice.Terms?.GetDisplayName()),
                        ("Location", x => ""),
                        ("Class", x => ""),
                        ("BillAddrLine1", x => x.Invoice.Customer.BillingAddress.Address1),
                        ("BillAddrLine2", x => x.Invoice.Customer.BillingAddress.Address2),
                        ("BillAddrLine3", x => ""),
                        ("BillAddrLine4", x => ""),
                        ("BillAddrCity", x => x.Invoice.Customer.BillingAddress.City),
                        ("BillAddrState", x => x.Invoice.Customer.BillingAddress.State),
                        ("BillAddrPostalCode", x => x.Invoice.Customer.BillingAddress.ZipCode),
                        ("BillAddrCountry", x => x.Invoice.Customer.BillingAddress.CountryCode),
                        ("ShipAddrLine1", x => x.Invoice.Customer.ShippingAddress.Address1),
                        ("ShipAddrLine2", x => x.Invoice.Customer.ShippingAddress.Address2),
                        ("ShipAddrLine3", x => ""),
                        ("ShipAddrLine4", x => ""),
                        ("ShipAddrCity", x => x.Invoice.Customer.ShippingAddress.City),
                        ("ShipAddrState", x => x.Invoice.Customer.ShippingAddress.State),
                        ("ShipAddrPostalCode", x => x.Invoice.Customer.ShippingAddress.ZipCode),
                        ("ShipAddrCountry", x => x.Invoice.Customer.ShippingAddress.CountryCode),
                        ("PrivateNote", x => ""),
                        ("Msg", x => x.Invoice.Message),
                        ("BillEmail", x => x.Invoice.EmailAddress),
                        ("BillEmailCc", x => ""),
                        ("BillEmailBcc", x => ""),
                        ("Currency", x => ""),
                        ("ExchangeRate", x => ""),
                        ("Deposit", x => ""),
                        ("ToBePrinted", x => "N"),
                        ("ToBeEmailed", x => "N"),
                        ("AllowIPNPayment", x => "N"),
                        ("AllowOnlineCreditCardPayment", x => "N"),
                        ("AllowOnlineACHPayment", x => "N"),
                        ("ShipAmt", x => ""),
                        ("ShipItem", x => ""),
                        ("DiscountAmt", x => ""),
                        ("DiscountRate", x => ""),
                        ("TaxRate", x => ""),
                        ("TaxAmt", x => ""),
                        ("DiscountTaxable", x => "N"),
                        ("LineServiceDate", x => x.Invoice.IssueDate?.ToString("d")),
                        ("LineItem", x => x.InvoiceLine.ItemName),
                        ("LineDesc", x => ""),
                        ("LineQty", x => x.InvoiceLine.Quantity.ToString()),
                        ("LineUnitPrice", x => (x.InvoiceLine.Subtotal / x.InvoiceLine.Quantity).ToString()),
                        ("LineAmount", x => x.InvoiceLine.Subtotal.ToString()),
                        ("LineClass", x => ""),
                        ("LineTaxable", x => x.InvoiceLine.IsTaxable == true ? "Y" : "N"),
                        ("Crew #", x => "")
                    );

                });
        }
    }
}
