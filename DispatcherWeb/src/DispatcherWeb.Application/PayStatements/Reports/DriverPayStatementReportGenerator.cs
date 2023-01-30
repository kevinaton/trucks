using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Extensions;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.PayStatements.Dto;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;

namespace DispatcherWeb.Orders.Reports
{
    public class DriverPayStatementReportGenerator : ITransientDependency
    {
        public ISettingManager SettingManager { get; }
        public IFeatureChecker FeatureChecker { get; }

        public DriverPayStatementReportGenerator(
            ISettingManager settingManager, 
            IFeatureChecker featureChecker
            )
        {
            SettingManager = settingManager;
            FeatureChecker = featureChecker;
        }

        public DriverPayStatementReport GenerateReportAndZip(PayStatementReportDto model, GetDriverPayStatementReportInput input)
        {
            var reportList = GenerateReport(model, input);
            if (reportList.Count == 1)
            {
                var report = reportList.First();
                report.FileName += ".pdf";
                report.MimeType = "application/pdf";
                return report;
            }

            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Update, true))
                {
                    foreach (var report in reportList)
                    {
                        var entry = archive.CreateEntry(report.FileName + ".pdf", CompressionLevel.NoCompression);
                        using (var entryStream = entry.Open())
                        {
                            entryStream.Write(report.FileBytes);
                        }
                    }

                }
                
