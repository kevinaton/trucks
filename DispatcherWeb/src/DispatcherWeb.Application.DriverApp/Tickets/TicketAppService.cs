using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.Tickets.Dto;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.Tickets
{
    [AbpAuthorize]
    public class TicketAppService : DispatcherWebDriverAppAppServiceBase, ITicketAppService
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Load> _loadRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;

        public TicketAppService(
            IRepository<Ticket> ticketRepository,
            IRepository<Load> loadRepository,
            IRepository<Dispatch> dispatchRepository
            )
        {
            _ticketRepository = ticketRepository;
            _loadRepository = loadRepository;
            _dispatchRepository = dispatchRepository;
        }

        public async Task<TicketDto> Post(TicketDto model)
        {
            var ticket = model.Id == 0
                ? new Ticket()
                : _ticketRepository.GetAll().FirstOrDefault(x => x.Id == model.Id);

            if (ticket == null)
            {
                throw new UserFriendlyException($"Ticket with id {model.Id} wasn't found");
            }

            if (!model.DispatchId.HasValue)
            {
                throw new UserFriendlyException("DispatchId is required");
            }

            var dispatchData = await _dispatchRepository.GetAll()
                .Where(x => x.Id == model.DispatchId)
                .Select(x => new
                {
                    x.OrderLineId,
                    OfficeId = x.OrderLine.Order.LocationId,
                    x.OrderLine.LoadAtId,
                    x.OrderLine.DeliverToId,
                    x.TruckId,
                    x.Truck.TruckCode,
                    LeaseHaulerId = (int?)x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    x.OrderLine.Order.CustomerId,
                    x.OrderLine.ServiceId,
                    x.DriverId,
                    x.OrderLine.Designation,
                    x.OrderLine.MaterialUomId,
                    x.OrderLine.FreightUomId,
                    x.TenantId
                }).FirstOrDefaultAsync();

            if (dispatchData == null)
            {
                throw new UserFriendlyException($"Dispatch with id {model.DispatchId} wasn't found");
            }

            ticket.Quantity = model.Quantity;
            ticket.TicketDateTime = model.TicketDateTime;
            ticket.TicketNumber = model.TicketNumber;
            ticket.TicketPhotoId = model.TicketPhotoId;
            ticket.TicketPhotoFilename = model.TicketPhotoFilename;

            if (ticket.Id == 0)
            {
                ticket.OrderLineId = dispatchData.OrderLineId;
                ticket.OfficeId = dispatchData.OfficeId;
                ticket.LoadAtId = dispatchData.LoadAtId;
                ticket.DeliverToId = dispatchData.DeliverToId;
                ticket.TruckId = dispatchData.TruckId;
                ticket.TruckCode = dispatchData.TruckCode;
                ticket.CarrierId = dispatchData.LeaseHaulerId;
                ticket.CustomerId = dispatchData.CustomerId;
                ticket.ServiceId = dispatchData.ServiceId;
                ticket.DriverId = dispatchData.DriverId;
                ticket.UnitOfMeasureId = dispatchData.Designation == DesignationEnum.MaterialOnly || dispatchData.Designation == DesignationEnum.FreightAndMaterial
                                   ? dispatchData.MaterialUomId
                                   : dispatchData.FreightUomId;
                ticket.TenantId = dispatchData.TenantId;
                ticket.LoadId = model.LoadId;

                if (model.LoadId == null)
                {
                    throw new UserFriendlyException("LoadId is required for new Tickets");
                }

                if (!await _loadRepository.GetAll().AnyAsync(x => x.Id == model.LoadId))
                {
                    throw new UserFriendlyException($"Load with id {model.LoadId} wasn't found");
                }

                await _ticketRepository.InsertAndGetIdAsync(ticket);

                model.Id = ticket.Id;

                if (ticket.TicketNumber.IsNullOrEmpty())
                {
                    ticket.TicketNumber = "G-" + ticket.Id;
                    model.TicketNumber = ticket.TicketNumber;
                }
            }

            return model;
        }
    }
}
