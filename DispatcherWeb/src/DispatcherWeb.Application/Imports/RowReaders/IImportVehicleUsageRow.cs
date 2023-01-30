using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Imports.RowReaders
{
    public interface IImportVehicleUsageRow : IImportRow
    {
        DateTime? ReadingDateTime { get; }
        decimal? OdometerReading { get; }
        decimal? EngineHours { get; }

    }
}
