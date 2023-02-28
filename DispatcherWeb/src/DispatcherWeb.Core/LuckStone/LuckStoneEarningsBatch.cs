using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.LuckStone
{
    [Table("LuckStoneEarningsBatch")]
    public class LuckStoneEarningsBatch : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [StringLength(200)]
        public string FilePath { get; set; }

        public List<LuckStoneEarnings> LuckStoneEarnings { get; set; }
    }
}
