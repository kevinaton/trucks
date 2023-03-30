using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abp.AspNetZeroCore.Net;
using Abp.Collections.Extensions;
using Abp.Dependency;
using CsvHelper;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Storage;

namespace DispatcherWeb.DataExporting.Csv
{
    public class CsvExporterBase : DispatcherWebServiceBase, ITransientDependency
    {
        private CsvWriter _csv;
        private readonly ITempFileCacheManager _tempFileCacheManager;

        protected CsvExporterBase(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        protected FileDto CreateCsvFile(string fileName, Action creator)
        {
            var file = new FileDto(fileName.SanitizeFilename(), MimeTypeNames.TextCsv);
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (_csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
            {
                creator();
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                _tempFileCacheManager.SetFile(file.FileToken, stream.ToArray());
            }

            return file;

        }

        protected void AddHeaderAndData<T>(IList<T> items, params (string header, Func<T, string> dataSelector)[] headerAndDataPairs)
        {
            AddHeader(headerAndDataPairs
                .Where(x => x.header != null)
                .Select(x => x.header)
                .ToArray());

            AddObjects(items, headerAndDataPairs
                .Where(x => x.header != null && x.dataSelector != null)
                .Select(x => x.dataSelector)
                .ToArray());
        }

        protected void AddHeader(params string[] headerTexts)
        {
            if (headerTexts.IsNullOrEmpty())
            {
                return;
            }

            foreach (var headerText in headerTexts)
            {
                if (headerText == null)
                {
                    continue;
                }
                AddHeader(SimplifyUtf(headerText));
            }
            _csv.NextRecord();
        }

        protected void AddHeader(string headerText)
        {
            _csv.WriteField(headerText);
        }

        protected void AddObjects<T>(IList<T> items, params Func<T, string>[] propertySelectors)
        {
            if (items.IsNullOrEmpty() || propertySelectors.IsNullOrEmpty())
            {
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                for (var j = 0; j < propertySelectors.Length; j++)
                {
                    var propertySelector = propertySelectors[j];
                    if (propertySelector == null)
                    {
                        continue;
                    }
                    _csv.WriteField(SimplifyUtf(propertySelector(items[i])) ?? "");
                }
                _csv.NextRecord();
            }
        }

        private string SimplifyUtf(string val)
        {
            return val?
                .Replace("–", "-") //en dash
                .Replace("—", "-") //em dash too just in case
                ;
        }
    }
}
