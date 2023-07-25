using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using DispatcherWeb.Orders.Dto;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using static DispatcherWeb.Orders.Dto.WorkOrderReportDto;

namespace DispatcherWeb.Orders.Reports
{
    public class WorkOrderReportGenerator : ITransientDependency
    {
        private readonly OrderTaxCalculator _orderTaxCalculator;

        public WorkOrderReportGenerator(OrderTaxCalculator orderTaxCalculator)
        {
            _orderTaxCalculator = orderTaxCalculator;
        }

        public async Task<Document> GenerateReport(List<WorkOrderReportDto> modelList)
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

            var firstOrder = true;

            foreach (var model in modelList)
            {
                if (firstOrder)
                {
                    firstOrder = false;
                }
                else
                {
                    section.AddPageBreak();
                }

                string taxWarning = null;
                const string taxWarningAsterisks = "**";

                if (model.UseActualAmount)
                {
                    model.Items.ForEach(x =>
                    {
                        x.FreightPrice = x.IsFreightTotalOverridden
                            ? x.FreightPrice
                            : decimal.Round((x.FreightPricePerUnit ?? 0) * (x.ActualQuantity ?? 0), 2);

                        x.MaterialPrice = x.IsMaterialTotalOverridden
                            ? x.MaterialPrice
                            : decimal.Round((x.MaterialPricePerUnit ?? 0) * (x.ActualQuantity ?? 0), 2);
                    });

                    var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

                    if (taxCalculationType == TaxCalculationType.NoCalculation)
                    {
                        if (model.IsShared)
                        {
                            model.SalesTax = 0;
                            taxWarning = "Unable to calculate for shared orders";
                        }
                        else
                        {
                            //keep the full sales tax for the office
                        }
                    }

                    await _orderTaxCalculator.CalculateTotalsAsync(model, model.Items);
                }

                Table table = document.LastSection.AddTable();
                if (model.DebugLayout)
                {
                    table.Borders.Width = Unit.FromPoint(1);
                }

                //text
                table.AddColumn(Unit.FromCentimeter(13));
                //logo
                table.AddColumn(Unit.FromCentimeter(4.21));

                int j = 0;
                Row row = table.AddRow();
                Cell cell = row.Cells[j++];
                
                paragraph = cell.AddParagraph(model.UseReceipts ? "Receipt" : model.ShowDeliveryInfo ? "Delivery Report" : model.OrderIsPending ? "Quote" : "Work Order");
                paragraph.Format.Font.Size = Unit.FromPoint(18);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);

                var secondColumnMargin = Unit.FromCentimeter(7.5);

                if (model.AuthorizationCaptureDateTime.HasValue)
                {
                    var date = model.AuthorizationCaptureDateTime?.ConvertTimeZoneTo(model.TimeZone).ToString("g");

                    var paidImage = section.AddImage(model.PaidImagePath);
                    paidImage.Height = Unit.FromCentimeter(2.1);
                    paidImage.Width = Unit.FromCentimeter(4.5);
                    //paidImage.LockAspectRatio = true;
                    paidImage.RelativeVertical = RelativeVertical.Page;
                    paidImage.RelativeHorizontal = RelativeHorizontal.Page;
                    paidImage.WrapFormat.Style = WrapStyle.Through;
                    paidImage.WrapFormat.DistanceLeft = Unit.FromCentimeter(15.5);
                    paidImage.WrapFormat.DistanceTop = Unit.FromCentimeter(6);

                    var paidInfo = section.AddTextFrame();
                    paidInfo.Width = Unit.FromCentimeter(6);
                    paidInfo.Height = Unit.FromCentimeter(5);
                    paidInfo.RelativeVertical = RelativeVertical.Page;
                    paidInfo.RelativeHorizontal = RelativeHorizontal.Page;
                    paidInfo.WrapFormat.Style = WrapStyle.Through;
                    paidInfo.WrapFormat.DistanceLeft = Unit.FromCentimeter(15.9);
                    paidInfo.WrapFormat.DistanceTop = Unit.FromCentimeter(8);

                    paragraph = paidInfo.AddParagraph("Amount: " + model.AuthorizationCaptureSettlementAmount?.ToString("C2", model.CurrencyCulture));
                    paragraph = paidInfo.AddParagraph(date);
                    paragraph = paidInfo.AddParagraph("Id: " + model.AuthorizationCaptureTransactionId);
                }
                else if (model.AuthorizationDateTime.HasValue && model.ShowPaymentStatus)
                {
                    var date = model.AuthorizationDateTime?.ConvertTimeZoneTo(model.TimeZone).ToString("g");
                    document.LastSection.AddParagraph("Authorized " + date);
                }

