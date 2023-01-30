﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DispatcherWeb.DriverAssignments.Dto;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace DispatcherWeb.DriverAssignments
{
    public static class DriverAssignmentReportGenerator
    {
        public static byte[] GenerateReport(DriverAssignmentReportDto model)
        {
            Document document = new Document();

            Section section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            //section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.PageHeight = Unit.FromInch(11);
            section.PageSetup.PageWidth = Unit.FromInch(8.5);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.3);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2.5);

            Style style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Times New Roman";
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.5);

            var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
            tableStyle.Font.Name = "Times New Roman";
            tableStyle.Font.Size = Unit.FromPoint(9.75);
            tableStyle.ParagraphFormat.SpaceAfter = 0;

            //style = document.Styles[StyleNames.Footer];
            //style.ParagraphFormat.AddTabStop(Unit.FromCentimeter(14), TabAlignment.Right);
            //paragraph = new Paragraph();
            //paragraph.AddDateField();
            //paragraph.AddTab();
            //paragraph.AddText("Page ");
            //paragraph.AddPageField();
            //paragraph.AddText(" of ");
            //paragraph.AddNumPagesField();
            //section.Footers.Primary.Add(paragraph);
            //section.Footers.EvenPage.Add(paragraph.Clone());

            Paragraph paragraph = document.LastSection.AddParagraph("Driver Assignments");
            paragraph.Format.Font.Size = Unit.FromPoint(18);

            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.Font.Size = Unit.FromPoint(12);
            paragraph.AddText("Date: " + model.Date.ToShortDateString());
			if (model.Shift.HasValue)
			{
				paragraph.Format.AddTabStop(Unit.FromCentimeter(6), TabAlignment.Right);
				paragraph.AddTab();
				paragraph.AddText("Shift: " + model.ShiftName);
			}
			else
			{
				paragraph.Format.AddTabStop(Unit.FromCentimeter(12), TabAlignment.Right);
			}
			paragraph.AddTab();
			paragraph.AddText("Office: " + model.OfficeName);

            Table table = document.LastSection.AddTable();
            table.Style = "Table";
            table.Borders.Width = Unit.FromPoint(1);
            var tm = new TextMeasurement(document.Styles["Table"].Font.Clone());

            //Truck
            table.AddColumn(Unit.FromCentimeter(3.1));
            //Driver
            table.AddColumn(Unit.FromCentimeter(3.2));
            //Load at location and time
            table.AddColumn(Unit.FromCentimeter(4.1));
            //Note
            table.AddColumn(Unit.FromCentimeter(6.5));

            Row row = table.AddRow();
            row.Height = Unit.FromCentimeter(1.2);
            row.HeadingFormat = true;

            int i = 0;
            Cell cell = row.Cells[i++];
            cell.AddParagraph("Truck");
            cell = row.Cells[i++];
            cell.AddParagraph("Driver");
            cell = row.Cells[i++];
            cell.AddParagraph("First Time on Job");
            cell = row.Cells[i++];
            cell.AddParagraph("Start time");

            if (model.Items.Any())
            {
                foreach (var item in model.Items)
                {
                    i = 0;
                    row = table.AddRow();
                    cell = row.Cells[i++];
                    cell.AddParagraph(item.TruckCode, tm);
                    cell = row.Cells[i++];
                    cell.AddParagraph(item.DriverName, tm);
                    cell = row.Cells[i++];
                    var firstTimeOnJob = item.FirstTimeOnJob?.ToString("t") + (!string.IsNullOrEmpty(item.LoadAtName) ? " at " + item.LoadAtName : "");
                    cell.AddParagraph(firstTimeOnJob, tm);
                    cell = row.Cells[i++];
                    cell.AddParagraph(item.StartTime?.ToString("t") ?? "", tm);
                }
            }
            else
            {
                table.AddRow();
            }


            table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);

            return document.SaveToBytesArray();
        }
    }
}
