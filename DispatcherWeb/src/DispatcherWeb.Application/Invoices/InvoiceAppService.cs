using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Dto;
using DispatcherWeb.Emailing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices.Dto;
using DispatcherWeb.Invoices.Reports;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Services;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;

namespace DispatcherWeb.Invoices
{
    [AbpAuthorize]
    public class InvoiceAppService : DispatcherWebAppServiceBase, IInvoiceAppService
    {
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<InvoiceBatch> _invoiceBatchRepository;
        private readonly IRepository<InvoiceLine> _invoiceLineRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<InvoiceEmail> _invoiceEmailRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly InvoicePrintOutGenerator1 _invoicePrintOutGenerator1;
        private readonly InvoicePrintOutGenerator2 _invoicePrintOutGenerator2;
        private readonly InvoicePrintOutGenerator3 _invoicePrintOutGenerator3;
        private readonly InvoicePrintOutGenerator4 _invoicePrintOutGenerator4;
        private readonly InvoicePrintOutGenerator5 _invoicePrintOutGenerator5;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly ICrossTenantOrderSender _crossTenantOrderSender;

        public InvoiceAppService(
            IRepository<Invoice> invoiceRepository,
            IRepository<InvoiceBatch> invoiceBatchRepository,
            IRepository<InvoiceLine> invoiceLineRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Customer> customerRepository,
            IRepository<InvoiceEmail> invoiceEmailRepository,
            IRepository<Service> serviceRepository,
            OrderTaxCalculator orderTaxCalculator,
            IBinaryObjectManager binaryObjectManager,
            InvoicePrintOutGenerator1 invoicePrintOutGenerator1,
            InvoicePrintOutGenerator2 invoicePrintOutGenerator2,
            InvoicePrintOutGenerator3 invoicePrintOutGenerator3,
            InvoicePrintOutGenerator4 invoicePrintOutGenerator4,
            InvoicePrintOutGenerator5 invoicePrintOutGenerator5,
            ITrackableEmailSender trackableEmailSender,
            ICrossTenantOrderSender crossTenantOrderSender
            )
        {
            _invoiceRepository = invoiceRepository;
            _invoiceBatchRepository = invoiceBatchRepository;
            _invoiceLineRepository = invoiceLineRepository;
            _ticketRepository = ticketRepository;
            _customerRepository = customerRepository;
            _invoiceEmailRepository = invoiceEmailRepository;
            _serviceRepository = serviceRepository;
            _orderTaxCalculator = orderTaxCalculator;
            _binaryObjectManager = binaryObjectManager;
            _invoicePrintOutGenerator1 = invoicePrintOutGenerator1;
            _invoicePrintOutGenerator2 = invoicePrintOutGenerator2;
            _invoicePrintOutGenerator3 = invoicePrintOutGenerator3;
            _invoicePrintOutGenerator4 = invoicePrintOutGenerator4;
            _invoicePrintOutGenerator5 = invoicePrintOutGenerator5;
            _trackableEmailSender = trackableEmailSender;
            _crossTenantOrderSender = crossTenantOrderSender;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<PagedResultDto<InvoiceDto>> GetInvoices(GetInvoicesInput input)
        {
            var query = _invoiceRepository.GetAll()
                .WhereIf(input.InvoiceId.HasValue,
                    x => x.Id == input.InvoiceId)
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId)
                .WhereIf(input.Status >= 0,
                    x => x.Status == input.Status)
                .WhereIf(input.IssueDateStart.HasValue,
                    x => x.IssueDate >= input.IssueDateStart)
                .WhereIf(input.IssueDateEnd.HasValue,
                    x => x.IssueDate <= input.IssueDateEnd)
                .WhereIf(input.OfficeId.HasValue,
                    x => x.OfficeId == input.OfficeId)
                .WhereIf(input.BatchId.HasValue,
                    x => x.BatchId == input.BatchId)
                .WhereIf(input.UploadBatchId.HasValue,
                    x => x.UploadBatchId == input.UploadBatchId)
                .WhereIf(!input.TicketNumber.IsNullOrEmpty(),
                    x => x.InvoiceLines.Any(l => l.Ticket.TicketNumber == input.TicketNumber));

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new InvoiceDto
                {
                    Id = x.Id,
                    Status = x.Status,
                    CustomerName = x.Customer.Name,
                    CustomerHasMaterialCompany = x.Customer.MaterialCompanyTenantId != null,
                    JobNumbers = x.InvoiceLines.Select(l => l.JobNumber).Where(j => !string.IsNullOrEmpty(j)).Distinct().ToList(),
                    JobNumberSort = x.InvoiceLines.Select(l => l.JobNumber).Where(j => !string.IsNullOrEmpty(j)).FirstOrDefault(),
                    IssueDate = x.IssueDate,
                    TotalAmount = x.TotalAmount,
                    QuickbooksExportDateTime = x.QuickbooksExportDateTime
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<InvoiceDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<InvoiceEditDto> GetInvoiceForEdit(NullableIdDto input)
        {
            InvoiceEditDto invoiceEditDto;

            if (input.Id.HasValue)
            {
                invoiceEditDto = await _invoiceRepository.GetAll()
                    .Select(invoice => new InvoiceEditDto
                    {
                        Id = invoice.Id,
                        BalanceDue = invoice.TotalAmount,
                        Subtotal = invoice.InvoiceLines.Sum(x => x.Subtotal),
                        TaxAmount = invoice.InvoiceLines.Sum(x => x.Tax),
                        TaxRate = invoice.TaxRate,
                        BillingAddress = invoice.BillingAddress,
                        EmailAddress = invoice.EmailAddress,
                        CustomerId = invoice.CustomerId,
                        CustomerName = invoice.Customer.Name,
                        CustomerInvoicingMethod = invoice.Customer.InvoicingMethod,
                        DueDate = invoice.DueDate,
                        IssueDate = invoice.IssueDate,
                        Message = invoice.Message,
                        Status = invoice.Status,
                        UploadBatchId = invoice.UploadBatchId,
                        BatchId = invoice.BatchId,
                        Terms = invoice.Terms,
                        JobNumber = invoice.JobNumber,
                        PoNumber = invoice.PoNumber,
                        ShowFuelSurchargeOnInvoice = invoice.ShowFuelSurchargeOnInvoice,
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                invoiceEditDto = new InvoiceEditDto
                {
                    IssueDate = await GetToday(),
                    Message = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.DefaultMessageOnInvoice),
                    ShowFuelSurchargeOnInvoice = await SettingManager.ShowFuelSurchargeOnInvoice()
                };
            }

            var fuelServiceId = await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice);
            var fuelService = fuelServiceId > 0 ? await _serviceRepository.GetAll()
                .Where(x => x.Id == fuelServiceId)
                .Select(x => new { x.Id, x.Service1, x.IsTaxable })
                .FirstOrDefaultAsync() : null;

            invoiceEditDto.FuelServiceId = fuelService?.Id;
            invoiceEditDto.FuelServiceName = fuelService?.Service1;
            invoiceEditDto.FuelServiceIsTaxable = fuelService?.IsTaxable ?? false;


            return invoiceEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<PagedResultDto<InvoiceLineEditDto>> GetInvoiceLines(GetInvoiceLinesInput input)
        {
            var items = await _invoiceLineRepository.GetAll()
                .Where(x => x.InvoiceId == input.InvoiceId)
                .Select(x => new InvoiceLineEditDto
                {
                    Id = x.Id,
                    LineNumber = x.LineNumber,
                    CarrierId = x.CarrierId,
                    CarrierName = x.Carrier.Name,
                    DeliveryDateTime = x.DeliveryDateTime,
                    Description = x.Description,
                    ItemId = x.ItemId,
                    ItemName = x.Item.Service1,
                    Quantity = x.Quantity,
                    MaterialRate = x.MaterialRate,
                    FreightRate = x.FreightRate,
                    Subtotal = x.Subtotal,
                    ExtendedAmount = x.ExtendedAmount,
                    FreightExtendedAmount = x.FreightExtendedAmount,
                    LeaseHaulerName = x.Ticket.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                    MaterialExtendedAmount = x.MaterialExtendedAmount,
                    FuelSurcharge = x.FuelSurcharge,
                    Tax = x.Tax,
                    IsTaxable = x.Item.IsTaxable,
                    TicketId = x.TicketId,
                    TicketNumber = x.Ticket.TicketNumber,
                    JobNumber = x.JobNumber,
                    PoNumber = x.Ticket.OrderLine.Order.PONumber,
                    TruckCode = x.TruckCode,
                    ChildInvoiceLineKind = x.ChildInvoiceLineKind,
                    ParentInvoiceLineId = x.ParentInvoiceLineId
                })
                .OrderBy(x => x.LineNumber)
                .ToListAsync();

            return new PagedResultDto<InvoiceLineEditDto>(items.Count, items);
        }

        private IQueryable<Ticket> GetCustomerTicketsQuery(GetCustomerTicketsInput input)
        {
            var query = _ticketRepository.GetAll()
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId)
                .WhereIf(input.IsBilled.HasValue,
                    x => x.IsBilled == input.IsBilled)
                .WhereIf(input.IsVerified.HasValue,
                    x => x.IsVerified == input.IsVerified)
                .WhereIf(input.HasInvoiceLineId == true,
                    x => x.InvoiceLine != null)
                .WhereIf(input.HasInvoiceLineId == false,
                    x => x.InvoiceLine == null)
                .WhereIf(input.ExcludeTicketIds?.Any() == true,
                    x => !input.ExcludeTicketIds.Contains(x.Id))
                .WhereIf(input.TicketIds != null,
                    x => input.TicketIds.Contains(x.Id));

            return query;
        }

        public async Task<bool> GetCustomerHasTickets(GetCustomerTicketsInput input)
        {
            return await GetCustomerTicketsQuery(input).AnyAsync();
        }

        public async Task<GetCustomerTicketsResult> GetCustomerTickets(GetCustomerTicketsInput input)
        {
            input.Normalize();

            var query = GetCustomerTicketsQuery(input);

            //var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new CustomerTicketDto
                {
                    Id = x.Id,
                    CarrierId = x.CarrierId,
                    CarrierName = x.Carrier.Name,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.Name,
                    DeliverToNamePlain = x.DeliverTo.Name + x.DeliverTo.StreetAddress + x.DeliverTo.City + x.DeliverTo.State, //for sorting
                    DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = x.DeliverTo.Name,
                        StreetAddress = x.DeliverTo.StreetAddress,
                        City = x.DeliverTo.City,
                        State = x.DeliverTo.State
                    },
                    LoadAtNamePlain = x.LoadAt.Name + x.LoadAt.StreetAddress + x.LoadAt.City + x.LoadAt.State, //for sorting
                    LoadAt = x.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = x.LoadAt.Name,
                        StreetAddress = x.LoadAt.StreetAddress,
                        City = x.LoadAt.City,
                        State = x.LoadAt.State
                    },
                    ServiceName = x.Service.Service1,
                    ServiceId = x.ServiceId,
                    IsTaxable = x.Service.IsTaxable,
                    TicketDateTime = x.TicketDateTime,
                    TicketNumber = x.TicketNumber,
                    TruckCode = x.TruckCode,
                    Quantity = x.Quantity,
                    Designation = x.OrderLine.Designation,
                    MaterialRate = x.OrderLine.MaterialPricePerUnit,
                    FreightRate = x.OrderLine.FreightPricePerUnit,
                    MaterialUomName = x.OrderLine.MaterialUom.Name,
                    FreightUomName = x.OrderLine.FreightUom.Name,
                    MaterialUomId = x.OrderLine.MaterialUomId,
                    FreightUomId = x.OrderLine.FreightUomId,
                    TicketUomId = x.UnitOfMeasureId,
                    JobNumber = x.OrderLine.JobNumber,
                    PoNumber = x.OrderLine.Order.PONumber,
                    SalesTaxRate = x.OrderLine.Order.SalesTaxRate,
                    IsOrderLineFreightTotalOverridden = x.OrderLine.IsFreightPriceOverridden,
                    IsOrderLineMaterialTotalOverridden = x.OrderLine.IsMaterialPriceOverridden,
                    OrderLineFreightTotal = x.OrderLine.FreightPrice,
                    OrderLineMaterialTotal = x.OrderLine.MaterialPrice,
                    FuelSurcharge = x.FuelSurcharge,
                    LeaseHaulerName = x.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                    InvoiceLineId = x.InvoiceLine.Id,
                    InvoicingMethod = x.Customer.InvoicingMethod,
                    //InvoiceLine = new InvoiceLineEditDto
                    //{
                    //    LineNumber = 0,
                    //    TicketId = x.Id,
                    //    DeliveryDateTime = x.TicketDateTime,
                    //    CarrierId = x.CarrierId,
                    //    CarrierName = x.Carrier.Name,
                    //    TruckCode = x.TruckCode,
                    //    MaterialExtendedAmount = x.MaterialQuantity * x.OrderLine.MaterialPricePerUnit ?? 0,
                    //    FreightExtendedAmount = x.Freigh
                    //}
                })
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

