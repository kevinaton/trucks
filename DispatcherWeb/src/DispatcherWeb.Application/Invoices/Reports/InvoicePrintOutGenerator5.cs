﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Invoices.Dto;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace DispatcherWeb.Invoices.Reports
{
    public class InvoicePrintOutGenerator5 : ITransientDependency
    {
        public InvoicePrintOutGenerator5()
        {
        }

        public Task<Document> GenerateReport(List<InvoicePrintOutDto> modelList)
        {
            Document document = new Document();

            Section section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            //section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.PageHeight = Unit.FromInch(11); //27.94cm
            section.PageSetup.PageWidth = Unit.FromInch(8.5); //21.59cm -3cm margin = 18.6cm total
            section.PageSetup.TopMargin = Unit.FromCentimeter(1);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.6);

            Style style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Times New Roman";
            style.Font.Size = Unit.FromPoint(11);
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
            tableStyle.Font.Name = "Times New Roman";
            tableStyle.Font.Size = Unit.FromPoint(10);
            tableStyle.ParagraphFormat.SpaceAfter = 0;

            Paragraph paragraph;
            Table table;
            Row row;
            Cell cell;

            //var headerStyle = document.Styles[StyleNames.Header];
            //headerStyle.Font.Name = "Times New Roman";
            //headerStyle.Font.Size = Unit.FromPoint(10);
            //paragraph = new Paragraph();
            //paragraph.AddText("Page ");
            //paragraph.AddPageField();
            //paragraph.AddText(" of ");
            //paragraph.AddNumPagesField();
            //section.Headers.Primary.Add(paragraph);
            //section.Headers.EvenPage.Add(paragraph.Clone());

            var firstInvoice = true;

            foreach (var model in modelList)
            {
                if (firstInvoice)
                {
                    firstInvoice = false;
                }
                else
                {
                    section.AddPageBreak();
                }

                //string taxWarning = null;
                //const string taxWarningAsterisks = "**";


                table = document.LastSection.AddTable();
                table.Style = "Table";
                if (model.DebugLayout)
                {
                    table.Borders.Width = Unit.FromPoint(1);
                }
                //todo, since we have different font sizes on a page
                //var tm = new TextMeasurement(document.Styles["Table"].Font.Clone());

                //18.6cm total width
                //Legal Name and Legal Address
                table.AddColumn(Unit.FromCentimeter(10));
                //logo
                table.AddColumn(Unit.FromCentimeter(8.6));

                row = table.AddRow();
                row.Format.Alignment = ParagraphAlignment.Left;
                row.Format.Font.Size = Unit.FromPoint(10);

                int i = 0;

                cell = row.Cells[i++];
                paragraph = cell.AddParagraph(model.LegalName ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(14);
                paragraph.Format.Font.Bold = true;
                //paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);
                paragraph = cell.AddParagraph(model.LegalAddress ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(10);


                cell = row.Cells[i++];
                if (model.LogoPath != null)
                {
                    paragraph = cell.AddParagraph();
                    var logo = paragraph.AddImage(model.LogoPath);
                    //logo.Height = Unit.FromCentimeter(3.2);
                    logo.Width = Unit.FromCentimeter(4.21);
                    logo.LockAspectRatio = true;
                    //cell.Format.Alignment = ParagraphAlignment.Left; //default
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                }

                paragraph = document.LastSection.AddParagraph();
                paragraph.Format.Font.Size = Unit.FromPoint(1);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);



                table = document.LastSection.AddTable();
                table.Style = "Table";
                if (model.DebugLayout)
                {
                    table.Borders.Width = Unit.FromPoint(1);
                }

                //empty space
                table.AddColumn(Unit.FromCentimeter(13));
                //date labels
                table.AddColumn(Unit.FromCentimeter(2.2));
                //Invoice #, dates
                table.AddColumn(Unit.FromCentimeter(3.4));


                row = table.AddRow();
                row.Format.Alignment = ParagraphAlignment.Left;
                row.Format.Font.Size = Unit.FromPoint(10);

                i = 0;

                cell = row.Cells[i++];
                cell = row.Cells[i++];
                paragraph = cell.AddParagraph(" ");
                paragraph.Format.Font.Size = Unit.FromPoint(12);
                paragraph.Format.Font.Bold = true;

                paragraph = cell.AddParagraph($"Issue Date");

                paragraph = cell.AddParagraph($"Due Date");

                cell = row.Cells[i++];
                cell.Format.Alignment = ParagraphAlignment.Right;
                paragraph = cell.AddParagraph($"Invoice #{model.InvoiceId}");
                paragraph.Format.Font.Size = Unit.FromPoint(12);
                paragraph.Format.Font.Bold = true;
                paragraph = cell.AddParagraph(model.IssueDate?.ToShortDateString() ?? "");
                paragraph = cell.AddParagraph(model.DueDate?.ToShortDateString() ?? "");


                paragraph = document.LastSection.AddParagraph();
                paragraph.Format.Font.Size = Unit.FromPoint(1);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

                paragraph = document.LastSection.AddParagraph(); //more spacing between the tables



                table = document.LastSection.AddTable();
                table.Style = "Table";
                if (model.DebugLayout)
                {
                    table.Borders.Width = Unit.FromPoint(1);
                }

                //18.6cm total width
                //Bill To
                table.AddColumn(Unit.FromCentimeter(7));
                //Remit To
                table.AddColumn(Unit.FromCentimeter(7));
                //Balance Due
                table.AddColumn(Unit.FromCentimeter(4.6));

                row = table.AddRow();
                row.Format.Alignment = ParagraphAlignment.Left;
                row.Format.Font.Size = Unit.FromPoint(11);

                i = 0;
                cell = row.Cells[i++];
                paragraph = cell.AddParagraph("Bill To:");
                paragraph.Format.Font.Size = Unit.FromPoint(11);
                paragraph.Format.Font.Bold = true;
                paragraph = cell.AddParagraph(model.CustomerName ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(10);
                paragraph = cell.AddParagraph(model.BillingAddress ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(10);


                cell = row.Cells[i++];
                paragraph = cell.AddParagraph("Remit To:");
                paragraph.Format.Font.Size = Unit.FromPoint(11);
                paragraph.Format.Font.Bold = true;
                paragraph = cell.AddParagraph(model.RemitToInformation);
                paragraph.Format.Font.Size = Unit.FromPoint(10);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.2);
                if (!string.IsNullOrEmpty(model.PoNumber))
                {
                    paragraph = cell.AddParagraph("PO Number:");
                    paragraph.Format.Font.Size = Unit.FromPoint(11);
                    paragraph.Format.Font.Bold = true;
                    paragraph = cell.AddParagraph(model.PoNumber);
                    paragraph.Format.Font.Size = Unit.FromPoint(10);
                }


                cell = row.Cells[i++];
                cell.Format.Alignment = ParagraphAlignment.Right;
                paragraph = cell.AddParagraph("Balance Due");
                paragraph.Format.Font.Size = Unit.FromPoint(12);
                paragraph.Format.Font.Bold = true;
                paragraph = cell.AddParagraph(model.TotalAmount.ToString("C2", model.CurrencyCulture) ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(12);
                paragraph.Format.Font.Bold = true;

                paragraph = document.LastSection.AddParagraph();
                paragraph.Format.Font.Size = Unit.FromPoint(1);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);



                table = document.LastSection.AddTable();
                table.Style = "Table";
                table.Borders.Width = Unit.FromPoint(1);
                var tm = new TextMeasurement(document.Styles["Table"].Font.Clone());

                //18.6cm total width
                //Date
                table.AddColumn(Unit.FromCentimeter(2));
                //Truck
                table.AddColumn(Unit.FromCentimeter(1.6));
                //Ticket #
                table.AddColumn(Unit.FromCentimeter(1.6));
                //Job Number
                table.AddColumn(Unit.FromCentimeter(2.4));
                //Description
                table.AddColumn(Unit.FromCentimeter(5.4));
                //Qty
                table.AddColumn(Unit.FromCentimeter(1.8));
                //Rate
                table.AddColumn(Unit.FromCentimeter(1.8));
                //Amount
                table.AddColumn(Unit.FromCentimeter(2));


                row = table.AddRow();
                row.Shading.Color = Colors.LightGray;
                row.Format.Font.Size = Unit.FromPoint(9);
                row.Format.Font.Bold = true;
                row.Format.Alignment = ParagraphAlignment.Center;
                row.Height = Unit.FromCentimeter(0.5);
                row.HeadingFormat = true;

                i = 0;
                cell = row.Cells[i++];
                cell.AddParagraph("Date");
                cell = row.Cells[i++];
                cell.AddParagraph("Truck");
                cell = row.Cells[i++];
                cell.AddParagraph("Ticket #");
                cell = row.Cells[i++];
                cell.AddParagraph("Job #");
                cell = row.Cells[i++];
                cell.AddParagraph("Description");
                cell = row.Cells[i++];
                cell.AddParagraph("Qty");
                cell = row.Cells[i++];
                cell.AddParagraph("Rate");
                cell = row.Cells[i++];
                cell.AddParagraph("Amount");


                if (model.InvoiceLines.Any())
                {
                    model.InvoiceLines = ReorderLineItems(model.InvoiceLines);
                    foreach (var invoiceLine in model.InvoiceLines)
                    {
                        i = 0;
                        row = table.AddRow();
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.DeliveryDateTime?.ToShortDateString(), tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.TruckCode, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.TicketNumber, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.JobNumber, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.Description, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.Quantity.ToString(Utilities.NumberFormatWithoutRounding), tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.RateSum?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)) ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(invoiceLine.Subtotal.ToString("C2", model.CurrencyCulture), tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                    }
                }
                else
                {
                    table.AddRow();
                }

                table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);

                if (model.InvoiceLines.Any())
                {
                    var footerRows = new List<(string caption, decimal value)>
                    {
                        ("Subtotal:", model.TotalAmount - model.Tax),
                        ("Tax:", model.Tax),
                        ("Total:", model.TotalAmount)
                    };

                    foreach (var footerRow in footerRows )
                    {
                        row = table.AddRow();
                        cell = row.Cells[0];
                        cell.MergeRight = 6;
                        cell.Borders.Visible = false;
                        cell.Borders.Left.Visible = true;
                        if (footerRow == footerRows.Last())
                        {
                            cell.Borders.Bottom.Visible = true;
                        }
                        paragraph = cell.AddParagraph(footerRow.caption, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                        cell = row.Cells[7];
                        cell.Borders.Visible = false;
                        cell.Borders.Right.Visible = true;
                        if (footerRow == footerRows.Last())
                        {
                            cell.Borders.Bottom.Visible = true;
                        }
                        paragraph = cell.AddParagraph(footerRow.value.ToString("C2", model.CurrencyCulture), tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Right;
                    }
                }


                paragraph = document.LastSection.AddParagraph(model.Message ?? "");
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);

                
                if (!string.IsNullOrEmpty(model.TermsAndConditions))
                {
                    paragraph = document.LastSection.AddParagraph();
                    paragraph.Format.Font.Size = Unit.FromPoint(7.5);

                    paragraph.AddLineBreak();
                    paragraph.AddLineBreak();
                    paragraph.AddLineBreak();
                    paragraph.AddText(model.TermsAndConditions);
                    paragraph.AddLineBreak();
                    paragraph.AddLineBreak();
                }

                //if (!taxWarning.IsNullOrEmpty())
                //{
                //    paragraph = document.LastSection.AddParagraph(taxWarningAsterisks + taxWarning);
                //    paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
                //}
            }

            return Task.FromResult(document);
        }

        public List<InvoicePrintOutLineItemDto> ReorderLineItems(List<InvoicePrintOutLineItemDto> invoiceLines)
        {
            invoiceLines = invoiceLines
                .OrderBy(x => x.DeliveryDateTime?.Date)
                .ThenBy(x => x.TruckCode)
                .ThenBy(x => x.TicketNumber)
                .ToList();

            var regularInvoiceLines = invoiceLines.Where(x => x.ChildInvoiceLineKind == null).ToList();
            var bottomLines = invoiceLines.Where(x => x.ChildInvoiceLineKind == ChildInvoiceLineKind.BottomFuelSurchargeLine).ToList();
            var perTicketLines = invoiceLines.Where(x => x.ChildInvoiceLineKind == ChildInvoiceLineKind.FuelSurchargeLinePerTicket).ToList();

            var reorderedLines = regularInvoiceLines.ToList();
            foreach (var perTicketLine in perTicketLines)
            {
                if (perTicketLine.ParentInvoiceLineId != null)
                {
                    var parentLine = reorderedLines.FirstOrDefault(x => x.Id == perTicketLine.ParentInvoiceLineId);
                    if (parentLine != null)
                    {
                        var parentLineIndex = reorderedLines.IndexOf(parentLine);
                        if (parentLineIndex != -1)
                        {
                            reorderedLines.Insert(parentLineIndex + 1, perTicketLine);
                            continue;
                        }
                    }
                }
                reorderedLines.Add(perTicketLine);
            }
            foreach (var bottomLine in bottomLines)
            {
                reorderedLines.Add(bottomLine);
            }

            return reorderedLines;
        } 
    }
}
