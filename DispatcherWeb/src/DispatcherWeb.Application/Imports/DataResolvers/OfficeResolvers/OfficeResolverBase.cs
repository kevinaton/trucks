using System;
using System.Collections.Generic;
using Castle.Core.Internal;

namespace DispatcherWeb.Imports.DataResolvers.OfficeResolvers
{
    public abstract class OfficeResolverBase
    {
        protected Dictionary<string, int> _officeStringValueIdDictionary;

        public int? GetOfficeId(string officeStringValue)
        {
            if (officeStringValue.IsNullOrEmpty())
            {
                throw new ArgumentException($"The {nameof(officeStringValue)} is null or empty!");
            }
            if (_officeStringValueIdDictionary == null)
            {
                _officeStringValueIdDictionary = GetOfficeStringValueIdDictionary();
            }

            if (_officeStringValueIdDictionary.ContainsKey(officeStringValue.ToLowerInvariant()))
            {
                return _officeStringValueIdDictionary[officeStringValue.ToLowerInvariant()];
            }

            return null;

        }

        protected abstract Dictionary<string, int> GetOfficeStringValueIdDictionary();
    }
}