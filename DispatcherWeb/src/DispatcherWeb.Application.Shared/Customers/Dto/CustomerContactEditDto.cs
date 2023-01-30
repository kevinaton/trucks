using DispatcherWeb.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Customers.Dto
{
    public class CustomerContactEditDto
    {
        public int? Id { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is a required field")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string Fax { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string Email { get; set; }

        [StringLength(40)]
        public string Title { get; set; }

        public bool IsActive { get; set; }
    }
}
