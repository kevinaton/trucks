using DispatcherWeb.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineNoteInput
    {
        public int? OrderLineId { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLine.Note)]
        public string Note { get; set; }
    }
}
