using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DispatcherWeb.PayStatements.Dto
{
    public class DriverPayStatementReport
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
    }
}
