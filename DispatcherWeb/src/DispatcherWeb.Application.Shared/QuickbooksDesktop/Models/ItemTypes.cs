namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public static class ItemTypes
    {
        public const string SalesTax = "COMPTAX";
        public const string Discount = "DISC";

        /// <summary>
        /// Group item (groups several invoice items into a single item)
        /// </summary>
        public const string Group = "GRP";

        public const string InventoryPart = "INVENTORY";
        public const string OtherCharge = "OTHC";
        public const string NonInventoryPart = "PART";
        public const string Payment = "PMT";
        public const string Service = "SERV";
        public const string SalesTaxGroup = "STAX";
        public const string Subtotal = "SUBT";
        public const string InventoryAssembly = "ASSEMBLY";

        public static string FromServiceType(ServiceType? serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.Discount:
                    return Discount;
                case ServiceType.InventoryAssembly:
                    return InventoryAssembly;
                case ServiceType.InventoryPart:
                    return InventoryPart;
                case ServiceType.NonInventoryPart:
                    return NonInventoryPart;
                case ServiceType.OtherCharge:
                    return OtherCharge;
                case ServiceType.Payment:
                    return Payment;
                case ServiceType.SalesTaxItem:
                    return SalesTax;
                case ServiceType.Service:
                    return Service;
                default:
                case ServiceType.System:
                    return null;
            }
        }
    }
}
