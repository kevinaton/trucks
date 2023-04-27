using DispatcherWeb.Dto;

namespace DispatcherWeb.DataExporting.Csv
{
    public interface ICsvExporter
    {
        FileDto StoreTempFile(FileBytesDto fileBytes);
    }
}
