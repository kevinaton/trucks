using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Validation;
using Abp.Timing;
using DispatcherWeb.Identity;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure.BackgroundJobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DispatcherWeb.Imports.Services
{
    public abstract class ImportDataBaseAppService<T> : DispatcherWebAppServiceBase, IImportDataBaseAppService where T : IImportRow
    {
        protected readonly ImportResultDto _result = new ImportResultDto();
        protected string _timeZone;
        protected ImportJobArgs _importJobArgs;
        protected int _tenantId;
        protected long _userId;

        protected ImportDataBaseAppService()
        {
        }

        [UnitOfWork(isTransactional: false)]
        [DisableValidation]
        [RemoteService(false)]
        public ImportResultDto Import(TextReader textReader, ImportJobArgs args)
        {
            _importJobArgs = args;
            _tenantId = args.RequestorUser.GetTenantId();
            _userId = args.RequestorUser.UserId;

            SetImportParametersForLog(_tenantId);
            LogInfo("Import started");

            using (Session.Use(_tenantId, _userId))
            using (CurrentUnitOfWork.SetTenantId(_tenantId))
            {
                _timeZone = SettingManager.GetSettingValue(TimingSettingNames.TimeZone);

                IImportReader reader = new ImportReader(textReader, args.FieldMap);

                if (!CacheResourcesBeforeImport(reader))
                {
                    return _result;
                }

                int rowNumber = 0;
                foreach (T row in reader.AsEnumerable<T>())
                {
                    rowNumber++;
                    if (IsRowEmpty(row))
                    {
                        if (_result.EmptyRows.Count < 50)
                        {
                            _result.EmptyRows.Add(rowNumber);
                        }
                        _result.SkippedNumber++;
                        WriteRowErrors(row, rowNumber);
                        continue;
                    }

                    ImportRowAndSave(row, rowNumber);
                }

                if (!PostImportTasks())
                {
                    return _result;
                }

                CurrentUnitOfWork.SaveChanges();

                _result.IsImported = true;
                LogInfo("Import successfully finished");
                return _result;
            }
        }

        protected void WriteRowErrors(T row, int rowNumber)
        {
            if (row.ParseErrors.Count > 0)
            {
                _result.ParseErrors.Add(rowNumber, row.ParseErrors);
                LogParseErrors(rowNumber, row.ParseErrors);
            }

            if (row.StringExceedErrors.Count > 0)
            {
                _result.StringExceedErrors.Add(rowNumber, row.StringExceedErrors);
                LogStringExceedErrors(rowNumber, row.StringExceedErrors);
            }
        }

        protected virtual void ImportRowAndSave(T row, int rowNumber)
        {
            using (var unitOfWork = UnitOfWorkManager.Begin())
            using (CurrentUnitOfWork.SetTenantId(_tenantId))
            {
                if (ImportRow(row))
                {
                    _result.ImportedNumber++;
                    CurrentUnitOfWork.SaveChanges();
                }

                WriteRowErrors(row, rowNumber);
                unitOfWork.Complete();
            }
        }

        protected DateTime ConvertLocalDateTimeToUtcDateTime(DateTime utcDateTime) =>
            utcDateTime.ConvertTimeZoneFrom(_timeZone);

        protected abstract bool IsRowEmpty(T row);
        protected abstract bool ImportRow(T row);
        protected virtual bool CacheResourcesBeforeImport(IImportReader reader)
        {
            return true;
        }

        protected virtual bool PostImportTasks()
        {
            return true;
        }

        private string _importParameters;

        private void SetImportParametersForLog(int tenantId)
        {
            _importParameters = $"TenantId={tenantId}";
        }

        protected void LogInfo(string message)
        {
            Logger.Info($"{message}. Time: {DateTime.UtcNow} UTC, {_importParameters}");
        }

        protected void LogDebug(string message)
        {
            Logger.Debug($"{message}. Time: {DateTime.UtcNow} UTC, {_importParameters}");
        }

        protected void LogParseErrors(int rowNumber, Dictionary<string, (string value, Type type)> rowParseErrors)
        {
            LogDebug($"Parse errors at row {rowNumber}: {rowParseErrors.Select(pair => "Column: " + pair.Key + ", Value: " + pair.Value.value).JoinAsString(", ")}");
        }
        protected void LogStringExceedErrors(int rowNumber, Dictionary<string, Tuple<string, int>> rowStringExceedErrors)
        {
            LogDebug($"Exceed length errors at row {rowNumber}: {rowStringExceedErrors.Select(pair => "Column: " + pair.Key + ", Value: " + pair.Value.Item1 + ", Max Length: " + pair.Value.Item2).JoinAsString(", ")}");
        }
        protected void LogResourceErrors(List<string> resourceErrors)
        {
            if (resourceErrors.Count > 0)
            {
                LogDebug($"Resources not found: {resourceErrors.JoinAsString(", ")}");
            }
        }

        protected void AddResourceError(string error)
        {
            if (!_result.ResourceErrors.Contains(error))
            {
                _result.ResourceErrors.Add(error);
            }
        }

        protected bool TryParseCityStateZip(string addressRow, out string city, out string state, out string zip)
        {
            city = null;
            state = null;
            zip = null;
            var addressRowParts = addressRow.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (addressRowParts.Length < 3)
            {
                return false;
            }

            var zipCandidate = addressRowParts.Last();
            if (!zipCandidate.All(c => char.IsDigit(c) || c == '-'))
            {
                return false;
            }
            zip = zipCandidate;

            var stateCandidate = addressRowParts.SkipLast(1).Last();
            if (stateCandidate.Length != 2)
            {
                return false;
            }
            state = stateCandidate;

            var cityCandidate = string.Join(" ", addressRowParts.SkipLast(2)).TrimEnd(',');
            if (cityCandidate.IsNullOrEmpty())
            {
                return false;
            }
            city = cityCandidate;

            return true;
        }
    }
}
