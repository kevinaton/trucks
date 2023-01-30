using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;

namespace DispatcherWeb.DailyHistory
{
    public class TransactionDailyHistory : Entity
    {
        public DateTime Date { get; set; }

        [StringLength(256)]
        [Required]
        public string ServiceName { get; set; }

        [StringLength(256)]
        [Required]
        public string MethodName { get; set; }

        public int NumberOfTransactions { get; set; }
        public int AverageExecutionDuration { get; set; }

    }
}
