using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Receipts.Dto;
using DispatcherWeb.Services;
using DispatcherWeb.Locations;
using DispatcherWeb.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DispatcherWeb.Common.Dto;

namespace DispatcherWeb.Receipts
{
    [AbpAuthorize]
    public class ReceiptAppService : DispatcherWebAppServiceBase, IReceiptAppService
    {
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<ReceiptLine> _receiptLineRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly OrderTaxCalculator _orderTaxCalculator;

        public ReceiptAppService(
            IRepository<Receipt> receiptRepository,
            IRepository<ReceiptLine> receiptLineRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Office> officeRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Location> locationRepository,
            IRepository<Service> serviceRepository,
            ISingleOfficeAppService singleOfficeService,
            OrderTaxCalculator orderTaxCalculator
            )
        {
            _receiptRepository = receiptRepository;
            _receiptLineRepository = receiptLineRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _officeRepository = officeRepository;
            _ticketRepository = ticketRepository;
            _locationRepository = locationRepository;
            _serviceRepository = serviceRepository;
            _singleOfficeService = singleOfficeService;
            _orderTaxCalculator = orderTaxCalculator;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<ReceiptEditDto> GetReceiptForEdit(GetReceiptForEditInput input)
        {
            ReceiptEditDto receiptEditDto;

            if (input.Id.HasValue)
            {
                receiptEditDto = await _receiptRepository.GetAll()
                    .Select(receipt => new ReceiptEditDto
                    {
                        Id = receipt.Id,
                        OrderId = receipt.OrderId,
                        OfficeId = receipt.OfficeId,
                        OfficeName = receipt.Office.Name,
                        MaterialTotal = receipt.MaterialTotal,
                        FreightTotal = receipt.FreightTotal,
                        PoNumber = receipt.PoNumber,
                        CustomerId = receipt.CustomerId,
                        CustomerName = receipt.Customer.Name,
                        DeliveryDate = receipt.DeliveryDate,
                        IsFreightTotalOverridden = receipt.IsFreightTotalOverridden,
                        IsMaterialTotalOverridden = receipt.IsMaterialTotalOverridden,
                        QuoteId = receipt.QuoteId,
                        QuoteName = receipt.Quote.Name,
                        ReceiptDate = receipt.ReceiptDate,
                        SalesTax = receipt.SalesTax,
                        SalesTaxRate = receipt.SalesTaxRate,
                        Shift = receipt.Shift,
                        Total = receipt.Total
                    })
                    .FirstAsync(x => x.Id == input.Id);

                var payment = await _orderRepository.GetAll()
                    .Where(x => x.Id == receiptEditDto.OrderId)
                    .SelectMany(x => x.OrderPayments)
                    .Where(x => x.OfficeId == OfficeId)
                    .Select(x => x.Payment)
                    .Where(x => !x.IsCancelledOrRefunded)
                    .Select(x => new
                    {
                        x.AuthorizationDateTime,
                        x.AuthorizationCaptureDateTime
                    }).FirstOrDefaultAsync();

                receiptEditDto.AuthorizationDateTime = payment?.AuthorizationDateTime;
                receiptEditDto.AuthorizationCaptureDateTime = payment?.AuthorizationCaptureDateTime;
            }
            else if (input.OrderId.HasValue)
            {
                var today = await GetToday();

                receiptEditDto = await _orderRepository.GetAll()
                    .Where(x => x.Id == input.OrderId)
                    .Select(order => new ReceiptEditDto
                    {
                        CustomerId = order.CustomerId,
                        CustomerName = order.Customer.Name,
                        DeliveryDate = order.DeliveryDate == null ? today : order.DeliveryDate.Value,
                        //OfficeId = order.LocationId,
                        //OfficeName = order.Office.Name,
                        OrderId = order.Id,
                        PoNumber = order.PONumber,
                        QuoteId = order.QuoteId,
                        QuoteName = order.Quote.Name,
                        Shift = order.Shift,
                        SalesTax = order.SalesTax,
                        SalesTaxRate = order.SalesTaxRate
                        //MaterialTotal
                        //FreightTotal
                        })
                    .FirstAsync();

                receiptEditDto.ReceiptDate = today;
                receiptEditDto.OfficeId = OfficeId;
                receiptEditDto.OfficeName = Session.OfficeName;
            }
            else
            {
                throw new ArgumentNullException(nameof(input.OrderId));
            }

            await _singleOfficeService.FillSingleOffice(receiptEditDto);

            return receiptEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<int> EditReceipt(ReceiptEditDto model)
        {
            var receipt = model.Id > 0 ? await _receiptRepository.GetAsync(model.Id.Value) : new Receipt();

            receipt.CustomerId = model.CustomerId;
            receipt.DeliveryDate = model.DeliveryDate;
            receipt.FreightTotal = model.FreightTotal;
            receipt.MaterialTotal = model.MaterialTotal;
            receipt.OfficeId = model.OfficeId;
            receipt.PoNumber = model.PoNumber;
            receipt.QuoteId = model.QuoteId;
            receipt.ReceiptDate = model.ReceiptDate;
            receipt.SalesTax = model.SalesTax;
            receipt.SalesTaxRate = model.SalesTaxRate;
            receipt.Shift = model.Shift;
            receipt.Total = model.Total;

            if (receipt.Id == 0)
            {
                receipt.OrderId = model.OrderId;
                model.Id = await _receiptRepository.InsertAndGetIdAsync(receipt);

                if (model.ReceiptLines != null)
                {
                    var ticketIds = model.ReceiptLines.SelectMany(x => x.TicketIds ?? new List<int>()).Distinct().ToList();
                    var tickets = await _ticketRepository.GetAll().Where(x => ticketIds.Contains(x.Id) && x.ReceiptLineId == null).ToListAsync();

                    foreach (var receiptLineModel in model.ReceiptLines)
                    {
                        receiptLineModel.ReceiptId = receipt.Id;
                        var receiptLine = await EditReceiptLineInternal(receiptLineModel);

                        if (receiptLineModel.TicketIds?.Any() == true)
                        {
                            tickets.Where(x => receiptLineModel.TicketIds.Contains(x.Id)).ToList()
                                .ForEach(x => x.ReceiptLine = receiptLine);
                        }
                    }

                    var serviceIds = model.ReceiptLines.Select(x => x.ServiceId).Distinct().ToList();
                    var services = await _serviceRepository.GetAll()
                        .Where(x => serviceIds.Contains(x.Id))
                        .Select(x => new
                        {
                            x.Id,
                            x.IsTaxable
                        }).ToListAsync();

                    await _orderTaxCalculator.CalculateTotalsAsync(receipt, model.ReceiptLines.Select(x => new ReceiptLineTaxDetailsDto
                    {
                        IsTaxable = services.FirstOrDefault(s => s.Id == x.ServiceId)?.IsTaxable ?? true,
                        FreightAmount = x.FreightAmount,
                        MaterialAmount = x.MaterialAmount
                    }));
                }
            }
            else
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                await _orderTaxCalculator.CalculateReceiptTotalsAsync(receipt.Id);
            }

            return receipt.Id;
        }

        //*********************//

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<PagedResultDto<ReceiptLineEditDto>> GetReceiptLines(GetReceiptLinesInput input)
        {
            if (input.ReceiptId.HasValue)
            {
                var query = _receiptLineRepository.GetAll();

                var totalCount = await query.CountAsync();

                var items = await query
                    .Where(x => x.ReceiptId == input.ReceiptId)
                    .WhereIf(input.LoadAtId.HasValue || input.ForceDuplicateFilters,
                             x => x.LoadAtId == input.LoadAtId)
                    .WhereIf(input.DeliverToId.HasValue || input.ForceDuplicateFilters,
                             x => x.DeliverToId == input.DeliverToId)
                    .WhereIf(input.ServiceId.HasValue,
                             x => x.ServiceId == input.ServiceId)
                    .WhereIf(input.FreightUomId.HasValue,
                             x => x.FreightUomId == input.FreightUomId)
                    .WhereIf(input.MaterialUomId.HasValue,
                             x => x.MaterialUomId == input.MaterialUomId)
                    .WhereIf(input.Designation.HasValue,
                             x => x.Designation == input.Designation)
                    .Select(x => new ReceiptLineEditDto
                    {
                        Id = x.Id,
                        LoadAtId = x.LoadAtId,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverToId = x.DeliverToId,
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        ServiceName = x.Service.Service1,
                        Designation = x.Designation,
                        FreightAmount = x.FreightAmount,
                        ServiceId = x.ServiceId,
                        FreightRate = x.FreightRate,
                        FreightQuantity = x.FreightQuantity,
                        FreightUomId = x.FreightUomId,
                        IsFreightAmountOverridden = x.IsFreightAmountOverridden,
                        IsMaterialAmountOverridden = x.IsMaterialAmountOverridden,
                        IsFreightRateOverridden = x.IsFreightRateOverridden,
                        IsMaterialRateOverridden = x.IsMaterialRateOverridden,
                        OrderLineId = x.OrderLineId,
                        MaterialUomId = x.MaterialUomId,
                        LineNumber = x.LineNumber,
                        MaterialRate = x.MaterialRate,
                        MaterialAmount = x.MaterialAmount,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightUomName = x.FreightUom.Name,
                        MaterialUomName = x.MaterialUom.Name,
                        JobNumber = x.JobNumber,
                        ReceiptId = x.ReceiptId,
                    })
                    .OrderBy(input.Sorting)
                    //.PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<ReceiptLineEditDto>(
                    totalCount,
                    items);
            }
            else if (input.OrderId.HasValue)
            {
                var allowAddingTickets = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AllowAddingTickets);
                var splitBillingByOffices = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.SplitBillingByOffices);
                var orderLines = await _orderLineRepository.GetAll()
                        .Where(x => x.OrderId == input.OrderId)
                        .Select(x => new
                        {
                            ReceiptLine = new ReceiptLineEditDto
                            {
                                Id = 0,
                                LoadAtId = x.LoadAtId,
                                LoadAt = x.LoadAt == null ? null : new LocationNameDto
                                {
                                    Name = x.LoadAt.Name,
                                    StreetAddress = x.LoadAt.StreetAddress,
                                    City = x.LoadAt.City,
                                    State = x.LoadAt.State
                                },
                                DeliverToId = x.DeliverToId,
                                DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                                {
                                    Name = x.DeliverTo.Name,
                                    StreetAddress = x.DeliverTo.StreetAddress,
                                    City = x.DeliverTo.City,
                                    State = x.DeliverTo.State
                                },
                                ServiceName = x.Service.Service1,
                                Designation = x.Designation,
                                FreightAmount = x.IsFreightPriceOverridden ? x.FreightPrice : 0,
                                ServiceId = x.ServiceId,
                                FreightRate = x.FreightPricePerUnit == null ? 0 : x.FreightPricePerUnit.Value,
                                //FreightQuantity = x.FreightQuantity,
                                FreightUomId = x.FreightUomId,
                                IsFreightAmountOverridden = x.IsFreightPriceOverridden,
                                IsMaterialAmountOverridden = x.IsMaterialPriceOverridden,
                                IsFreightRateOverridden = x.IsFreightPricePerUnitOverridden,
                                IsMaterialRateOverridden = x.IsMaterialPricePerUnitOverridden,
                                OrderLineId = x.Id,
                                MaterialUomId = x.MaterialUomId,
                                LineNumber = x.LineNumber,
                                MaterialRate = x.MaterialPricePerUnit == null ? 0 : x.MaterialPricePerUnit.Value,
                                MaterialAmount = x.IsMaterialPriceOverridden ? x.MaterialPrice : 0,
                                //MaterialQuantity = x.MaterialQuantity,
                                FreightUomName = x.FreightUom.Name,
                                MaterialUomName = x.MaterialUom.Name,
                                JobNumber = x.JobNumber,
                                //ReceiptId = x.ReceiptId
                            },
                            Tickets = x.Tickets
                                .Where(t => !splitBillingByOffices || t.OfficeId == OfficeId)
                                .Select(t => new
                            {
                                t.Id,
                                t.Quantity,
                                t.UnitOfMeasureId,
                                t.ReceiptLineId
                            }).ToList(),
                            HasPreviousReceiptLines = x.ReceiptLines.Any(r => r.Receipt.OfficeId == OfficeId)
                        })
                        .ToListAsync();

