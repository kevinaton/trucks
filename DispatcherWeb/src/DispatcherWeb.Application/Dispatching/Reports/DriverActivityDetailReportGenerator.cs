using Abp.Dependency;
using DispatcherWeb.Dispatching.Dto;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Reports
{
    public class DriverActivityDetailReportGenerator
    {
        public Document GenerateReport(DriverActivityDetailReportDto input)
        {
            Document document = new Document();

            Section section = document.AddSection();
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
            tableStyle.Font.Size = Unit.FromPoint(11);
            tableStyle.ParagraphFormat.SpaceAfter = 0;

            var headerStyle = document.Styles[StyleNames.Header];
            headerStyle.Font.Name = "Times New Roman";
            headerStyle.Font.Size = Unit.FromPoint(10);
            Paragraph paragraph = new Paragraph();
            paragraph.AddText("Page ");
            paragraph.AddPageField();
            paragraph.AddText(" of ");
            paragraph.AddNumPagesField();
            section.Headers.Primary.Add(paragraph);
            section.Headers.EvenPage.Add(paragraph.Clone());

            var firstPage = true;

            foreach (var model in input.Pages)
            {
                if (firstPage)
                {
                    firstPage = false;
                }
                else
                {
                    section.AddPageBreak();
                }


                paragraph = document.LastSection.AddParagraph("Driver Activity Report");
                paragraph.Format.Font.Size = Unit.FromPoint(18);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);
                paragraph.Format.Alignment = ParagraphAlignment.Center;

                var secondColumnMargin = Unit.FromCentimeter(7.5);


                paragraph = document.LastSection.AddParagraph(model.Date.ToString("d"));

                paragraph = document.LastSection.AddParagraph("Name: ");
                paragraph.AddText(model.DriverName ?? "");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText("Scheduled start time: ");
                paragraph.AddText(model.ScheduledStartTime?.ToShortTimeString() ?? "-");

                var elapsedTime = TimeSpan.FromSeconds(0);
                foreach (var employeeTime in model.EmployeeTimes)
                {
                    paragraph = document.LastSection.AddParagraph("Clock-in: ");
                    paragraph.AddText(employeeTime.ClockInTime.ConvertTimeZoneTo(input.Timezone).ToString("t") ?? "");
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddSpace(12);
                    paragraph.AddText(employeeTime.TimeClassificationName);
                    paragraph.AddTab();
                    paragraph.AddText("Clock-out: ");
                    paragraph.AddText(employeeTime.ClockOutTime?.ConvertTimeZoneTo(input.Timezone).ToString("t") ?? "");
                    if (employeeTime.ClockOutTime.HasValue)
                    {
                        elapsedTime += employeeTime.ClockOutTime.Value - employeeTime.ClockInTime;
                    }
                }

                paragraph = document.LastSection.AddParagraph("Elapsed Time: ");
                paragraph.AddText(Math.Round(elapsedTime.TotalHours, 2).ToString("0.##") + " Hours");



                paragraph = document.LastSection.AddParagraph("Tickets");

                Table table = document.LastSection.AddTable();
                table.Style = "Table";
                table.Borders.Width = Unit.FromPoint(1);
                var tm = new TextMeasurement(document.Styles["Table"].Font.Clone());

                //Customer
                table.AddColumn(Unit.FromCentimeter(3.4));
                //Load At
                table.AddColumn(Unit.FromCentimeter(3.5));
                //Deliver To
                table.AddColumn(Unit.FromCentimeter(3.5));
                //Ticket Number
                table.AddColumn(Unit.FromCentimeter(2.2));
                //Quantity
                table.AddColumn(Unit.FromCentimeter(2.2));
                //Load Time
                table.AddColumn(Unit.FromCentimeter(1.5)); //1.9 is still too small to fit 12:34 PM on one line
                //Delivery Time
                table.AddColumn(Unit.FromCentimeter(1.5));
                //Cycle Time
                table.AddColumn(Unit.FromCentimeter(1.2));

                Row row = table.AddRow();
                row.Shading.Color = Colors.LightGray;
                row.Format.Font.Size = Unit.FromPoint(9);
                row.Format.Font.Bold = true;
                row.Format.Alignment = ParagraphAlignment.Center;
                row.Height = Unit.FromCentimeter(0.5);
                row.HeadingFormat = true;

                int i = 0;
                Cell cell = row.Cells[i++];
                cell.AddParagraph("Customer");
                cell = row.Cells[i++];
                cell.AddParagraph("Load At");
                cell = row.Cells[i++];
                cell.AddParagraph("Deliver To");
                cell = row.Cells[i++];
                cell.AddParagraph("Ticket Number");
                cell = row.Cells[i++];
                cell.AddParagraph("Quantity");
                cell = row.Cells[i++];
                cell.AddParagraph("Load Time");
                cell = row.Cells[i++];
                cell.AddParagraph("Delivery Time");
                cell = row.Cells[i++];
                cell.AddParagraph("Cycle Time");

                if (model.Loads.Any())
                {
                    var lastTruckId = (int?)null;

                    foreach (var load in model.Loads)
                    {
                        if (load.TruckId != lastTruckId)
                        {
                            lastTruckId = load.TruckId;
                            i = 0;
                            row = table.AddRow();
                            cell = row.Cells[i++];
                            cell.MergeRight = table.Columns.Count - 1;
                            paragraph = cell.AddParagraph($"Truck {load.TruckCode}:", tm);
                            paragraph.Format.SpaceAfter = Unit.FromMillimeter(2);
                            paragraph.Format.SpaceBefore = Unit.FromMillimeter(1);
                        }

                        i = 0;
                        row = table.AddRow();
                        cell = row.Cells[i++];
                        if (!string.IsNullOrEmpty(load.JobNumber))
                        {
                            paragraph = cell.AddParagraph(load.CustomerName + " - " + load.JobNumber, tm);
                        }
                        else
                        {
                            paragraph = cell.AddParagraph(load.CustomerName, tm);
                        }
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(load.LoadAtName, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(load.DeliverToName, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(load.TicketNumber, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(load.Quantity.HasValue ? load.Quantity?.ToString(Utilities.NumberFormatWithoutRounding) + " " + load.UomName : "", tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(load.LoadTime?.ConvertTimeZoneTo(input.Timezone).ToShortTimeString(), tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(load.DeliveryTime?.ConvertTimeZoneTo(input.Timezone).ToShortTimeString(), tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(load.CycleTime?.ToString("h\\:mm") ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                    }

                    section.AddParagraph();
                    paragraph = document.LastSection.AddParagraph("Job Summary");

                    Table summaryTable = document.LastSection.AddTable();
                    summaryTable.Style = "Table";
                    summaryTable.Borders.Width = Unit.FromPoint(1);

                    //Customer
                    summaryTable.AddColumn(Unit.FromCentimeter(3.85));
                    //Load At
                    summaryTable.AddColumn(Unit.FromCentimeter(3.95));
                    //Deliver To
                    summaryTable.AddColumn(Unit.FromCentimeter(3.95));
                    //Ticket Number
                    summaryTable.AddColumn(Unit.FromCentimeter(2.65));
                    //Quantity
                    summaryTable.AddColumn(Unit.FromCentimeter(2.62));
                    //Load Time
                    summaryTable.AddColumn(Unit.FromCentimeter(1.95)); //1.9 is still too small to fit 12:34 PM on one line
                    
                    Row summaryRow = summaryTable.AddRow();
                    summaryRow.Shading.Color = Colors.LightGray;
                    summaryRow.Format.Font.Size = Unit.FromPoint(9);
                    summaryRow.Format.Font.Bold = true;
                    summaryRow.Format.Alignment = ParagraphAlignment.Center;
                    summaryRow.Height = Unit.FromCentimeter(0.5);
                    summaryRow.HeadingFormat = true;

                    i = 0;
                    Cell summaryCell = summaryRow.Cells[i++];
                    summaryCell.AddParagraph("Customer");
                    summaryCell = summaryRow.Cells[i++];
                    summaryCell.AddParagraph("Load At");
                    summaryCell = summaryRow.Cells[i++];
                    summaryCell.AddParagraph("Deliver To");
                    summaryCell = summaryRow.Cells[i++];
                    summaryCell.AddParagraph("Item");
                    summaryCell = summaryRow.Cells[i++];
                    summaryCell.AddParagraph("Quantity");
                    summaryCell = summaryRow.Cells[i++];
                    summaryCell.AddParagraph("Job Time");
                    
                    foreach (var jobLoads in model.Loads.GroupBy(l => l.OrderLineId))
                    {
                        i = 0;
                        summaryRow = summaryTable.AddRow();
                        summaryCell = summaryRow.Cells[i++]; 

                        var job = jobLoads.First();
                        var jobNumber = job.JobNumber;
                        if (!string.IsNullOrEmpty(jobNumber))
                        {
                            paragraph = summaryCell.AddParagraph(job.CustomerName + " - " + jobNumber, tm);
                        }
                        else
                        {
                            paragraph = summaryCell.AddParagraph(job.CustomerName, tm);
                        }

                        summaryCell = summaryRow.Cells[i++];
                        paragraph = summaryCell.AddParagraph(job.LoadAtName, tm);
                        summaryCell = summaryRow.Cells[i++];
                        paragraph = summaryCell.AddParagraph(job.DeliverToName, tm);
                        summaryCell = summaryRow.Cells[i++];
                        paragraph = summaryCell.AddParagraph(job.ProductOrService, tm);
                        summaryCell = summaryRow.Cells[i++];
                        paragraph = summaryCell.AddParagraph(jobLoads.Where(j => j.Quantity.HasValue).Sum(j => j.Quantity.Value).ToString(Utilities.NumberFormatWithoutRounding) + " " + job.UomName, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        summaryCell = summaryRow.Cells[i++];
                        paragraph = summaryCell.AddParagraph(new TimeSpan(jobLoads.Where(s => s.CycleTime.HasValue).Sum(s => s.CycleTime.Value.Ticks)).ToString("h\\:mm") ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        
                    }
                }
                else
                {
                    table.AddRow();
                }

                table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
            }

            return document;
        }
    }
}