                paragraph = cell.AddParagraph("Order Number: " + model.Id);

                if (model.ShowSpectrumNumber)
                {
                    paragraph = cell.AddParagraph(model.SpectrumNumberLabel + ": "); //Spectrum #
                    paragraph.AddText(model.SpectrumNumber ?? "");
                }

                paragraph = cell.AddParagraph("Account #: ");
                paragraph.AddText(model.CustomerAccountNumber ?? "");
                if (model.ShowOfficeName)
                {
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Office: ");
                    paragraph.AddText(model.OfficeName ?? "");
                }

                paragraph = cell.AddParagraph("Delivery Date: ");
                paragraph.AddText(model.OrderDeliveryDate?.ToShortDateString() ?? "");

                if (model.OrderShift.HasValue)
                {
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Shift: ");
                    paragraph.AddText(model.OrderShiftName);
                }

                paragraph = cell.AddParagraph("Customer: ");
                paragraph.AddText(model.CustomerName ?? "");

                paragraph = cell.AddParagraph("Contact: ");
                paragraph.AddText(model.ContactFullDetails ?? "");

                paragraph = cell.AddParagraph("PO Number: ");
                paragraph.AddText(model.PoNumber ?? "");

                if (!model.HidePrices && model.SplitRateColumn)
                {
                    paragraph = cell.AddParagraph("Material Total: ");
                    paragraph.AddText(model.MaterialTotal.ToString("C2", model.CurrencyCulture) ?? "");
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Freight Total: ");
                    paragraph.AddText(model.FreightTotal.ToString("C2", model.CurrencyCulture) ?? "");
                }

                if (!model.HidePrices)
                {
                    paragraph = cell.AddParagraph("Sales Tax: ");

                    if (taxWarning.IsNullOrEmpty())
                    {
                        paragraph.AddText(model.SalesTax.ToString(Utilities.NumberFormatWithoutRounding) ?? "");
                    }
                    else
                    {
                        paragraph.AddText(taxWarningAsterisks);
                    }

                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Total: ");
                    paragraph.AddText(model.CodTotal.ToString("C2", model.CurrencyCulture) ?? "");
                }

                paragraph = cell.AddParagraph("Charge To: ");
                paragraph.AddText(model.ChargeTo ?? "");

                if (model.UseActualAmount) //this flag is set to true for backoffice reports
                {
                    paragraph = cell.AddParagraph("Trucks: ");
                    paragraph.AddText(JoinTrucks(model.GetNonLeasedTrucks(), model.ShowDriverNamesOnPrintedOrder));
                    var leasedTrucks = model.GetLeasedTrucks();
                    if (leasedTrucks.Any())
                    {
                        paragraph = cell.AddParagraph("Lease Hauler Trucks: ");
                        paragraph.AddText(JoinTrucks(leasedTrucks, model.ShowDriverNamesOnPrintedOrder));
                    }
                    paragraph.Format.SpaceAfter = Unit.FromCentimeter(1);
                }
                else
                {
                    paragraph = cell.AddParagraph("Trucks: ");
                    paragraph.AddText(JoinTrucks(model.GetAllTrucks(), model.ShowDriverNamesOnPrintedOrder));
                    paragraph.Format.SpaceAfter = Unit.FromCentimeter(1);
                }

                cell = row.Cells[j++];
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


                table = document.LastSection.AddTable();
                table.Style = "Table";
                table.Borders.Width = Unit.FromPoint(1);
                var tm = new TextMeasurement(document.Styles["Table"].Font.Clone());

                //Line #
                table.AddColumn(Unit.FromCentimeter(0.8));
                //Item
                table.AddColumn(Unit.FromCentimeter(!model.HidePrices && model.SplitRateColumn ? 2.2 : 3.3));
                if (model.ShowTruckCategories)
                {
                    //Truck Categories
                    table.AddColumn(Unit.FromCentimeter(1.8));
                }
                //Designation
                table.AddColumn(Unit.FromCentimeter(1.8));
                //Quarry/Load At
                table.AddColumn(Unit.FromCentimeter(2.2));
                //Deliver To
                table.AddColumn(Unit.FromCentimeter(2.2));
                //Quantity
                table.AddColumn(Unit.FromCentimeter(2.5));
                if (!model.HidePrices)
                {
                    if (model.SplitRateColumn)
                    {
                        //Material Rate
                        table.AddColumn(Unit.FromCentimeter(1.4));
                        //Freight Rate
                        table.AddColumn(Unit.FromCentimeter(1.4));
                    }
                    else
                    {
                        //Rate
                        table.AddColumn(Unit.FromCentimeter(1.4));
                    }
                    //Total
                    table.AddColumn(Unit.FromCentimeter(2.2));
                }
                //Time on Job
                table.AddColumn(Unit.FromCentimeter(1.3));

