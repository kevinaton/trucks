using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Tickets.Dto;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;

namespace DispatcherWeb.Tickets.Reports
{
    public class TicketPrintOutGenerator : ITransientDependency
    {
        public TicketPrintOutGenerator() {
        }

        public Task<Document> GenerateReport(List<TicketPrintOutDto> modelList)
        {
            Document document = new Document();

            Section section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            //section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.PageHeight = Unit.FromInch(11); //27.94cm
            section.PageSetup.PageWidth = Unit.FromInch(8.5); //21.59cm -3cm margin = 18.6cm total
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.6);

            Style style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Times New Roman";
            style.Font.Size = Unit.FromPoint(11);
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            var headerStyle = document.Styles[StyleNames.Header];
            headerStyle.Font.Name = "Times New Roman";
            headerStyle.Font.Size = Unit.FromPoint(10);
            
            Paragraph paragraph;

            var firstTicket = true;

            foreach (var model in modelList)
            {
                if (firstTicket)
                {
                    firstTicket = false;
                }
                else
                {
                    section.AddPageBreak();
                }

                paragraph = document.LastSection.AddParagraph(model.LegalName ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(14);
                paragraph.Format.Font.Bold = true;

                paragraph = document.LastSection.AddParagraph(model.LegalAddress ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(10);

                paragraph = document.LastSection.AddParagraph(model.BillingPhoneNumber ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(10);

                if (model.LogoPath != null)
                {
                    var logo = section.AddImage(model.LogoPath);
                    logo.Height = Unit.FromCentimeter(3.2);
                    //logo.Width = Unit.FromCentimeter(4.5);
                    logo.LockAspectRatio = true;
                    logo.RelativeVertical = RelativeVertical.Page;
                    logo.RelativeHorizontal = RelativeHorizontal.Page;
                    logo.WrapFormat.Style = WrapStyle.Through;
                    logo.WrapFormat.DistanceLeft = Unit.FromCentimeter(15.5);
                    logo.WrapFormat.DistanceTop = Unit.FromCentimeter(1.2);
                }

                paragraph.Format.SpaceAfter = Unit.FromCentimeter(2.5);

                var secondColumnMargin = Unit.FromCentimeter(4);

                paragraph = document.LastSection.AddParagraph("Ticket Number: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.TicketNumber ?? "");

                paragraph = document.LastSection.AddParagraph("Date Time: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.TicketDateTime?.ToShortDateString() ?? "");

                paragraph = document.LastSection.AddParagraph("Sold To: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.CustomerName ?? "");

                paragraph = document.LastSection.AddParagraph("Product: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.ServiceName ?? "");

                paragraph = document.LastSection.AddParagraph("Quantity: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.MaterialQuantity + " " + model.MaterialUomName ?? "");

                paragraph = document.LastSection.AddParagraph("Note: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.Note ?? "");
            }

            return Task.FromResult(document);
        }
    }
}
