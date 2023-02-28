﻿using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.DriverApp.BinaryObjects.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.BinaryObjects
{
    public class BinaryObjectAppService : DispatcherWebDriverAppAppServiceBase, IBinaryObjectAppService
    {
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Load> _loadRepository;

        public BinaryObjectAppService(
            IBinaryObjectManager binaryObjectManager,
            IRepository<Ticket> ticketRepository,
            IRepository<Load> loadRepository
            )
        {
            _binaryObjectManager = binaryObjectManager;
            _ticketRepository = ticketRepository;
            _loadRepository = loadRepository;
        }

        public async Task<Guid> Post(BinaryObjectDto input)
        {
            var id = await _binaryObjectManager.UploadDataUriStringAsync(input.Base64String, AbpSession.TenantId, 12000000);

            if (id == null)
            {
                throw new UserFriendlyException("Input was empty or in an unexpected format. Expected format: \"data:image/png;base64,iVBORw0K...\"");
            }

            return id.Value;
        }

        public async Task Delete(Guid id)
        {
            if (await _ticketRepository.GetAll().AnyAsync(x => x.TicketPhotoId == id))
            {
                throw new UserFriendlyException("Binary Object cannot be deleted because it is already associated with a ticket");
            }

            if (await _loadRepository.GetAll().AnyAsync(x => x.SignatureId == id))
            {
                throw new UserFriendlyException("Binary Object cannot be deleted because it is already associated with a load");
            }

            await _binaryObjectManager.DeleteAsync(id);
        }

        public async Task<BinaryObjectDto> Get(Guid id)
        {
            var file = await _binaryObjectManager.GetOrNullAsync(id);
            return file == null ? null : new BinaryObjectDto
            {
                Base64String = Convert.ToBase64String(file.Bytes)
            };
        }
    }
}