                row = table.AddRow();
                row.Shading.Color = Colors.LightGray;
                row.Format.Font.Size = Unit.FromPoint(9);
                row.Format.Font.Bold = true;
                row.Format.Alignment = ParagraphAlignment.Center;
                row.Height = Unit.FromCentimeter(0.5);
                row.HeadingFormat = true;

                int i = 0;
                cell = row.Cells[i++];
                cell.AddParagraph("Line #");
                cell = row.Cells[i++];
                cell.AddParagraph("Item");
                if (model.ShowTruckCategories)
                {
                    cell = row.Cells[i++];
                    cell.AddParagraph("Truck Categories");
                }
                cell = row.Cells[i++];
                cell.AddParagraph("Designation");
                cell = row.Cells[i++];
                cell.AddParagraph("Load At");
                cell = row.Cells[i++];
                cell.AddParagraph("Deliver To");
                cell = row.Cells[i++];
                cell.AddParagraph("Quantity");
                if (!model.HidePrices)
                {
                    if (model.SplitRateColumn)
                    {
                        cell = row.Cells[i++];
                        cell.AddParagraph("Material Rate");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Freight Rate");
                    }
                    else
                    {
                        cell = row.Cells[i++];
                        cell.AddParagraph("Rate");
                    }
                    cell = row.Cells[i++];
                    cell.AddParagraph("Total");
                }
                cell = row.Cells[i++];
                cell.AddParagraph("Time on Job");

