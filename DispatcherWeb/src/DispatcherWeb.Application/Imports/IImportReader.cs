using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatcherWeb.Imports.RowReaders;

namespace DispatcherWeb.Imports
{
    public interface IImportReader
    {
        string[] GetCsvHeaders();
        IEnumerable<T> AsEnumerable<T>() where T: IImportRow;
    }
}
