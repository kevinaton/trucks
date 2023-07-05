﻿using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Customers.Dto
{
    public class CustomerContactEditDto
    {
        public int? Id { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "LastName is a required field")]
        [StringLength(30)]
        public string LastName { get; set; }


        [Required(ErrorMessage = "FirstName is a required field")]
        [StringLength(30)]
        public string FirstName { get; set; }

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
