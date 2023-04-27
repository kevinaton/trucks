using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using DispatcherWeb.SyncRequests.Entities;

namespace DispatcherWeb
{
    public static class Utilities
    {
        //public const string CurrencyFormatWithoutRounding = "$0.#########";
        public const string NumberFormatWithoutRounding = "0.#########";

        public static string GetCurrencyFormatWithoutRounding(CultureInfo currencyCulture)
        {
            return GetCurrencyFormatWithoutRounding(currencyCulture.NumberFormat.CurrencySymbol);
        }

        public static string GetCurrencyFormatWithoutRounding(string currencySymbol)
        {
            return currencySymbol + NumberFormatWithoutRounding;
        }

        public static string FormatFullName(String first, String last, String middle = null)
        {
            return string.IsNullOrEmpty(middle)
                ? first + " " + last
                : first + " " + middle + " " + last;
        }

        public static (string FirstName, string LastName) SplitFullName(string fullName)
        {
            if (fullName == null)
            {
                return (null, null);
            }

            if (fullName == "")
            {
                return ("", "");
            }

            if (fullName.Contains(","))
            {
                var parts = fullName.Split(',');
                var lastName = parts[0].Trim();
                var firstName = string.Join(",", parts.Skip(1)).Trim();
                return (firstName, lastName);
            }
            else
            {
                var parts = fullName.Split(" ");
                var firstName = parts[0].Trim();
                var lastName = string.Join(" ", parts.Skip(1)).Trim();
                return (firstName, lastName);
            }
        }

        public static string ConcatenateAddress(params string[] parts)
        {
            var joins = parts.ToList();
            joins.RemoveAll(string.IsNullOrWhiteSpace);
            return string.Join(", ", joins);
        }

        public static string FormatAddress(string address1, string city, string state, string zipCode, string countryCode) =>
            ConcatenateAddress(address1, city, state, zipCode, countryCode);

        public static string FormatAddress2(string address1, string address2, string city, string state, string zipCode)
        {
            //$"{BillingAddress1}{BillingAddress2}\n{BillingCity}, {BillingState} {BillingZipCode}";
            address1 = address1.IsNullOrWhiteSpace() ? null : address1 + "\n";
            address2 = address2.IsNullOrWhiteSpace() ? null : address2 + "\n";
            city = city.IsNullOrWhiteSpace() ? null : city + ", ";
            state = state.IsNullOrWhiteSpace() ? null : state + ", ";
            zipCode = zipCode.IsNullOrWhiteSpace() ? null : zipCode;
            return address1 + address2 + city + state + zipCode;
        }

        public static string RemoveInvalidFileNameChars(string filename)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (filename.Contains(invalidChar))
                {
                    filename = filename.Replace(invalidChar.ToString(), "");
                }
            }
            return filename;
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }
            var result = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.GetName();
            if (result != null)
            {
                return result;
            }
            result = enumValue.ToString();
            if (result == "0")
            {
                return string.Empty;
            }
            return result;
        }

        public static bool TryGetEnumFromDisplayName<T>(string displayName, out T enumValue) where T : Enum
        {
            enumValue = default;
            if (string.IsNullOrEmpty(displayName))
            {
                return false;
            }

            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();

            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DisplayAttribute)) is DisplayAttribute displayAttribute)
                {
                    if (displayAttribute.Name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        enumValue = (T)field.GetValue(null);
                        return true;
                    }
                }
                else
                {
                    if (field.Name == displayName)
                    {
                        enumValue = (T)field.GetValue(null);
                        return true;
                    }
                }
            }

            return false;
        }

        private static readonly Regex TemplateVariableRegex = new Regex(@"{(\w+)}", RegexOptions.Compiled);
        public static string ReplaceTemplateVariables(string templateString, Dictionary<string, string> varReplacements)
        {
            string result = TemplateVariableRegex.Replace(templateString, match => varReplacements.ContainsKey(match.Groups[1].Value) ? varReplacements[match.Groups[1].Value] : match.Groups[1].Value);
            return result;
        }

        public static bool IsImageFileType(FileType fileType) => fileType == FileType.Bmp || fileType == FileType.Jpg || fileType == FileType.Gif || fileType == FileType.Png;

        public static string GetFontAwesomeClass(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Doc:
                    return "fa-file-word-o";
                case FileType.Pdf:
                    return "fa-file-pdf-o";
                default:
                    return "fa-file-o";
            }
        }


        public static int GetOrder(this EmailDeliveryStatus status)
        {
            switch (status)
            {
                case EmailDeliveryStatus.NotProcessed: return 0;
                case EmailDeliveryStatus.Processed: return 1;
                case EmailDeliveryStatus.Dropped: return 2;
                case EmailDeliveryStatus.Deferred: return 3;
                case EmailDeliveryStatus.Bounced: return 4;
                case EmailDeliveryStatus.Delivered: return 5;
                case EmailDeliveryStatus.Opened: return 6;
                default: return 0;
            }
        }

        public static bool IsFailed(this EmailDeliveryStatus status)
        {
            switch (status)
            {
                case EmailDeliveryStatus.Bounced:
                case EmailDeliveryStatus.Dropped:
                    return true;
                default:
                    return false;
            }
        }

        public static EmailDeliveryStatus? GetLowestStatus(this IEnumerable<EmailDeliveryStatus> statuses)
        {
            var list = statuses.ToList();

            if (!list.Any())
            {
                return null; //EmailDeliveryStatus.NotProcessed;
            }

            if (list.Count == 1)
            {
                return list.First();
            }

            var lowestDeliveryStatus = list.First();
            foreach (var status in list)
            {
                if (status.GetOrder()
                    < lowestDeliveryStatus.GetOrder())
                {
                    lowestDeliveryStatus = status;
                }
            }

            return lowestDeliveryStatus;
        }

        public static string GetDomainFromUrl(string url)
        {
            Uri rootUri = new Uri(url);
            return rootUri.Host;
        }

        public static bool HasMaterial(this DesignationEnum val)
        {
            switch (val)
            {
                case DesignationEnum.MaterialOnly:
                case DesignationEnum.FreightAndMaterial:
                case DesignationEnum.BackhaulFreightAndMaterial:
                case DesignationEnum.Disposal:
                case DesignationEnum.BackHaulFreightAndDisposal:
                case DesignationEnum.StraightHaulFreightAndDisposal:
                    return true;
            }
            return false;
        }

        public static bool MaterialOnly(this DesignationEnum val)
        {
            switch (val)
            {
                case DesignationEnum.MaterialOnly:
                    return true;
            }
            return false;
        }

        public static bool FreightOnly(this DesignationEnum val)
        {
            switch (val)
            {
                case DesignationEnum.FreightOnly:
                case DesignationEnum.BackhaulFreightOnly:
                    return true;
            }
            return false;
        }

        public static bool FreightAndMaterial(this DesignationEnum val)
        {
            switch (val)
            {
                case DesignationEnum.FreightAndMaterial:
                case DesignationEnum.BackhaulFreightAndMaterial:
                case DesignationEnum.Disposal:
                case DesignationEnum.BackHaulFreightAndDisposal:
                case DesignationEnum.StraightHaulFreightAndDisposal:
                    return true;
            }
            return false;
        }

        public static T SetLastUpdateDateTime<T, TKey>(this T model, FullAuditedEntity<TKey> entity) where T : ChangedDriverAppEntity<TKey>
        {
            model.LastUpdateDateTime = entity.LastModificationTime.HasValue && entity.LastModificationTime.Value > entity.CreationTime ? entity.LastModificationTime.Value : entity.CreationTime;
            return model;
        }
    }
}
