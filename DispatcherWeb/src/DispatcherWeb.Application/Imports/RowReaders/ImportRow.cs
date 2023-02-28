using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvHelper;
using DispatcherWeb.Infrastructure.Extensions;

namespace DispatcherWeb.Imports.RowReaders
{
    public class ImportRow : IImportRow
    {
        protected readonly CsvReader _csv;
        private readonly ILookup<string, string> _fieldMap;
        private static readonly CultureInfo CultureInfo = CultureInfo.CreateSpecificCulture("en-US");

        public ImportRow(CsvReader csv, ILookup<string, string> fieldMap)
        {
            _csv = csv;
            _fieldMap = fieldMap;
        }

        public Dictionary<string, (string value, Type type)> ParseErrors { get; } = new Dictionary<string, (string value, Type type)>();
        public Dictionary<string, Tuple<string, int>> StringExceedErrors { get; } = new Dictionary<string, Tuple<string, int>>();


        protected T GetField<T>(string fieldName)
        {
            if (_fieldMap != null)
            {
                if (!_fieldMap.Contains(fieldName))
                {
                    return default(T);
                }
                return _csv.GetField<T>(_fieldMap[fieldName].First());
            }

            if (!_csv.TryGetField<T>(fieldName, out T field))
            {
                return default(T);
            }
            return field;
        }

        protected bool HasField(string fieldName)
        {
            if (_fieldMap != null)
            {
                return _fieldMap.Contains(fieldName);
            }

            return _csv.TryGetField(fieldName, out string _);
        }

        protected string GetString(string fieldName, int maxLength)
        {
            string resultString = GetField<string>(fieldName);
            if (resultString?.Length > maxLength)
            {
                AddStringExceedErrorIfNotExist(fieldName, resultString, maxLength);
                return resultString.Substring(0, maxLength);
            }
            return resultString;
        }

        protected DateTime? GetDate(string fieldName, bool required = false)
        {
            string dateString = GetField<string>(fieldName)?.Trim();
            if (string.IsNullOrEmpty(dateString))
            {
                if (required)
                {
                    AddParseErrorIfNotExist(fieldName, dateString, typeof(DateTime));
                }
                return null;
            }

            string[] formats = {
                "M/d/yyyy h:mm:ss tt",
                "M/d/yyyy h:mm tt",
                "MM/dd/yyyy HH:mm:ss",
                "M/d/yyyy H:mm:ss",
                "M/d/yyyy hh:mm tt",
                "M/d/yyyy hh tt",
                "M/d/yyyy H:mm",
                "MM/dd/yyyy HH:mm",
                "M/dd/yyyy HH:mm",
                "MM/dd/yyyy",
                "M/dd/yyyy",
                "M/d/yyyy",
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "M/d/yy h:mm tt",
                "M/d/yy H:mm",
                "M/d/yy HH:mm"
            };
            if (!DateTime.TryParseExact(dateString, formats, CultureInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out var result))
            {
                AddParseErrorIfNotExist(fieldName, dateString, typeof(DateTime));
                return null;
            }
            if (result < new DateTime(1753, 1, 1))
            {
                AddParseErrorIfNotExist(fieldName, dateString, typeof(DateTime));
                return null;
            }
            return result;
        }

        protected DateTime? GetDateTimeWithTimeZone(string fieldName, bool required = false)
        {
            string dateString = GetField<string>(fieldName)?.Trim();
            if (string.IsNullOrEmpty(dateString))
            {
                if (required)
                {
                    AddParseErrorIfNotExist(fieldName, dateString, typeof(DateTime));
                }
                return null;
            }

            var dateStringWithOffset = dateString.ReplaceTimeZoneAbbreviationWithOffset();

            string[] formats = {
                "M/d/yyyy h:mm:ss tt zzz",
                "M/d/yyyy h:mm tt zzz",
                "MM/dd/yyyy hh:mm:ss zzz",
                "M/d/yyyy h:mm:ss zzz",
                "M/d/yyyy hh:mm tt zzz",
                "M/d/yyyy hh tt zzz",
                "M/d/yyyy h:mm zzz",
                "M/d/yyyy h:mm zzz",
                "MM/dd/yyyy hh:mm zzz",
                "M/dd/yyyy hh:mm zzz",
                "yyyy-MM-dd HH:mm:ss zzz",
            };
            if (!DateTime.TryParseExact(dateStringWithOffset, formats, CultureInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out var result))
            {
                AddParseErrorIfNotExist(fieldName, dateString, typeof(DateTime));
                return null;
            }
            if (result < new DateTime(1753, 1, 1))
            {
                AddParseErrorIfNotExist(fieldName, dateString, typeof(DateTime));
                return null;
            }
            return result;
        }

        protected decimal? GetDecimal(string fieldName, int decimals) =>
            GetDecimal(fieldName)?.RoundTo(decimals);

        protected decimal? GetDecimal(string fieldName, bool required = false)
        {
            string decimalString = GetField<string>(fieldName)?.Trim();
            if (String.IsNullOrEmpty(decimalString))
            {
                if (required)
                {
                    AddParseErrorIfNotExist(fieldName, decimalString, typeof(decimal));
                }
                return null;
            }

            if (!decimal.TryParse(
                decimalString,
                NumberStyles.AllowCurrencySymbol | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                CultureInfo,
                out var result
            ))
            {
                AddParseErrorIfNotExist(fieldName, decimalString, typeof(decimal));
                return null;
            }
            return result;
        }

        protected int? GetInt(string fieldName, bool required = false)
        {
            string intString = GetField<string>(fieldName)?.Trim();
            if (string.IsNullOrEmpty(intString))
            {
                if (required)
                {
                    AddParseErrorIfNotExist(fieldName, intString, typeof(int));
                }
                return null;
            }

            if (!int.TryParse(
                intString,
                NumberStyles.Number,
                CultureInfo,
                out var result
            ))
            {
                AddParseErrorIfNotExist(fieldName, intString, typeof(int));
                return null;
            }
            return result;
        }

        public void AddParseErrorIfNotExist(string fieldName, string valueString, Type type)
        {
            if (!ParseErrors.ContainsKey(fieldName))
            {
                ParseErrors.Add(fieldName, (valueString, type));
            }
        }

        private void AddStringExceedErrorIfNotExist(string fieldName, string valueString, int maxLength)
        {
            if (!StringExceedErrors.ContainsKey(fieldName))
            {
                StringExceedErrors.Add(fieldName, new Tuple<string, int>(valueString, maxLength));
            }
        }


    }
}
