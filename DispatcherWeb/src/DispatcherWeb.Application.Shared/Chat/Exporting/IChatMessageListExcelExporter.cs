using System.Collections.Generic;
using Abp;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Chat.Exporting
{
    public interface IChatMessageListExcelExporter
    {
        FileDto ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages);
    }
}