                var receiptLines = orderLines
                    .Where(x =>
                    {
                        if (x.ReceiptLine.IsMaterialAmountOverridden && x.ReceiptLine.IsFreightAmountOverridden && x.HasPreviousReceiptLines)
                        {
                            return false;
                        }
                        return true;
                    })
                    .Select(x =>
                        {
                            x.ReceiptLine.TicketIds = new List<int>();
                            if (allowAddingTickets)
                            {
                                if (x.ReceiptLine.IsMaterialAmountOverridden || x.ReceiptLine.IsFreightAmountOverridden)
                                {
                                    //only one ticket is allowed for the overridden values
                                    var ticket = x.Tickets.OrderBy(t => t.Id).FirstOrDefault();
                                    if (ticket != null)
                                    {
                                        var ticketAmount = new TicketQuantityDto(ticket.Quantity, x.ReceiptLine.Designation, x.ReceiptLine.MaterialUomId, x.ReceiptLine.FreightUomId, ticket.UnitOfMeasureId);
                                        //the single allowed ticket hasn't been used up by another receipt line yet
                                        if (ticket.ReceiptLineId == null)
                                        {
                                            x.ReceiptLine.TicketIds.Add(ticket.Id);
                                            if (x.ReceiptLine.IsMaterialAmountOverridden)
                                            {
                                                x.ReceiptLine.MaterialQuantity = ticketAmount.GetMaterialQuantity();
                                            }
                                            if (x.ReceiptLine.IsFreightAmountOverridden)
                                            {
                                                x.ReceiptLine.FreightQuantity = ticketAmount.GetFreightQuantity();
                                            }
                                        }
                                        else
                                        {
                                            if (x.ReceiptLine.IsMaterialAmountOverridden)
                                            {
                                                x.ReceiptLine.MaterialQuantity = 0;
                                                x.ReceiptLine.MaterialAmount = 0;
                                            }
                                            if (x.ReceiptLine.IsFreightAmountOverridden)
                                            {
                                                x.ReceiptLine.FreightQuantity = 0;
                                                x.ReceiptLine.FreightAmount = 0;
                                            }
                                        }
                                    }
                                }
                                if (!x.ReceiptLine.IsMaterialAmountOverridden || !x.ReceiptLine.IsFreightAmountOverridden)
                                {
                                    //all new tickets are allowed for non-overridden values
                                    var tickets = x.Tickets.Where(t => t.ReceiptLineId == null).ToList();
                                    x.ReceiptLine.TicketIds.AddRange(tickets.Select(t => t.Id).Except(x.ReceiptLine.TicketIds));
                                    if (!x.ReceiptLine.IsMaterialAmountOverridden)
                                    {
                                        x.ReceiptLine.MaterialQuantity = tickets.Any()
                                            ? tickets
                                                .Select(t => new TicketQuantityDto(t.Quantity, x.ReceiptLine.Designation, x.ReceiptLine.MaterialUomId, x.ReceiptLine.FreightUomId, t.UnitOfMeasureId))
                                                .Sum(t => t.GetMaterialQuantity())
                                            : (decimal?)null;
                                        x.ReceiptLine.MaterialAmount = (x.ReceiptLine.MaterialQuantity ?? 0) * x.ReceiptLine.MaterialRate;
                                    }
                                    if (!x.ReceiptLine.IsFreightAmountOverridden)
                                    {
                                        x.ReceiptLine.FreightQuantity = tickets.Any()
                                            ? tickets
                                                .Select(t => new TicketQuantityDto(t.Quantity, x.ReceiptLine.Designation, x.ReceiptLine.MaterialUomId, x.ReceiptLine.FreightUomId, t.UnitOfMeasureId))
                                                .Sum(t => t.GetFreightQuantity())
                                            : (decimal?)null;
                                        x.ReceiptLine.FreightAmount = (x.ReceiptLine.FreightQuantity ?? 0) * x.ReceiptLine.FreightRate;
                                    }
                                }
                                return x.ReceiptLine;
                            }
                            else
                            {
                                return x.ReceiptLine;
                            }
                        })
                .ToList();

