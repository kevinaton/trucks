﻿using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherWeb.Imports.RowReaders
{
    public class CustomerImportRow : ImportRow
    {
        public CustomerImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public bool IsActive => GetString(CustomerColumn.IsActive, 30) == "Active" || !HasField(CustomerColumn.IsActive);
        public string Name => GetString(CustomerColumn.Name, EntityStringFieldLengths.Customer.Name);
        public string AccountNumber => GetString(CustomerColumn.AccountNumber, EntityStringFieldLengths.Customer.AccountNumber);
        public string InvoiceEmail => GetString(CustomerColumn.Email, EntityStringFieldLengths.General.Email);
        public string Terms => GetString(CustomerColumn.Terms, 100);

        public string Address1 => GetString(CustomerColumn.Address1, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string Address2 => GetString(CustomerColumn.Address2, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string City => GetString(CustomerColumn.City, EntityStringFieldLengths.GeneralAddress.MaxCityLength);
        public string State => GetString(CustomerColumn.State, EntityStringFieldLengths.GeneralAddress.MaxStateLength);
        public string ZipCode => GetString(CustomerColumn.ZipCode, EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength);
        public string CountryCode => GetString(CustomerColumn.CountryCode, EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength);

        public string BillingAddress1 => GetString(CustomerColumn.BillingAddress1, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string BillingAddress2 => GetString(CustomerColumn.BillingAddress2, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string BillingCity => GetString(CustomerColumn.BillingCity, EntityStringFieldLengths.GeneralAddress.MaxCityLength);
        public string BillingState => GetString(CustomerColumn.BillingState, EntityStringFieldLengths.GeneralAddress.MaxStateLength);
        public string BillingZipCode => GetString(CustomerColumn.BillingZipCode, EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength);
        public string BillingCountryCode => GetString(CustomerColumn.BillingCountryCode, EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength);


        public string ContactTitle => GetString(CustomerColumn.ContactTitle, EntityStringFieldLengths.CustomerContact.Title);
        public string ContactName => GetString(CustomerColumn.ContactName, EntityStringFieldLengths.CustomerContact.Name);
        public string ContactPhone => GetString(CustomerColumn.ContactPhone, EntityStringFieldLengths.General.PhoneNumber);
        public string ContactFax => GetString(CustomerColumn.ContactFax, EntityStringFieldLengths.General.PhoneNumber);
        public string Contact2Name => GetString(CustomerColumn.Contact2Name, EntityStringFieldLengths.CustomerContact.Name);
        public string Contact2Phone => GetString(CustomerColumn.Contact2Phone, EntityStringFieldLengths.General.PhoneNumber);
    }
}