                if (model.Items.Any())
                {
                    foreach (var item in model.Items)
                    {
                        i = 0;
                        row = table.AddRow();
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.LineNumber.ToString(), tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.ServiceName, tm);
                        if (model.ShowTruckCategories)
                        {
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(string.Join(", ", item.OrderLineVehicleCategories), tm);
                        }
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.DesignationName, tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.LoadAtName, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.DeliverToName, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(model.UseActualAmount ? item.ActualQuantityFormatted : item.QuantityFormatted, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        if (!model.HidePrices)
                        {
                            if (model.SplitRateColumn)
                            {
                                cell = row.Cells[i++];
                                if (!item.IsMaterialTotalOverridden)
                                {
                                    paragraph = cell.AddParagraph(item.MaterialPricePerUnit?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)) ?? "", tm);
                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                }
                                cell = row.Cells[i++];
                                if (!item.IsFreightTotalOverridden)
                                {
                                    paragraph = cell.AddParagraph(item.FreightPricePerUnit?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)) ?? "", tm);
                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                }
                            }
                            else
                            {
                                cell = row.Cells[i++];
                                if (!item.IsMaterialTotalOverridden && !item.IsFreightTotalOverridden)
                                {
                                    paragraph = cell.AddParagraph(item.Rate?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)) ?? "", tm);
                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                }
                            }
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph((item.FreightPrice + item.MaterialPrice).ToString("C2", model.CurrencyCulture) ?? "", tm);
                            if (item.IsMaterialTotalOverridden || item.IsFreightTotalOverridden)
                            {
                                if (!model.UseReceipts)
                                {
                                    cell.Format.Shading.Color = Colors.MistyRose;
                                }
                            }
                            paragraph.Format.Alignment = ParagraphAlignment.Center;
                        }
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.TimeOnJob?.ConvertTimeZoneTo(model.TimeZone).ToString("t") ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        if (item.IsTimeStaggered)
                        {
                            var staggeredIcon = paragraph.AddImage(model.StaggeredTimeImagePath);
                            staggeredIcon.Height = Unit.FromCentimeter(0.5);
                            staggeredIcon.LockAspectRatio = true;
                        }

                        if (!string.IsNullOrEmpty(item.Note))
                        {
                            i = 0;
                            row = table.AddRow();
                            cell = row.Cells[i++];
                            cell.MergeRight = 3 + (model.HidePrices ? 0 : 2 + (model.SplitRateColumn ? 1 : 0) + (model.UseActualAmount ? 1 : 0))
                                + (model.ShowTruckCategories ? 1 : 0);
                            paragraph = cell.AddParagraph(item.Note, tm);
                        }
                    }
                }
                else
                {
                    table.AddRow();
                }

                table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);

                if (!model.HidePrices && model.Items.Any())
                {
                    row = table.AddRow();
                    cell = row.Cells[0];
                    cell.MergeRight = 6 + (model.SplitRateColumn ? 1 : 0) + (model.ShowTruckCategories ? 1 : 0);
                    cell.Borders.Visible = false;
                    cell.Borders.Left.Visible = true;
                    cell.Borders.Bottom.Visible = true;
                    paragraph = cell.AddParagraph("Total:", tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                    cell = row.Cells[7 + (model.SplitRateColumn ? 1 : 0) + (model.ShowTruckCategories ? 1 : 0)];
                    cell.Borders.Visible = false;
                    cell.Borders.Right.Visible = true;
                    cell.Borders.Bottom.Visible = true;
                    paragraph = cell.AddParagraph(model.Items.Sum(x => x.FreightPrice + x.MaterialPrice).ToString("C2", model.CurrencyCulture) ?? "", tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                }

                if (model.ShowDeliveryInfo && model.DeliveryInfoItems?.Any() == true)
                {
                    paragraph = document.LastSection.AddParagraph();
                    table = document.LastSection.AddTable();
                    table.Style = "Table";
                    table.Borders.Width = Unit.FromPoint(1);

                    //Truck number
                    table.AddColumn(Unit.FromCentimeter(2));
                    //Delivery time
                    table.AddColumn(Unit.FromCentimeter(2.3));
                    //Ticket number
                    table.AddColumn(Unit.FromCentimeter(3));
                    //Qty
                    table.AddColumn(Unit.FromCentimeter(1.5));
                    //UOM
                    table.AddColumn(Unit.FromCentimeter(1.7));
                    if (model.ShowSignatureColumn)
                    {
                        //Signature
                        table.AddColumn(Unit.FromCentimeter(5.5));
                    }
                    //19.2

                    row = table.AddRow();
                    row.Shading.Color = Colors.LightGray;
                    row.Format.Font.Size = Unit.FromPoint(9);
                    row.Format.Font.Bold = true;
                    row.Format.Alignment = ParagraphAlignment.Center;
                    row.Height = Unit.FromCentimeter(0.5);
                    row.HeadingFormat = true;

                    i = 0;
                    cell = row.Cells[i++];
                    cell.AddParagraph("Truck number");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Delivery time");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Ticket number");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Qty");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Unit");
                    if (model.ShowSignatureColumn)
                    {
                        cell = row.Cells[i++];
                        cell.AddParagraph("Signature");
                    }

                    foreach (var item in model.DeliveryInfoItems
                                              .OrderBy(x => x.Load?.DeliveryTime)
                                              .ThenBy(x => x.DriverName)
                                              .ToList())
                    {
                        i = 0;
                        row = table.AddRow();
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.TruckNumber, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.Load?.DeliveryTime?.ConvertTimeZoneTo(model.TimeZone).ToString("t") ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.TicketNumber, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.Quantity?.ToString(Utilities.NumberFormatWithoutRounding) ?? "", tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.UomName, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        if (model.ShowSignatureColumn)
                        {
                            cell = row.Cells[i++];
                            if (!string.IsNullOrEmpty(item.Load?.Signature))
                            {
                                var logo = cell.AddImage(item.Load?.Signature);
                                //logo.Height = Unit.FromCentimeter(3.2);
                                logo.Width = Unit.FromCentimeter(4);
                                logo.LockAspectRatio = true;
                                cell.Format.Alignment = ParagraphAlignment.Center;
                                paragraph = cell.AddParagraph(item.Load?.SignatureName, tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;
                            }
                        }
                    }

                    table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
                }

                if (model.IncludeTickets && model.DeliveryInfoItems?.Any() == true)
                {
                    foreach (var ticket in model.DeliveryInfoItems)
                    {
                        if (string.IsNullOrEmpty(ticket.TicketPhoto))
                        {
                            continue;
                        }
                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);
                        //var image = document.LastSection.AddImage(ticket.TicketPhoto);
                        var image = paragraph.AddImage(ticket.TicketPhoto);
                        //image.Width = Unit.FromInch(8.5) - Unit.FromCentimeter(3); //full width
                        image.Width = Unit.FromCentimeter(13);
                        image.LockAspectRatio = true;
                    }
                }

                paragraph = document.LastSection.AddParagraph("Comments: ");
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
                paragraph = document.LastSection.AddParagraph(model.Directions ?? "");

                if (!taxWarning.IsNullOrEmpty())
                {
                    paragraph = document.LastSection.AddParagraph(taxWarningAsterisks + taxWarning);
                    paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
                }
            }

            return document;
        }

        private static string JoinTrucks(IEnumerable<TruckDriverDto> trucks, bool showDriverNamesOnPrintedOrder)
        {
            return showDriverNamesOnPrintedOrder
                ? string.Join(", ", trucks)
                : string.Join(", ", trucks.Select(x => x.TruckCode));
        }
    }
}