                return new PagedResultDto<ReceiptLineEditDto>(
                    receiptLines.Count, 
                    receiptLines);
            }
            else
            {
                throw new ArgumentNullException(nameof(input.OrderId));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View)]
        public async Task<ReceiptLineEditDto> GetReceiptLineForEdit(GetReceiptLineForEditInput input)
        {
            ReceiptLineEditDto receiptLineEditDto;

            if (input.Id.HasValue)
            {
                receiptLineEditDto = await _receiptLineRepository.GetAll()
                    .Select(x => new ReceiptLineEditDto
                    {
                        Id = x.Id,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        ServiceName = x.Service.Service1,
                        Designation = x.Designation,
                        FreightAmount = x.FreightAmount,
                        LoadAtId = x.LoadAtId,
                        DeliverToId = x.DeliverToId,
                        ServiceId = x.ServiceId,
                        FreightRate = x.FreightRate,
                        FreightQuantity = x.FreightQuantity,
                        FreightUomId = x.FreightUomId,
                        IsFreightRateOverridden = x.IsFreightRateOverridden,
                        IsMaterialRateOverridden = x.IsMaterialRateOverridden,
                        IsFreightAmountOverridden = x.IsFreightAmountOverridden,
                        IsMaterialAmountOverridden = x.IsMaterialAmountOverridden,
                        OrderLineId = x.Id,
                        MaterialUomId = x.MaterialUomId,
                        LineNumber = x.LineNumber,
                        MaterialRate = x.MaterialRate,
                        MaterialAmount = x.MaterialAmount,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightUomName = x.FreightUom.Name,
                        MaterialUomName = x.MaterialUom.Name,
                        JobNumber = x.JobNumber,
                        ReceiptId = x.ReceiptId
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else if (input.ReceiptId.HasValue)
            {
                var lastReceiptLine = await _receiptLineRepository.GetAll()
                    .Where(x => x.ReceiptId == input.ReceiptId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new
                    {
                        Id = x.Id,
                        LoadAtId = x.LoadAtId,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverToId = x.DeliverToId,
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                    })
                    .FirstOrDefaultAsync();

                var receipt = await _receiptRepository.GetAll()
                    .Where(x => x.Id == input.ReceiptId)
                    .Select(x => new
                    {
                        ReceiptLinesCount = x.ReceiptLines.Count
                    })
                    .FirstOrDefaultAsync();

                receiptLineEditDto = new ReceiptLineEditDto
                {
                    ReceiptId = input.ReceiptId.Value,
                    LoadAtId = lastReceiptLine?.LoadAtId,
                    LoadAt = lastReceiptLine?.LoadAt,
                    DeliverToId = lastReceiptLine?.DeliverToId,
                    DeliverTo = lastReceiptLine?.DeliverTo,
                    LineNumber = receipt.ReceiptLinesCount + 1,
                };
            }
            else
            {
                receiptLineEditDto = new ReceiptLineEditDto();
            }

            return receiptLineEditDto;
        }

        private async Task<ReceiptLine> EditReceiptLineInternal(ReceiptLineEditDto model)
        {
            var isNew = model.Id == 0 || model.Id == null;
            var receiptLine = !isNew ? await _receiptLineRepository.GetAsync(model.Id.Value) : new ReceiptLine();

            if (!isNew)
            {
                await PermissionChecker.AuthorizeAsync(AppPermissions.Pages_Orders_Edit);
            }

            if (isNew)
            {
                receiptLine.ReceiptId = model.ReceiptId;
                receiptLine.OrderLineId = model.OrderLineId;
            }

            receiptLine.Designation = model.Designation;
            receiptLine.FreightAmount = model.FreightAmount;
            receiptLine.LoadAtId = model.LoadAtId;
            receiptLine.DeliverToId = model.DeliverToId;
            receiptLine.ServiceId = model.ServiceId;
            receiptLine.FreightRate = model.FreightRate;
            receiptLine.FreightQuantity = model.FreightQuantity;
            receiptLine.FreightUomId = model.FreightUomId;
            receiptLine.IsFreightAmountOverridden = model.IsFreightAmountOverridden;
            receiptLine.IsMaterialAmountOverridden = model.IsMaterialAmountOverridden;
            receiptLine.IsFreightRateOverridden = model.IsFreightRateOverridden;
            receiptLine.IsMaterialRateOverridden = model.IsMaterialRateOverridden;
            receiptLine.MaterialUomId = model.MaterialUomId;
            receiptLine.LineNumber = model.LineNumber;
            receiptLine.MaterialRate = model.MaterialRate;
            receiptLine.MaterialAmount = model.MaterialAmount;
            receiptLine.MaterialQuantity = model.MaterialQuantity;
            receiptLine.JobNumber = model.JobNumber;

            if (isNew)
            {
                await _receiptLineRepository.InsertAsync(receiptLine);
            }

            return receiptLine;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<EditReceiptLineOutput> EditReceiptLine(ReceiptLineEditDto model)
        {
            var receiptLine = await EditReceiptLineInternal(model);

            await CurrentUnitOfWork.SaveChangesAsync();
            model.Id = receiptLine.Id;

            var taxDetails = await _orderTaxCalculator.CalculateReceiptTotalsAsync(model.ReceiptId);

            return new EditReceiptLineOutput
            {
                ReceiptLineId = receiptLine.Id,
                OrderTaxDetails = new ReceiptTaxDetailsDto(taxDetails)
            };
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
        public async Task<DeleteReceiptLineOutput> DeleteReceiptLines(IdListInput input)
        {
            var receipts = await _receiptRepository.GetAll()
                .Include(x => x.ReceiptLines)
                .Where(x => x.ReceiptLines.Any(r => input.Ids.Contains(r.Id)))
                .ToListAsync();

            var receiptLinesToDelete = receipts.SelectMany(x => x.ReceiptLines).Where(x => input.Ids.Contains(x.Id)).ToList();

            foreach (var receiptLine in receiptLinesToDelete)
            {
                await _receiptLineRepository.DeleteAsync(receiptLine.Id);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

            var serviceIds = receipts.SelectMany(x => x.ReceiptLines).Select(x => x.ServiceId).Distinct().ToList();
            var services = await _serviceRepository.GetAll()
                .Where(x => serviceIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.IsTaxable
                }).ToListAsync();

            foreach (var receipt in receipts)
            {
                OrderTaxCalculator.CalculateTotals(taxCalculationType, receipt, 
                    receipt.ReceiptLines
                        .Except(receiptLinesToDelete)
                        .Select(x => new ReceiptLineTaxDetailsDto
                        {
                            IsTaxable = services.FirstOrDefault(s => s.Id == x.ServiceId)?.IsTaxable ?? true,
                            FreightAmount = x.FreightAmount,
                            MaterialAmount = x.MaterialAmount
                        })
                        .ToList());
            }

            return new DeleteReceiptLineOutput
            {
                OrderTaxDetails = receipts.Any() ? new ReceiptTaxDetailsDto(receipts.FirstOrDefault()) : null
            };
        }

        public async Task<IOrderTaxDetails> CalculateReceiptTotals(ReceiptTaxDetailsDto receiptTaxDetails)
        {
            List<ReceiptLineTaxDetailsDto> receiptLines;

            if (receiptTaxDetails.Id != 0)
            {
                receiptLines = await _receiptLineRepository.GetAll()
                    .Where(x => x.ReceiptId == receiptTaxDetails.Id)
                    .Select(x => new ReceiptLineTaxDetailsDto
                    {
                        FreightAmount = x.FreightAmount,
                        MaterialAmount = x.MaterialAmount,
                        IsTaxable = x.Service.IsTaxable
                    })
                    .ToListAsync();
            }
            else if (receiptTaxDetails.ReceiptLines != null)
            {
                receiptLines = receiptTaxDetails.ReceiptLines;
            }
            else
            {
                receiptLines = new List<ReceiptLineTaxDetailsDto>();
            }

            await _orderTaxCalculator.CalculateTotalsAsync(receiptTaxDetails, receiptLines);

            return receiptTaxDetails;
        }
    }
}
