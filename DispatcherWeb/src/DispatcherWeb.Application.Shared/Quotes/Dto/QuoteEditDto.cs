using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Quotes.Dto
{
    public class QuoteEditDto
    {
        public int? Id { get; set; }

        public int? ProjectId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int? ContactId { get; set; }

        public string ContactName { get; set; }

        public string ProjectName { get; set; }

        [Required]
        [StringLength(155)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime? ProposalDate { get; set; }

        public DateTime? ProposalExpiryDate { get; set; }

        public DateTime? InactivationDate { get; set; }

        public ProjectStatus Status { get; set; }

        [StringLength(20)]
        public string PONumber { get; set; }

        [StringLength(20)]
        public string SpectrumNumber { get; set; }

        //[Required(ErrorMessage = "Base Fuel Cost is a required field")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal? BaseFuelCost { get; set; }

        public int? FuelSurchargeCalculationId { get; set; }

        public string FuelSurchargeCalculationName { get; set; }

        public bool? CanChangeBaseFuelCost { get; set; }

        public string Directions { get; set; }

        public string Notes { get; set; }

        public long? SalesPersonId { get; set; }

        public string SalesPersonName { get; set; }

        [StringLength(500)]
        public string ChargeTo { get; set; }

        public bool HasOrders { get; set; }
    }
}
