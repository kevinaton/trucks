﻿using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Drivers.Dto
{
    public class DriverEditDto : IOfficeIdNameDto
    {
        public int? Id { get; set; }

        public long? UserId { get; set; }

        [Required(ErrorMessage = "First Name is a required field")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is a required field")]
        [StringLength(50)]
        public string LastName { get; set; }

        public string FullName => FirstName + " " + LastName;

        [Required(ErrorMessage = "Office is a required field")]
        public int? OfficeId { get; set; }

        public bool IsInactive { get; set; }

        public string OfficeName { get; set; }

        [StringLength(256)]
        public string EmailAddress { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string CellPhoneNumber { get; set; }

        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string Address { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        public List<EmployeeTimeClassificationEditDto> EmployeeTimeClassifications { get; set; }

        [StringLength(20)]
        public string LicenseNumber { get; set; }

        [StringLength(50)]
        public string TypeOfLicense { get; set; }

        public DateTime? LicenseExpirationDate { get; set; }

        public DateTime? LastPhysicalDate { get; set; }

        public DateTime? NextPhysicalDueDate { get; set; }

        public DateTime? LastMvrDate { get; set; }

        public DateTime? NextMvrDueDate { get; set; }

        public DateTime? DateOfHire { get; set; }

        public bool IsSingleOffice { get; set; }
        int IOfficeIdNameDto.OfficeId { get => OfficeId ?? 0; set => OfficeId = value; }
    }
}
