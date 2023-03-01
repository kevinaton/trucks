namespace DispatcherWeb.QuickbooksDesktop.Dto
{
    public class ExportInvoicesToIIFResult
    {
        public byte[] FileBytes { get; set; }
        public int UploadBatchId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