            items.ForEach(x =>
            {
                OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, x, x.SalesTaxRate ?? 0);
            });

            return new GetCustomerTicketsResult(
                items.Count,
                items);
        }

        public async Task<PagedResultDto<SelectListDto>> GetActiveCustomersSelectList(GetSelectListInput input)
        {
            var query = _customerRepository.GetAll()
                .Where(x => x.IsActive)
                .Select(x => new SelectListDto<CustomerSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new CustomerSelectListInfoDto
                    {
                        CustomerId = x.Id,
                        InvoiceEmail = x.InvoiceEmail,
                        BillingAddress1 = x.BillingAddress1,
                        BillingAddress2 = x.BillingAddress2,
                        BillingCity = x.BillingCity,
                        BillingState = x.BillingState,
                        BillingZipCode = x.BillingZipCode,
                        Terms = x.Terms,
                        InvoicingMethod = x.InvoicingMethod
                    }
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<int> EditInvoice(InvoiceEditDto model)
        {
            short lineNumber;
            var invoice = model.Id > 0
                ? await _invoiceRepository.GetAll()
                    .Include(x => x.InvoiceLines)
                    .Where(x => x.Id == model.Id.Value)
                    .FirstAsync()
                : new Invoice
                {
                    TenantId = AbpSession.TenantId ?? 0,
                    OfficeId = OfficeId,
                };

            if (invoice.Status != model.Status)
            {
                switch (invoice.Status)
                {
                    case InvoiceStatus.Draft:
                    case InvoiceStatus.Printed:
                        if (model.Status.IsIn(InvoiceStatus.ReadyForExport, InvoiceStatus.Sent))
                        {
                            invoice.Status = model.Status;
                        }
                        break;
                    case InvoiceStatus.ReadyForExport:
                        if (model.Status.IsIn(InvoiceStatus.Sent))
                        {
                            invoice.Status = model.Status;
                        }
                        break;
                    case InvoiceStatus.Sent:
                    case InvoiceStatus.Viewed:
                        break;
                }
            }

            if (invoice.CustomerId != model.CustomerId)
            {
                if (invoice.InvoiceLines.Any())
                {
                    throw new UserFriendlyException("You can't change the customer after the tickets for the customers were selected");
                }
                if (model.CustomerId.HasValue)
                {
                    invoice.CustomerId = model.CustomerId.Value;
                }
            }

            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

            invoice.EmailAddress = model.EmailAddress;
            invoice.BillingAddress = model.BillingAddress;
            invoice.Terms = model.Terms;
            invoice.IssueDate = model.IssueDate;
            invoice.DueDate = model.DueDate;
            invoice.Message = model.Message;
            invoice.TaxRate = model.TaxRate;
            invoice.JobNumber = model.JobNumber;
            invoice.PoNumber = model.PoNumber;
            invoice.ShowFuelSurchargeOnInvoice = model.ShowFuelSurchargeOnInvoice;

            var newInvoiceLineEntities = new List<InvoiceLine>();
            var removedTicketIds = new List<int>();
            if (model.InvoiceLines != null)
            {
                foreach (var invoiceLine in model.InvoiceLines)
                {
                    OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, invoiceLine, invoice.TaxRate);
                }
                invoice.TotalAmount = model.InvoiceLines.Sum(x => x.ExtendedAmount);
                invoice.Tax = model.InvoiceLines.Sum(x => x.Tax);

                var ticketIds = model.InvoiceLines.Where(x => x.TicketId.HasValue).Select(x => x.TicketId.Value).ToList();
                var takenTicketIds = await _ticketRepository.GetAll()
                    .Where(x => ticketIds.Contains(x.Id) && x.InvoiceLine != null)
                    .Select(x => x.Id).ToListAsync();

                model.InvoiceLines = model.InvoiceLines
                        .OrderByDescending(x => x.ChildInvoiceLineKind != ChildInvoiceLineKind.BottomFuelSurchargeLine)
                        .ThenBy(x => x.DeliveryDateTime)
                        .ThenBy(x => x.TruckCode)
                        .ThenBy(x => x.TicketNumber)
                        .ToList();

                lineNumber = 1;
                foreach (var modelInvoiceLine in model.InvoiceLines)
                {
                    var invoiceLine = modelInvoiceLine.Id == 0 ? null : invoice.InvoiceLines.FirstOrDefault(x => x.Id == modelInvoiceLine.Id);
                    if (invoiceLine == null)
                    {
                        invoiceLine = new InvoiceLine
                        {
                            Invoice = invoice,
                            TenantId = AbpSession.TenantId ?? 0
                        };
                        //invoice.InvoiceLines.Add(invoiceLine);
                        _invoiceLineRepository.Insert(invoiceLine);
                        newInvoiceLineEntities.Add(invoiceLine);
                    }

                    invoiceLine.LineNumber = lineNumber++;
                    if (invoiceLine.TicketId != modelInvoiceLine.TicketId)
                    {
                        if (modelInvoiceLine.TicketId.HasValue)
                        {
                            if (takenTicketIds.Contains(modelInvoiceLine.TicketId.Value))
                            {
                                continue;
                            }
                            takenTicketIds.Add(modelInvoiceLine.TicketId.Value);
                        }
                        invoiceLine.TicketId = modelInvoiceLine.TicketId;
                    }
                    invoiceLine.CarrierId = modelInvoiceLine.CarrierId;
                    invoiceLine.DeliveryDateTime = modelInvoiceLine.DeliveryDateTime;
                    invoiceLine.TruckCode = modelInvoiceLine.TruckCode;
                    invoiceLine.ItemId = modelInvoiceLine.ItemId;
                    invoiceLine.JobNumber = modelInvoiceLine.JobNumber;
                    invoiceLine.Description = modelInvoiceLine.Description;
                    invoiceLine.Quantity = modelInvoiceLine.Quantity;
                    invoiceLine.MaterialRate = modelInvoiceLine.MaterialRate;
                    invoiceLine.FreightRate = modelInvoiceLine.FreightRate;
                    invoiceLine.MaterialExtendedAmount = modelInvoiceLine.MaterialExtendedAmount;
                    invoiceLine.FreightExtendedAmount = modelInvoiceLine.FreightExtendedAmount;
                    invoiceLine.FuelSurcharge = modelInvoiceLine.FuelSurcharge;
                    invoiceLine.Tax = modelInvoiceLine.Tax;
                    invoiceLine.Subtotal = modelInvoiceLine.Subtotal;
                    invoiceLine.ExtendedAmount = modelInvoiceLine.ExtendedAmount;
                    invoiceLine.ChildInvoiceLineKind = modelInvoiceLine.ChildInvoiceLineKind;
                    invoiceLine.ParentInvoiceLineId = modelInvoiceLine.ParentInvoiceLineId;
                }

                foreach (var invoiceLine in invoice.InvoiceLines)
                {
                    if (invoiceLine.Id > 0 && !model.InvoiceLines.Any(x => x.Id == invoiceLine.Id))
                    {
                        //invoice.InvoiceLines.Remove(invoiceLine);
                        _invoiceLineRepository.Delete(invoiceLine);
                        if (invoiceLine.TicketId.HasValue)
                        {
                            removedTicketIds.Add(invoiceLine.TicketId.Value);
                        }
                    }
                }

                if (removedTicketIds.Any())
                {
                    var tickets = await _ticketRepository.GetAll()
                        .Where(x => removedTicketIds.Contains(x.Id))
                        .ToListAsync();
                    tickets.ForEach(x => x.IsBilled = false);
                }
            }

            var ticketIdsToBill = invoice.InvoiceLines.Where(x => x.TicketId.HasValue).Select(x => x.TicketId.Value).Except(removedTicketIds).ToList();
            var ticketsToBill = await _ticketRepository.GetAll().Where(x => ticketIdsToBill.Contains(x.Id)).ToListAsync();
            ticketsToBill.ForEach(t => t.IsBilled = true);

            if (invoice.Id == 0)
            {
                _invoiceRepository.Insert(invoice);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            if (model.InvoiceLines != null)
            {
                foreach (var childInvoiceLineModel in model.InvoiceLines.Where(x => x.ParentInvoiceLineGuid.HasValue))
                {
                    var parentInvoiceLineModel = model.InvoiceLines.FirstOrDefault(x => x.Guid == childInvoiceLineModel.ParentInvoiceLineGuid);
                    var parentEntity = newInvoiceLineEntities.FirstOrDefault(x => x.LineNumber == parentInvoiceLineModel?.LineNumber);
                    var childEntity = newInvoiceLineEntities.FirstOrDefault(x => x.LineNumber == childInvoiceLineModel.LineNumber);
                    if (parentEntity == null || childEntity == null)
                    {
                        continue;
                    }
                    childEntity.ParentInvoiceLineId = parentEntity.Id;
                }
            }

            var invoiceLinesToReorder = invoice.InvoiceLines.OrderBy(x => x.LineNumber).ToList();
            var regularInvoiceLines = invoiceLinesToReorder.Where(x => x.ChildInvoiceLineKind == null).ToList();
            var bottomLines = invoiceLinesToReorder.Where(x => x.ChildInvoiceLineKind == ChildInvoiceLineKind.BottomFuelSurchargeLine).ToList();
            var perTicketLines = invoiceLinesToReorder.Where(x => x.ChildInvoiceLineKind == ChildInvoiceLineKind.FuelSurchargeLinePerTicket).ToList();

            var reorderedLines = regularInvoiceLines.ToList();
            foreach (var perTicketLine in perTicketLines)
            {
                if (perTicketLine.ParentInvoiceLineId != null)
                {
                    var parentLine = reorderedLines.FirstOrDefault(x => x.Id == perTicketLine.ParentInvoiceLineId);
                    if (parentLine != null)
                    {
                        var parentLineIndex = reorderedLines.IndexOf(parentLine);
                        if (parentLineIndex != -1)
                        {
                            reorderedLines.Insert(parentLineIndex + 1, perTicketLine);
                            continue;
                        }
                    }
                }
                reorderedLines.Add(perTicketLine);
            }
            foreach (var bottomLine in bottomLines)
            {
                reorderedLines.Add(bottomLine);
            }

            lineNumber = 1;
            foreach (var line in reorderedLines)
            {
                line.LineNumber = lineNumber++;
            }

            return invoice.Id;
        }

        //public async Task<InvoiceLineEditDto> GetTicketDetailsByRowTicketId(GetTicketDetailsByTicketNumberInput input)
        //{
        //    var ticket = await _ticketRepository.GetAll()
        //        .Where(x => x.TicketNumber == input.TicketNumber && x.CustomerId == input.CustomerId)
        //        .Select(x => new
        //        {

        //        }).FirstOrDefaultAsync();
        //}

        public async Task<CreateInvoicesForTicketsResult> CreateInvoicesForTickets(CreateInvoicesForTicketsInput input)
        {
            var tickets = await GetCustomerTickets(new GetCustomerTicketsInput
            {
                //IsBilled = false,
                IsVerified = true,
                TicketIds = input.TicketIds,
                HasInvoiceLineId = false
            });

            if (!tickets.Items.Any())
            {
                return new CreateInvoicesForTicketsResult
                {
                    BatchId = null
                };
            }

            var customerIds = tickets.Items.Select(x => x.CustomerId).Distinct().ToList();
            var customers = await _customerRepository.GetAll()
                .Where(x => customerIds.Contains(x.Id))
                .Select(x => new CustomerSelectListInfoDto
                {
                    CustomerId = x.Id,
                    InvoiceEmail = x.InvoiceEmail,
                    BillingAddress1 = x.BillingAddress1,
                    BillingAddress2 = x.BillingAddress2,
                    BillingCity = x.BillingCity,
                    BillingState = x.BillingState,
                    BillingZipCode = x.BillingZipCode,
                    Terms = x.Terms,
                    InvoicingMethod = x.InvoicingMethod
                }).ToListAsync();

            if (!customers.Any())
            {
                return new CreateInvoicesForTicketsResult
                {
                    BatchId = null
                };
            }

            var invoiceBatch = new InvoiceBatch { TenantId = AbpSession.TenantId ?? 0 };
            await _invoiceBatchRepository.InsertAndGetIdAsync(invoiceBatch);

            var today = await GetToday();
            var defaultMessage = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.DefaultMessageOnInvoice);
            var showFuelSurchargeOnInvoice = await SettingManager.ShowFuelSurchargeOnInvoice();
            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

            var fuelServiceId = await SettingManager.GetSettingValueAsync<int>(AppSettings.Fuel.ItemIdToUseForFuelSurchargeOnInvoice);
            var fuelService = fuelServiceId > 0 ? await _serviceRepository.GetAll()
                .Where(x => x.Id == fuelServiceId)
                .Select(x => new { x.Id, x.Service1, x.IsTaxable })
                .FirstOrDefaultAsync() : null;

            foreach (var customer in customers)
            {
                var customerTicketsGroups = tickets.Items.Where(x => x.CustomerId == customer.CustomerId).GroupBy(x => x.SalesTaxRate ?? 0);
                foreach (var customerTickets in customerTicketsGroups)
                {
                    var taxRate = customerTickets.Key;
                    switch (customer.InvoicingMethod)
                    {
                        case InvoicingMethodEnum.AggregateAllTickets:
                            AddInvoiceFromCustomerTickets(customerTickets, customer, taxRate);
                            break;
                        case InvoicingMethodEnum.SeparateTicketsByJobNumber:
                            var ticketsByJobNumber = customerTickets.GroupBy(x => x.JobNumber);
                            foreach (var ticketSubGroup in ticketsByJobNumber)
                            {
                                AddInvoiceFromCustomerTickets(ticketSubGroup, customer, taxRate);
                            }
                            break;
                        case InvoicingMethodEnum.SeparateInvoicePerTicket:
                            foreach (var customerTicket in customerTickets)
                            {
                                AddInvoiceFromCustomerTickets(new[] { customerTicket }, customer, taxRate);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            var ticketsToBill = await _ticketRepository.GetAll().Where(x => input.TicketIds.Contains(x.Id)).ToListAsync();
            ticketsToBill.ForEach(t => t.IsBilled = true);

            return new CreateInvoicesForTicketsResult
            {
                BatchId = invoiceBatch.Id
            };

            void AddInvoiceFromCustomerTickets(IEnumerable<CustomerTicketDto> customerTickets, CustomerSelectListInfoDto customer, decimal taxRate)
            {
                var dueDate = CalculateDueDate(new CalculateDueDateInput
                {
                    IssueDate = today,
                    Terms = customer.Terms
                });
                var invoice = new Invoice
                {
                    TenantId = AbpSession.TenantId ?? 0,
                    BatchId = invoiceBatch.Id,
                    EmailAddress = customer.InvoiceEmail,
                    DueDate = dueDate,
                    IssueDate = today,
                    BillingAddress = customer.FullAddress,
                    CustomerId = customer.CustomerId,
                    OfficeId = OfficeId,
                    Terms = customer.Terms,
                    Status = InvoiceStatus.Draft,
                    TaxRate = taxRate,
                    Message = defaultMessage,
                    ShowFuelSurchargeOnInvoice = showFuelSurchargeOnInvoice
                };
                _invoiceRepository.Insert(invoice);

                var jobNumbers = customerTickets.Select(x => x.JobNumber).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (jobNumbers.Any())
                {
                    invoice.JobNumber = string.Join("; ", jobNumbers).TruncateWithPostfix(EntityStringFieldLengths.Invoice.JobNumber, "…");
                }
                var poNumbers = customerTickets.Select(x => x.PoNumber).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (poNumbers.Any())
                {
                    invoice.PoNumber = string.Join("; ", poNumbers).TruncateWithPostfix(EntityStringFieldLengths.Invoice.PoNumber, "…");
                }

                short lineNumber = 1;
                decimal totalFuelSurcharge = 0;
                foreach (var ticket in customerTickets)
                {
                    var invoiceLine = new InvoiceLine
                    {
                        LineNumber = lineNumber++,
                        TicketId = ticket.Id,
                        DeliveryDateTime = ticket.TicketDateTime,
                        CarrierId = ticket.CarrierId,
                        TruckCode = ticket.TruckCode,
                        Description = ticket.Description,
                        ItemId = ticket.ServiceId,
                        JobNumber = ticket.JobNumber,
                        Quantity = ticket.Quantity,
                        FreightRate = ticket.FreightRate,
                        MaterialRate = ticket.MaterialRate,
                        MaterialExtendedAmount = ticket.MaterialTotal,
                        FreightExtendedAmount = ticket.FreightTotal,
                        FuelSurcharge = ticket.FuelSurcharge,
                        Tax = ticket.Tax,
                        //IsTaxable = ticket.Tax > 0 && ticket.IsTaxable ? true : false,
                        Subtotal = ticket.Subtotal,
                        ExtendedAmount = ticket.Total,
                        Invoice = invoice,
                        TenantId = AbpSession.TenantId ?? 0
                    };
                    _invoiceLineRepository.Insert(invoiceLine);
                    invoice.TotalAmount += invoiceLine.ExtendedAmount;
                    invoice.Tax += invoiceLine.Tax;

                    if (showFuelSurchargeOnInvoice == ShowFuelSurchargeOnInvoiceEnum.SingleLineItemAtTheBottom)
                    {
                        totalFuelSurcharge += ticket.FuelSurcharge;
                    }
                    else if (showFuelSurchargeOnInvoice == ShowFuelSurchargeOnInvoiceEnum.LineItemPerTicket)
                    {
                        if (ticket.FuelSurcharge != 0)
                        {
                            if (fuelService == null)
                            {
                                throw new UserFriendlyException(L("PleaseSelectItemToUseForFuelSurchargeOnInvoiceInSettings"));
                            }
                            var fuelLine = new InvoiceLine
                            {
                                LineNumber = lineNumber++,
                                DeliveryDateTime = ticket.TicketDateTime,
                                Description = fuelService.Service1,
                                ItemId = fuelService.Id,
                                Quantity = 1,
                                FreightRate = ticket.FuelSurcharge,
                                FreightExtendedAmount = ticket.FuelSurcharge,
                                FuelSurcharge = 0,
                                Invoice = invoice,
                                TenantId = AbpSession.TenantId ?? 0,
                                ChildInvoiceLineKind = ChildInvoiceLineKind.FuelSurchargeLinePerTicket,
                                ParentInvoiceLine = invoiceLine,
                            };
                            CalculateInvoiceLineTotals(fuelLine, fuelService.IsTaxable, invoice.TaxRate, taxCalculationType);
                            _invoiceLineRepository.Insert(fuelLine);
                            invoice.TotalAmount += fuelLine.ExtendedAmount;
                            invoice.Tax += fuelLine.Tax;
                        }
                    }
                }

                if (showFuelSurchargeOnInvoice == ShowFuelSurchargeOnInvoiceEnum.SingleLineItemAtTheBottom
                    && totalFuelSurcharge != 0)
                {
                    if (fuelService == null)
                    {
                        throw new UserFriendlyException(L("PleaseSelectItemToUseForFuelSurchargeOnInvoiceInSettings"));
                    }
                    var fuelLine = new InvoiceLine
                    {
                        LineNumber = lineNumber++,
                        DeliveryDateTime = null,
                        Description = fuelService.Service1,
                        ItemId = fuelService.Id,
                        Quantity = 1,
                        FreightRate = totalFuelSurcharge,
                        FreightExtendedAmount = totalFuelSurcharge,
                        FuelSurcharge = 0,
                        Invoice = invoice,
                        TenantId = AbpSession.TenantId ?? 0,
                        ChildInvoiceLineKind = ChildInvoiceLineKind.BottomFuelSurchargeLine,
                    };
                    CalculateInvoiceLineTotals(fuelLine, fuelService.IsTaxable, invoice.TaxRate, taxCalculationType);
                    _invoiceLineRepository.Insert(fuelLine);
                    invoice.TotalAmount += fuelLine.ExtendedAmount;
                    invoice.Tax += fuelLine.Tax;
                }
            }
        }

        public DateTime CalculateDueDate(CalculateDueDateInput input)
        {
            switch (input.Terms)
            {
                case BillingTermsEnum.DueOnReceipt: return input.IssueDate;
                case BillingTermsEnum.DueByTheFirstOfTheMonth: return input.IssueDate.AddMonths(1).AddDays(-(input.IssueDate.Day - 1));
                case BillingTermsEnum.Net10: return input.IssueDate.AddDays(10);
                case BillingTermsEnum.Net15: return input.IssueDate.AddDays(15);
                case BillingTermsEnum.Net30: return input.IssueDate.AddDays(30);
                case BillingTermsEnum.Net60: return input.IssueDate.AddDays(60);
                case BillingTermsEnum.Net5: return input.IssueDate.AddDays(5);
                case BillingTermsEnum.Net14: return input.IssueDate.AddDays(14);
                default: return input.IssueDate;
            }
        }

        private void CalculateInvoiceLineTotals(InvoiceLine invoiceLine, bool serviceIsTaxable, decimal taxRate, TaxCalculationType taxCalculationType)
        {
            var lineDto = new OrderLineTaxTotalDetailsDto
            {
                FreightPrice = invoiceLine.FreightRate ?? 0,
                MaterialPrice = invoiceLine.MaterialRate ?? 0,
                IsTaxable = serviceIsTaxable,
                Subtotal = invoiceLine.Subtotal,
                Tax = invoiceLine.Tax,
                TotalAmount = invoiceLine.ExtendedAmount
            };
            OrderTaxCalculator.CalculateSingleOrderLineTotals(taxCalculationType, lineDto, taxRate);
            invoiceLine.Subtotal = lineDto.Subtotal;
            invoiceLine.ExtendedAmount = lineDto.TotalAmount;
            invoiceLine.Tax = lineDto.Tax;
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task DeleteInvoice(EntityDto input)
        {
            var invoice = await _invoiceRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.Status
                }).FirstOrDefaultAsync();

            //if (invoice != null && invoice.Status != InvoiceStatus.Draft)
            //{
            //    throw new UserFriendlyException(L("InvoiceDeleteErrorNotDraft"));
            //}

            var tickets = await _ticketRepository.GetAll()
                .Where(x => x.InvoiceLine.InvoiceId == input.Id && x.IsBilled)
                .ToListAsync();

            foreach (var ticket in tickets)
            {
                ticket.IsBilled = false;
            }

            await _invoiceLineRepository.DeleteAsync(x => x.InvoiceId == input.Id);
            await _invoiceRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task UndoInvoiceExport(EntityDto input)
        {
            var invoice = await _invoiceRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .FirstAsync();

            invoice.UploadBatchId = null;
            invoice.QuickbooksExportDateTime = null;
            invoice.QuickbooksInvoiceId = null;
            invoice.Status = InvoiceStatus.ReadyForExport;
        }


        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task<Document> GetInvoicePrintOut(GetInvoicePrintOutInput input)
        {
            var invoice = await _invoiceRepository.GetAsync(input.InvoiceId);
            if (invoice.Status.IsIn(InvoiceStatus.Draft))
            {
                invoice.Status = InvoiceStatus.Printed;
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var data = await GetInvoicePrintOutData(input);

            var invoiceTemplate = (InvoiceTemplateEnum)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.InvoiceTemplate);

            Document report;

            switch (invoiceTemplate)
            {
                case InvoiceTemplateEnum.Invoice1:
                    report = await _invoicePrintOutGenerator1.GenerateReport(data);
                    break;

                case InvoiceTemplateEnum.Invoice2:
                    report = await _invoicePrintOutGenerator2.GenerateReport(data);
                    break;

                case InvoiceTemplateEnum.Invoice3:
                    report = await _invoicePrintOutGenerator3.GenerateReport(data);
                    break;

                case InvoiceTemplateEnum.Invoice4:
                    report = await _invoicePrintOutGenerator4.GenerateReport(data);
                    break;

                case InvoiceTemplateEnum.Invoice5:
                    report = await _invoicePrintOutGenerator5.GenerateReport(data);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return report;
        }

        private async Task<List<InvoicePrintOutDto>> GetInvoicePrintOutData(GetInvoicePrintOutInput input)
        {
            var item = await _invoiceRepository.GetAll()
                .Where(x => input.InvoiceId == x.Id)
                .Select(x => new InvoicePrintOutDto
                {
                    InvoiceId = x.Id,
                    BillingAddress = x.BillingAddress,
                    CustomerName = x.Customer.Name,
                    JobNumber = x.JobNumber,
                    PoNumber = x.PoNumber,
                    DueDate = x.DueDate,
                    IssueDate = x.IssueDate,
                    Message = x.Message,
                    Tax = x.Tax,
                    TotalAmount = x.TotalAmount,
                    InvoiceLines = x.InvoiceLines.Select(l => new InvoicePrintOutLineItemDto
                    {
                        DeliveryDateTime = l.DeliveryDateTime,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        FreightRate = l.FreightRate,
                        MaterialRate = l.MaterialRate,
                        ItemName = l.Item.Service1,
                        JobNumber = l.JobNumber,
                        Subtotal = l.Subtotal,
                        ExtendedAmount = l.ExtendedAmount,
                        FreightExtendedAmount = l.FreightExtendedAmount,
                        MaterialExtendedAmount = l.MaterialExtendedAmount,
                        Tax = l.Tax,
                        LeaseHaulerName = l.Ticket.Truck.LeaseHaulerTruck.LeaseHauler.Name,
                        LineNumber = l.LineNumber,
                        TicketNumber = l.Ticket.TicketNumber,
                        TruckCode = l.TruckCode,
                        ChildInvoiceLineKind = l.ChildInvoiceLineKind
                    }).ToList()
                }).FirstOrDefaultAsync();

            var timezone = await GetTimezone();
            item.InvoiceLines = item.InvoiceLines.OrderBy(x => x.LineNumber ?? 0).ToList();
            item.InvoiceLines.ForEach(x => x.DeliveryDateTime = x.DeliveryDateTime?.ConvertTimeZoneTo(timezone));

            item.InvoiceLines.RemoveAll(l =>
                    l.ChildInvoiceLineKind.IsIn(ChildInvoiceLineKind.BottomFuelSurchargeLine, ChildInvoiceLineKind.FuelSurchargeLinePerTicket)
                    && l.ExtendedAmount == 0);

            item.LegalName = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingLegalName);
            item.LegalAddress = await SettingManager.GetSettingValueAsync(AppSettings.TenantManagement.BillingAddress);
            item.RemitToInformation = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.RemitToInformation);

            item.LogoPath = await _binaryObjectManager.GetLogoAsBase64StringAsync(await GetCurrentTenantAsync());
            item.TimeZone = await GetTimezone();
            item.CurrencyCulture = await SettingManager.GetCurrencyCultureAsync();

            item.CompanyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
            item.TermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.TermsAndConditions);
            item.TermsAndConditions = item.TermsAndConditions
                .Replace("{CompanyName}", item.CompanyName)
                .Replace("{CompanyNameUpperCase}", item.CompanyName.ToUpper());

            item.DebugLayout = input.DebugLayout;
            item.DebugInput = input;

            return new List<InvoicePrintOutDto> { item };
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<EmailInvoicePrintOutDto> GetEmailInvoicePrintOutModel(EntityDto input)
        {
            var user = await UserManager.Users
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname
                })
                .FirstAsync();

            var invoice = await _invoiceRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.EmailAddress
                })
                .FirstAsync();

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = ReplaceEmailSubjectTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailSubjectTemplate), companyName);

            var body = ReplaceEmailBodyTemplateTokens(await SettingManager.GetSettingValueAsync(AppSettings.Invoice.EmailBodyTemplate), user.FirstName, user.LastName);

            var ccMeOnInvoices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.DispatchingAndMessaging.CCMeOnInvoices);
            return new EmailInvoicePrintOutDto
            {
                InvoiceId = input.Id,
                From = user.Email,
                To = invoice.EmailAddress,
                CC = ccMeOnInvoices ? user.Email : null,
                Subject = subject,
                Body = body
            };
        }

        public static string ReplaceEmailSubjectTemplateTokens(string subjectTemplate, string companyName)
        {
            return subjectTemplate
                .Replace("{CompanyName}", companyName);
        }

        public static string ReplaceEmailBodyTemplateTokens(string bodyTemplate, string userFirstName, string userLastName)
        {
            return bodyTemplate
                .Replace("{User.FirstName}", userFirstName)
                .Replace("{User.LastName}", userLastName);
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task EmailInvoicePrintOut(EmailInvoicePrintOutDto input)
        {
            var report = await GetInvoicePrintOut(new GetInvoicePrintOutInput
            {
                InvoiceId = input.InvoiceId
            });
            var message = new MailMessage
            {
                From = new MailAddress(input.From),
                Subject = input.Subject,
                Body = input.Body,
                IsBodyHtml = false
            };
            foreach (var to in EmailHelper.SplitEmailAddresses(input.To))
            {
                message.To.Add(to);
            }
            foreach (var cc in EmailHelper.SplitEmailAddresses(input.CC))
            {
                message.CC.Add(cc);
            }

            var filename = "Invoice";

            filename = Utilities.RemoveInvalidFileNameChars(filename);
            filename += ".pdf";

            using (var stream = new MemoryStream())
            {
                report.SaveToMemoryStream(stream);
                message.Attachments.Add(new Attachment(stream, filename));

                var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);
                await _invoiceEmailRepository.InsertAsync(new InvoiceEmail
                {
                    EmailId = trackableEmailId,
                    InvoiceId = input.InvoiceId
                });

                var invoice = await _invoiceRepository.GetAsync(input.InvoiceId);
                if (invoice.Status.IsIn(InvoiceStatus.Draft, InvoiceStatus.ReadyForExport, InvoiceStatus.Printed))
                {
                    invoice.Status = InvoiceStatus.Sent;
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Invoices)]
        public async Task SendInvoiceTicketsToCustomerTenant(EntityDto input)
        {
            await _crossTenantOrderSender.SendInvoiceTicketsToCustomerTenant(input);
        }
    }
}
