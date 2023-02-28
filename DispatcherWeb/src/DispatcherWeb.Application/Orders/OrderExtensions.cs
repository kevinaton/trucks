namespace DispatcherWeb.Orders
{
    public static class OrderExtensions
    {
        public static Order CreateCopy(this Order order)
        {
            return new Order
            {
                CODTotal = order.CODTotal,
                ContactId = order.ContactId,
                ChargeTo = order.ChargeTo,
                CustomerId = order.CustomerId,
                DeliveryDate = order.DeliveryDate,
                Shift = order.Shift,
                IsPending = order.IsPending,
                Directions = order.Directions,
                FreightTotal = order.FreightTotal,
                IsClosed = order.IsClosed,
                LocationId = order.LocationId,
                MaterialTotal = order.MaterialTotal,
                PONumber = order.PONumber,
                SpectrumNumber = order.SpectrumNumber,
                ProjectId = order.ProjectId,
                QuoteId = order.QuoteId,
                SalesTax = order.SalesTax,
                SalesTaxRate = order.SalesTaxRate,
                Priority = order.Priority,
                EncryptedInternalNotes = order.EncryptedInternalNotes,
                HasInternalNotes = order.HasInternalNotes,
                FuelSurchargeCalculationId = order.FuelSurchargeCalculationId,
                BaseFuelCost = order.BaseFuelCost
            };

        }
    }
}
