using System.Collections.Generic;
using System.Linq;
using Abp;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.DataExporting.Csv;
using DispatcherWeb.Dto;
using DispatcherWeb.Storage;

namespace DispatcherWeb.Chat.Exporting
{
    public class ChatMessageListExcelExporter : CsvExporterBase, IChatMessageListExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;

        public ChatMessageListExcelExporter(
            ITempFileCacheManager tempFileCacheManager,
            ITimeZoneConverter timeZoneConverter
            ) : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
        }

        public FileDto ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages)
        {
            var tenancyName = messages.Count > 0 ? messages.First().TargetTenantName : L("Anonymous");
            var userName = messages.Count > 0 ? messages.First().TargetUserName : L("Anonymous");

            return CreateCsvFile(
                $"Chat_{tenancyName}_{userName}.csv",
                () =>
                {
                    AddHeaderAndData(
                        messages,
                        (L("ChatMessage_From"), x => x.Side == ChatSide.Receiver ? (x.TargetTenantName + "/" + x.TargetUserName) : L("You")),
                        (L("ChatMessage_To"), x => x.Side == ChatSide.Receiver ? L("You") : (x.TargetTenantName + "/" + x.TargetUserName)),
                        (L("Message"), x => x.Message),
                        (L("ReadState"), x => x.Side == ChatSide.Receiver ? x.ReadState.ToString() : x.ReceiverReadState.ToString()),
                        (L("CreationTime"), x => _timeZoneConverter.Convert(x.CreationTime, user.TenantId, user.UserId)?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"))
                    );
                });
        }
    }
}