                return new DriverPayStatementReport
                {
                    FileBytes = zipStream.ToArray(),
                    FileName = GetFilename(model) + ".zip",
                    MimeType = "application/zip"
                };
            }
        }

        private string GetFilename(PayStatementReportDto model)
        {
            return $"Pay Statement through {model.EndDate:yyyy-MM-dd}".SanitizeFilename();
        }

        public List<DriverPayStatementReport> GenerateReport(PayStatementReportDto model, GetDriverPayStatementReportInput input)
        {
            var result = new List<DriverPayStatementReport>();
            Document document;
            Section section;
            Paragraph paragraph;

            void InitNewDocument()
            {
                document = new Document();
                section = document.AddSection();
                section.PageSetup = document.DefaultPageSetup.Clone();
                //section.PageSetup.Orientation = Orientation.Landscape;
                section.PageSetup.PageFormat = PageFormat.Letter;
                section.PageSetup.PageHeight = Unit.FromInch(11);
                section.PageSetup.PageWidth = Unit.FromInch(8.5);
                section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.6);

                Style style = document.Styles[StyleNames.Normal];
                style.Font.Name = "Times New Roman";
                style.Font.Size = Unit.FromPoint(12);
                style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

                var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
                tableStyle.Font.Name = "Times New Roman";
                tableStyle.Font.Size = Unit.FromPoint(9);
                tableStyle.ParagraphFormat.SpaceAfter = 0;

                var headerStyle = document.Styles[StyleNames.Header];
                headerStyle.Font.Name = "Times New Roman";
                headerStyle.Font.Size = Unit.FromPoint(10);
                paragraph = new Paragraph();
                paragraph.AddText("Page ");
                paragraph.AddPageField();
                paragraph.AddText(" of ");
                paragraph.AddNumPagesField();
                section.Headers.Primary.Add(paragraph);
                section.Headers.EvenPage.Add(paragraph.Clone());
            }
            InitNewDocument();
            
            var filename = GetFilename(model);

            var firstPage = true;
            foreach (var driverModel in model.Drivers.OrderBy(x => x.DriverName))
            {
                if (firstPage)
                {
                    firstPage = false;
                }
                else
                {
                    if (input.SplitByDriver)
                    {
                        InitNewDocument();
                    }
                    else
                    {
                        section.AddPageBreak();
                    }
                }

                paragraph = document.LastSection.AddParagraph(driverModel.DriverName ?? "");
                paragraph = document.LastSection.AddParagraph($"Through {model.EndDate:d}");
                //paragraph.Format.Font.Size = Unit.FromPoint(18);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);

                var items = new List<PayStatementItemEditDto>();
                items.AddRange(driverModel.TimeRecords.Select(t => new PayStatementItemEditDto
                {
                    ItemKind = PayStatementItemKind.Time,
                    Date = t.Date,
                    Quantity = t.Quantity,
                    Total = t.Total,
                    DriverPayRate = t.DriverPayRate,
                    TimeClassificationName = t.TimeClassificationName,
                    TimeClassificationId = t.TimeClassificationId,
                    IsProductionPay = t.IsProductionPay
                }));
                items.AddRange(driverModel.Tickets.Select(t => new PayStatementItemEditDto
                {
                    ItemKind = PayStatementItemKind.Ticket,
                    Date = t.TicketDateTime,
                    Quantity = t.Quantity,
                    Total = t.Total,
                    DriverPayRate = t.DriverPayRate,
                    TimeClassificationName = t.TimeClassificationName,
                    TimeClassificationId = t.TimeClassificationId,
                    IsProductionPay = t.IsProductionPay,
                    DeliverTo = t.DeliverTo,
                    CustomerName = t.CustomerName,
                    FreightRate = t.FreightRate,
                    Item = t.Item,
                    JobNumber = t.JobNumber,
                    LoadAt = t.LoadAt
                }));

                var allowProductionPay = SettingManager.GetSettingValue<bool>(AppSettings.TimeAndPay.AllowProductionPay) && FeatureChecker.IsEnabled(AppFeatures.DriverProductionPayFeature);

                if (items.Any())
                {
                    Table table = document.LastSection.AddTable();
                    table.Style = "Table";
                    table.Borders.Width = Unit.FromPoint(1);
                    var tm = new TextMeasurement(document.Styles["Table"].Font.Clone());

                    //Date
                    table.AddColumn(Unit.FromCentimeter(1.8));
                    if (allowProductionPay)
                    {
                        //Item
                        table.AddColumn(Unit.FromCentimeter(2));
                        //Customer
                        table.AddColumn(Unit.FromCentimeter(1.5));
                        //Job#
                        table.AddColumn(Unit.FromCentimeter(1));
                        //Deliver To
                        table.AddColumn(Unit.FromCentimeter(2.5));
                        //Load At
                        table.AddColumn(Unit.FromCentimeter(2.5));
                    }
                    //Time classification
                    table.AddColumn(Unit.FromCentimeter(1.8));
                    if (allowProductionPay)
                    {
                        //Freight Rate
                        table.AddColumn(Unit.FromCentimeter(1.2));
                    }
                    //Driver Pay Rate
                    table.AddColumn(Unit.FromCentimeter(1.2));
                    //Quantity
                    table.AddColumn(Unit.FromCentimeter(1.5));
                    //Total
                    table.AddColumn(Unit.FromCentimeter(1.7));


                    Row row = table.AddRow();
                    row.Shading.Color = Colors.LightGray;
                    row.Format.Font.Size = Unit.FromPoint(9);
                    row.Format.Font.Bold = true;
                    row.Format.Alignment = ParagraphAlignment.Center;
                    row.Height = Unit.FromCentimeter(0.5);
                    row.HeadingFormat = true;

                    int i = 0;
                    Cell cell = row.Cells[i++];
                    cell.AddParagraph("Date");
                    if (allowProductionPay)
                    {
                        cell = row.Cells[i++];
                        cell.AddParagraph("Item");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Customer");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Job #");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Deliver To");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Load At");
                    }
                    cell = row.Cells[i++];
                    cell.AddParagraph("Time class.");
                    if (allowProductionPay)
                    {
                        cell = row.Cells[i++];
                        cell.AddParagraph("Freight Rate");
                    }
                    cell = row.Cells[i++];
                    cell.AddParagraph("Driver Pay Rate");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Quantity");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Total");

                    foreach (var item in items.OrderBy(x => x.Date))
                    {
                        i = 0;
                        row = table.AddRow();
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.Date?.ToString("d"), tm);
                        //paragraph.Format.Alignment = ParagraphAlignment.Center;
                        if (allowProductionPay)
                        {
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.Item, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.CustomerName, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.JobNumber, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.DeliverToName, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.LoadAtName, tm);
                        }
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.TimeClassificationName ?? "", tm);
                        if (allowProductionPay)
                        {
                            cell = row.Cells[i++];
                            var freightRate = item.ItemKind == PayStatementItemKind.Ticket
                                ? item.FreightRate.ToString("C2", model.CurrencyCulture)
                                : "";
                            paragraph = cell.AddParagraph(freightRate ?? "", tm);
                            paragraph.Format.Alignment = ParagraphAlignment.Right;
                        }
                        cell = row.Cells[i++];
                        var driverPayRate = item.IsProductionPay
                            ? (item.DriverPayRate?.ToString(Utilities.NumberFormatWithoutRounding) ?? "0") + "%"
                            : item.DriverPayRate?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture));
                        paragraph = cell.AddParagraph(driverPayRate ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.Quantity.ToString(Utilities.NumberFormatWithoutRounding) ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.Total.ToString("C2", model.CurrencyCulture) ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                    }

                    table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);


                    row = table.AddRow();
                    row.Borders.Visible = false;
                    row = table.AddRow();
                    row.Borders.Visible = false;
                    cell = row.Cells[0];
                    cell.MergeRight = 3 + (allowProductionPay ? 6 : 0);
                    paragraph = cell.AddParagraph($"Total: ", tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Right;

                    cell = row.Cells[4 + (allowProductionPay ? 6 : 0)];
                    paragraph = cell.AddParagraph(driverModel.Total.ToString("C2", model.CurrencyCulture) ?? "", tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                    paragraph.Format.Font.Bold = true;
                }
                
                //if (driverModel.PayMethod == PayMethod.Salary)
                //{
                //    paragraph = document.LastSection.AddParagraph($"Salary: {driverModel.PayRate.ToString("C2", model.CurrencyCulture)}");
                //}



                if (input.SplitByDriver)
                {
                    var filenameByDriver = $"{filename} for {driverModel.DriverName}".SanitizeFilename();
                    if (result.Any(x => x.FileName == filenameByDriver))
                    {
                        var counter = 2;
                        var newFileName = filenameByDriver + counter;
                        while (result.Any(x => x.FileName == newFileName))
                        {
                            counter++;
                        }
                        filenameByDriver = newFileName;
                    }

                    result.Add(new DriverPayStatementReport
                    {
                        FileBytes = document.SaveToBytesArray(),
                        FileName = filenameByDriver
                    });
                }
            }

            if (!input.SplitByDriver)
            {
                result.Add(new DriverPayStatementReport
                {
                    FileBytes = document.SaveToBytesArray(),
                    FileName = filename
                });
            }

            return result;
        }
    }
}
