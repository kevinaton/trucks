using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.QuickbooksOnline.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.QuickbooksOnline
{
    public static class QuickbooksExtensions
    {
        public async static Task<List<InvoiceToUploadDto<Invoices.Invoice>>> ToInvoiceToUploadList(this IQueryable<Invoices.Invoice> query, string timezone)
        {
            var invoicesToUpload = await query
                .Select(x => new InvoiceToUploadDto<Invoices.Invoice>
                {
                    Invoice = x,
                    InvoiceId = x.Id,
                    BillingAddress = x.BillingAddress,
                    EmailAddress = x.EmailAddress,
                    Terms = x.Terms,
                    Customer = new CustomerToUploadDto
                    {
                        Id = x.CustomerId,
                        Name = x.Customer.Name,
                        IsInQuickBooks = x.Customer.IsInQuickBooks,
                        InvoiceEmail = x.Customer.InvoiceEmail,
                        InvoicingMethod = x.Customer.InvoicingMethod,
                        BillingAddress = new PhysicalAddressDto
                        {
                            Address1 = x.Customer.BillingAddress1,
                            Address2 = x.Customer.BillingAddress2,
                            City = x.Customer.BillingCity,
                            CountryCode = x.Customer.BillingCountryCode,
                            State = x.Customer.BillingState,
                            ZipCode = x.Customer.BillingZipCode,
                        },
                        ShippingAddress = new PhysicalAddressDto
                        {
                            Address1 = x.Customer.Address1,
                            Address2 = x.Customer.Address2,
                            City = x.Customer.City,
                            CountryCode = x.Customer.CountryCode,
                            State = x.Customer.State,
                            ZipCode = x.Customer.ZipCode
                        }
                    },
                    DueDate = x.DueDate,
                    IssueDate = x.IssueDate,
                    Message = x.Message,
                    Tax = x.Tax,
                    TaxRate = x.TaxRate,
                    TotalAmount = x.TotalAmount,
                    InvoiceLines = x.InvoiceLines.Select(l => new InvoiceLineToUploadDto
                    {
                        DeliveryDateTime = l.DeliveryDateTime,
                        Description = l.Description,
                        Subtotal = l.Subtotal,
                        ExtendedAmount = l.ExtendedAmount,
                        FreightExtendedAmount = l.FreightExtendedAmount,
                        MaterialExtendedAmount = l.MaterialExtendedAmount,
                        Tax = l.Tax,
                        IsTaxable = l.Item.IsTaxable,
                        PONumber = l.Ticket.OrderLine.Order.PONumber,
                        JobNumber = l.JobNumber,
                        LeaseHaulerName = l.Ticket.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                        LineNumber = l.LineNumber,
                        TicketNumber = l.Ticket.TicketNumber,
                        TruckCode = l.TruckCode,
                        Quantity = l.Quantity,
                        ItemId = l.ItemId,
                        ItemName = l.Item.Service1,
                        ItemIsInQuickBooks = l.Item.IsInQuickBooks,
                        ItemType = l.Item.Type,
                        ItemIncomeAccount = l.Item.IncomeAccount,
                        //ServiceIsTaxable = l.Item.IsTaxable,
                        //ServiceHasMaterialPricing = l.Ticket.Service.OfficeServicePrices.Any(o => o.PricePerUnit > 0),
                        ChildInvoiceLineKind = l.ChildInvoiceLineKind,
                        Ticket = l.Ticket != null ? new TicketToUploadDto
                        {
                            TicketDateTimeUtc = l.Ticket.TicketDateTime,
                            //OrderMaterialPrice = l.Ticket.OrderLine.MaterialPricePerUnit,
                            //OrderFreightPrice = l.Ticket.OrderLine.FreightPricePerUnit
                        } : null,
                    }).ToList()
                }).ToListAsync();

            foreach (var invoice in invoicesToUpload)
            {
                foreach (var line in invoice.InvoiceLines)
                {
                    line.DeliveryDateTime = line.DeliveryDateTime?.ConvertTimeZoneTo(timezone);
                }

                invoice.InvoiceLines = invoice.InvoiceLines.OrderBy(x => x.LineNumber).ToList();
            }

            invoicesToUpload.ForEach(i =>
                i.InvoiceLines.RemoveAll(l =>
                    l.ChildInvoiceLineKind.IsIn(ChildInvoiceLineKind.BottomFuelSurchargeLine, ChildInvoiceLineKind.FuelSurchargeLinePerTicket)
                    && l.ExtendedAmount == 0));

            return invoicesToUpload;
        }

        public static void SplitMaterialAndFreightLines(this List<InvoiceToUploadDto<Invoices.Invoice>> invoicesToUpload)
        {
            foreach (var invoice in invoicesToUpload)
            {
                var newLinesList = new List<InvoiceLineToUploadDto>();
                foreach (var invoiceLine in invoice.InvoiceLines)
                {
                    if (invoiceLine.MaterialExtendedAmount == 0 || invoiceLine.FreightExtendedAmount == 0 || invoiceLine.IsTaxable == false)
                    {
                        //no need to split lines with only either material or freight total
                        newLinesList.Add(invoiceLine);
                        continue;
                    }

                    var materialLine = invoiceLine.Clone();
                    materialLine.Subtotal -= materialLine.FreightExtendedAmount;
                    materialLine.ExtendedAmount -= materialLine.FreightExtendedAmount;
                    materialLine.FreightExtendedAmount = 0;
                    newLinesList.Add(materialLine);

                    var freightLine = invoiceLine.Clone();
                    freightLine.Subtotal -= freightLine.MaterialExtendedAmount;
                    freightLine.ExtendedAmount -= freightLine.MaterialExtendedAmount + freightLine.Tax;
                    freightLine.MaterialExtendedAmount = 0;
                    freightLine.Tax = 0;
                    //freightLine.IsTaxable = false;
                    newLinesList.Add(freightLine);
                }

                short lineNumber = 1;
                foreach (var newLine in newLinesList)
                {
                    newLine.LineNumber = lineNumber++;
                }

                invoice.InvoiceLines = newLinesList;
            }
        }

        public static void RecalculateTotals(this InvoiceToUploadDto<Invoices.Invoice> invoiceToUpload, TaxCalculationType taxCalculationType)
        {
            foreach (var line in invoiceToUpload.InvoiceLines)
            {
                OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, line, invoiceToUpload.TaxRate);
            }
            invoiceToUpload.Tax = invoiceToUpload.InvoiceLines.Sum(x => x.Tax);
            invoiceToUpload.TotalAmount = invoiceToUpload.InvoiceLines.Sum(x => x.ExtendedAmount);
        }
    }
}
