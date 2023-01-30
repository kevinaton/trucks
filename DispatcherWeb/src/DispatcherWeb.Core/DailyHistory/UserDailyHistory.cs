using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.DailyHistory
{
    public class UserDailyHistory : Entity
    {
        public long UserId { get; set; }
        public User User { get; set; }

        public int? TenantId { get; set; }

        public DateTime Date { get; set; }

        public int NumberOfTransactions { get; set; }

    }
}
