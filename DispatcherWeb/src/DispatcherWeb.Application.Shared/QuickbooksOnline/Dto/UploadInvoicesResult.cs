using System;
using System.Collections.Generic;
using System.Text;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class UploadInvoicesResult
    {
        public int UploadedInvoicesCount { get; set; }
        public List<string> ErrorList { get; set; } = new List<string>();
    }
}
