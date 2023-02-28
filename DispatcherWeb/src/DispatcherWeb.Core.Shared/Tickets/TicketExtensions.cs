namespace DispatcherWeb.Tickets
{
    public static class TicketExtensions
    {
        public static decimal? GetFreightQuantity(this ITicketQuantity ticket)
        {
            var (materialQuantity, freightQuantity) = GetMaterialAndFreightQuantity(ticket);
            return freightQuantity;
        }

        public static decimal? GetMaterialQuantity(this ITicketQuantity ticket)
        {
            var (materialQuantity, freightQuantity) = GetMaterialAndFreightQuantity(ticket);
            return materialQuantity;
        }

        public static (decimal? materialQuantity, decimal? freightQuantity) GetMaterialAndFreightQuantity(this ITicketQuantity ticket)
        {
            if (ticket.Designation == DesignationEnum.MaterialOnly)
            {
                return (ticket.Quantity, null); //quantity is material
            }
            if (ticket.Designation == DesignationEnum.FreightAndMaterial)
            {
                if (ticket.MaterialUomId == ticket.FreightUomId)
                {
                    return (ticket.Quantity, ticket.Quantity); //quantity is material and freight
                }

                if (ticket.TicketUomId == ticket.MaterialUomId)
                {
                    return (ticket.Quantity, null); //quantity is material
                }

                if (ticket.TicketUomId == ticket.FreightUomId)
                {
                    return (null, ticket.Quantity); //quantity is freight
                }

                //fallback to the previous version of logic:
                return (ticket.Quantity, null); //quantity is material
            }
            return (null, ticket.Quantity); //quantity is freight
        }

        public static (bool useMaterial, bool useFreight) GetAmountTypeToUse(this ITicketQuantity ticket)
        {
            if (ticket.Designation == DesignationEnum.MaterialOnly)
            {
                return (true, false); //quantity is material
            }
            if (ticket.Designation == DesignationEnum.FreightAndMaterial)
            {
                if (ticket.MaterialUomId == ticket.FreightUomId)
                {
                    return (true, true); //quantity is material and freight
                }

                if (ticket.TicketUomId == ticket.MaterialUomId)
                {
                    return (true, false); //quantity is material
                }

                if (ticket.TicketUomId == ticket.FreightUomId)
                {
                    return (false, true); //quantity is freight
                }

                return (true, false); //quantity is material
            }
            return (false, true); //quantity is freight
        }
    }
}
