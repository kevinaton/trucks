using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Extensions;
using DispatcherWeb.Common.Dto;
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
                        AccountNumber = x.Customer.AccountNumber,
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
                    PONumber = x.PoNumber,
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
                        FreightRate = l.FreightRate,
                        MaterialRate = l.MaterialRate,
                        Tax = l.Tax,
                        IsTaxable = l.Item.IsTaxable,
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
                            MaterialUomId = l.Ticket.OrderLine.MaterialUomId,
                            FreightUomId = l.Ticket.OrderLine.FreightUomId,
                            TicketUomId = l.Ticket.UnitOfMeasureId,
                            TicketUomName = l.Ticket.UnitOfMeasure.Name,
                            IsOrderLineMaterialTotalOverridden = l.Ticket.OrderLine.IsMaterialPriceOverridden,
                            IsOrderLineFreightTotalOverridden = l.Ticket.OrderLine.IsFreightPriceOverridden,
                            OrderLineMaterialTotal = l.Ticket.OrderLine.MaterialPrice,
                            OrderLineFreightTotal = l.Ticket.OrderLine.FreightPrice,
                            Designation = l.Ticket.OrderLine.Designation,
                            LoadAt = l.Ticket.OrderLine.LoadAt == null ? null : new LocationNameDto
                            {
                                Name = l.Ticket.OrderLine.LoadAt.Name,
                                StreetAddress = l.Ticket.OrderLine.LoadAt.StreetAddress,
                                City = l.Ticket.OrderLine.LoadAt.City,
                                State = l.Ticket.OrderLine.LoadAt.State
                            },
                            DeliverTo = l.Ticket.OrderLine.DeliverTo == null ? null : new LocationNameDto
                            {
                                Name = l.Ticket.OrderLine.DeliverTo.Name,
                                StreetAddress = l.Ticket.OrderLine.DeliverTo.StreetAddress,
                                City = l.Ticket.OrderLine.DeliverTo.City,
                                State = l.Ticket.OrderLine.DeliverTo.State
                            },
                            Quantity = l.Ticket.Quantity,
                            HasOrderLine = l.Ticket.OrderLine != null,
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
                    materialLine.FreightRate = 0;
                    if (materialLine.Ticket != null)
                    {
                        materialLine.Ticket.FreightUomId = null;
                        materialLine.Ticket.OrderLineFreightTotal = null;
                        materialLine.Ticket.IsOrderLineFreightTotalOverridden = false;
                    }
                    materialLine.Subtotal -= materialLine.FreightExtendedAmount;
                    materialLine.ExtendedAmount -= materialLine.FreightExtendedAmount;
                    materialLine.FreightExtendedAmount = 0;
                    materialLine.IsSplitMaterialLine = true;
                    newLinesList.Add(materialLine);

                    var freightLine = invoiceLine.Clone();
                    freightLine.MaterialRate = 0;
                    if (freightLine.Ticket != null)
                    {
                        freightLine.Ticket.MaterialUomId = null;
                        freightLine.Ticket.OrderLineMaterialTotal = null;
                        freightLine.Ticket.IsOrderLineMaterialTotalOverridden = false;
                    }
                    freightLine.Subtotal -= freightLine.MaterialExtendedAmount;
                    freightLine.ExtendedAmount -= freightLine.MaterialExtendedAmount + freightLine.Tax;
                    freightLine.MaterialExtendedAmount = 0;
                    freightLine.Tax = 0;
                    //freightLine.IsTaxable = false;
                    freightLine.IsSplitFreightLine = true;
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
    }
}
