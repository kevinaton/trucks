using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Invoices
{
    public class InvoiceEventHandler : IAsyncEventHandler<EntityDeletingEventData<InvoiceLine>>, ITransientDependency
    {
        private readonly IRepository<InvoiceLine> _invoiceLineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public InvoiceEventHandler(
            IRepository<InvoiceLine> invoiceLineRepository,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _invoiceLineRepository = invoiceLineRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task HandleEventAsync(EntityDeletingEventData<InvoiceLine> eventData)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                if (eventData.Entity.TicketId != null)
                {
                    eventData.Entity.TicketId = null;
                }
                await uow.CompleteAsync();
            }
        }
    }
}
