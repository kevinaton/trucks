using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.Imports.RowReaders
{
    public interface ITruckImportRow : IImportRow
    {
        string Office { get; }
        string TruckNumber { get; }
    }
}
