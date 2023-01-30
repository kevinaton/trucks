using System;
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
        private readonly IAbpSession _abpSession;

        public ChatMessageListExcelExporter(
            ITempFileCacheManager tempFileCacheManager,
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession
            ) : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages)
        {
            var tenancyName = messages.Count > 0 ? messages.First().TargetTenantName : L("Anonymous");
            var userName = messages.Count > 0 ? messages.First().TargetUserName : L("Anonymous");

            return CreateCsvFile(
                $"Chat_{tenancyName}_{userName}.csv",
                () =>
                {
                    AddHeader(
                        L("ChatMessage_From"),
                        L("ChatMessage_To"),
                        L("Message"),
                        L("ReadState"),
                        L("CreationTime")
                    );

                    AddObjects(
                        messages,
                        _ => _.Side == ChatSide.Receiver ? (_.TargetTenantName + "/" + _.TargetUserName) : L("You"),
                        _ => _.Side == ChatSide.Receiver ? L("You") : (_.TargetTenantName + "/" + _.TargetUserName),
                        _ => _.Message,
                        _ => _.Side == ChatSide.Receiver ? _.ReadState.ToString() : _.ReceiverReadState.ToString(),
                        _ => _timeZoneConverter.Convert(_.CreationTime, user.TenantId, user.UserId)?.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")
                    );
                });
        }
    }
}
