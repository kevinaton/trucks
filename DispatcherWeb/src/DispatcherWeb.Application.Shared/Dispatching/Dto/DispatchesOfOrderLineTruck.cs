using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Dispatching.Dto.DispatchSender;
using System.Collections.Generic;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchesOfOrderLineTruck
    {
        public OrderLineTruckDto OrderLineTruck { get; set; }
        public IEnumerable<FullAuditedEntity> Dispatches { get; set; }
    }
}
