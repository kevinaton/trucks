using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Attributes
{
    public class MileageColumnAttribute : ColumnAttribute
    {
        public MileageColumnAttribute()
        {
            TypeName = "decimal(19, 1)";
        }

    }
}
