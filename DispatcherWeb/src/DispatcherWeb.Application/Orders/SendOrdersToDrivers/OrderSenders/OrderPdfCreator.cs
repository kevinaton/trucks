using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Orders.SendOrdersToDrivers.Dto;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace DispatcherWeb.Orders.SendOrdersToDrivers.OrderSenders
{
    public class OrderPdfCreator
	{
        public static void CreatePdf(DriverOrderDto driverOrder, MemoryStream stream)
		{
			Document document = new Document();

			Section section = document.AddSection();
			section.PageSetup = document.DefaultPageSetup.Clone();
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

			section.AddParagraph($"Following are your orders for {driverOrder.DeliveryDate:d}.");

			int orderNumber = 1;
			foreach (var order in driverOrder.Orders)
			{
				section.AddParagraph($"Order {orderNumber} {(order.OrderTime.HasValue ? "at " + order.OrderTime.Value.ToString("F") : "")}.");
				section.AddParagraph("Items:");
				foreach (var orderLine in order.OrderLines)
				{
					section.AddParagraph($"{orderLine.MaterialQuantity} {orderLine.MaterialUomName} of {orderLine.Item} from {orderLine.LoadAtName} to {orderLine.DeliverToName}");
				}

				orderNumber++;
			}

		    document.SaveToMemoryStream(stream);
		}
	}
}
