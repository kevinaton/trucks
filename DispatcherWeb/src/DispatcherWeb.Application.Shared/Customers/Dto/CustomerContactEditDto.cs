using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Customers.Dto
{
    public class CustomerContactEditDto
    {
        public int? Id { get; set; }

        public int CustomerId { get; set; }

        [StringLength(EntityStringFieldLengths.CustomerContact.Name)]
        [Obsolete]
        public string Name { get; set; }

        [RegularExpression("^[a-zA-Z]+(\\s+[a-zA-Z]+)*$", ErrorMessage = "This isn't a valid first name. Only characters and spaces are allowed.")]
        [Required(ErrorMessage = "First Name is a required field")]
        [StringLength(EntityStringFieldLengths.CustomerContact.FirstName)]
        public string FirstName { get; set; }


        [RegularExpression("^[a-zA-Z]+(\\s+[a-zA-Z]+)*$", ErrorMessage = "This isn't a valid last name. Only characters and spaces are allowed.")]
        [Required(ErrorMessage = "Last Name is a required field")]
        [StringLength(EntityStringFieldLengths.CustomerContact.LastName)]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string Fax { get; set; }

        [StringLength(EntityStringFieldLengths.General.Email)]
        public string Email { get; set; }

        [StringLength(40)]
        public string Title { get; set; }

        public bool IsActive { get; set; }

        public bool HasCustomerPortalAccess { get; set; }
    }
}
