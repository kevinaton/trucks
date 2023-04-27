using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.IO;
using Abp.Linq.Extensions;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Emailing;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.Projects;
using DispatcherWeb.Quotes.Dto;
using DispatcherWeb.Storage;
using DispatcherWeb.Tickets;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Quotes
{
    [AbpAuthorize]
    public class QuoteAppService : DispatcherWebAppServiceBase, IQuoteAppService
    {
        private readonly IRepository<Quote> _quoteRepository;
        private readonly IRepository<QuoteService> _quoteServiceRepository;
        private readonly IRepository<QuoteHistoryRecord> _quoteHistoryRepository;
        private readonly IRepository<QuoteFieldDiff> _quoteFieldDiffRepository;
        private readonly IRepository<QuoteEmail> _quoteEmailRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<ProjectService> _projectServiceRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<FuelSurchargeCalculation> _fuelSurchargeCalculationRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IAppFolders _appFolders;
        private readonly IEmailSender _emailSender;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly IWebUrlService _webUrlService;

        public QuoteAppService(
            IRepository<Quote> quoteRepository,
            IRepository<QuoteService> quoteServiceRepository,
            IRepository<QuoteHistoryRecord> quoteHistoryRepository,
            IRepository<QuoteFieldDiff> quoteFieldDiffRepository,
            IRepository<QuoteEmail> quoteEmailRepository,
            IRepository<Project> projectRepository,
            IRepository<ProjectService> projectServiceRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<User, long> userRepository,
            IRepository<FuelSurchargeCalculation> fuelSurchargeCalculationRepository,
            IBinaryObjectManager binaryObjectManager,
            IAppFolders appFolders,
            IEmailSender emailSender,
            ITrackableEmailSender trackableEmailSender,
            IWebUrlService webUrlService
            )
        {
            _quoteRepository = quoteRepository;
            _quoteServiceRepository = quoteServiceRepository;
            _quoteHistoryRepository = quoteHistoryRepository;
            _quoteFieldDiffRepository = quoteFieldDiffRepository;
            _quoteEmailRepository = quoteEmailRepository;
            _projectRepository = projectRepository;
            _projectServiceRepository = projectServiceRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _userRepository = userRepository;
            _fuelSurchargeCalculationRepository = fuelSurchargeCalculationRepository;
            _binaryObjectManager = binaryObjectManager;
            _appFolders = appFolders;
            _emailSender = emailSender;
            _trackableEmailSender = trackableEmailSender;
            _webUrlService = webUrlService;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PagedResultDto<QuoteDto>> GetQuotes(GetQuotesInput input)
        {
            var query = _quoteRepository.GetAll()
                .WhereIf(input.ProjectId.HasValue,
                    x => x.ProjectId == input.ProjectId)
                .WhereIf(input.QuoteId.HasValue,
                    x => x.Id == input.QuoteId)
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId)
                .WhereIf(input.SalesPersonId.HasValue,
                    x => x.SalesPersonId == input.SalesPersonId)
                .WhereIf(!string.IsNullOrEmpty(input.Misc),
                    x => x.Name.Contains(input.Misc)
                         || x.Project.Name.Contains(input.Misc)
                         || x.Description.Contains(input.Misc)
                         || x.Project.Description.Contains(input.Misc)
                         || x.Directions.Contains(input.Misc)
                         || x.Project.Location.Contains(input.Misc)
                     );

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new QuoteDto
                {
                    Id = x.Id,
                    QuoteName = x.Name,
                    ProjectName = x.Project.Name,
                    Description = x.Description,
                    CustomerName = x.Customer.Name,
                    QuoteDate = x.ProposalDate,
                    ContactName = x.Contact.Name,
                    SalesPersonName = x.SalesPerson.Name + " " + x.SalesPerson.Surname,
                    EmailDeliveryStatuses = x.QuoteEmails.Select(y => y.Email.CalculatedDeliveryStatus).ToList(),
                    Status = x.Status
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<QuoteDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task<ListResultDto<ProjectQuoteDto>> GetProjectQuotes(GetProjectQuotesInput input)
        {
            var items = await _quoteRepository.GetAll()
                .WhereIf(input.ProjectId.HasValue, x => x.ProjectId == input.ProjectId)
                .WhereIf(!input.ProjectId.HasValue, x => x.ProjectId == null)
                .Select(x => new ProjectQuoteDto
                {
                    Id = x.Id,
                    CustomerName = x.Customer.Name,
                    ContactName = x.Contact.Name,
                    ContactPhone = x.Contact.PhoneNumber,
                    ContactEmail = x.Contact.Email
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new ListResultDto<ProjectQuoteDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View, AppPermissions.Pages_Quotes_View)]
        public async Task<ListResultDto<SelectListDto>> GetQuotesForCustomer(GetQuotesForCustomerInput input)
        {
            if (input.Id == null)
            {
                return new ListResultDto<SelectListDto>();
            }
            var quotes = await _quoteRepository.GetAll()
                .Where(x => x.CustomerId == input.Id)
                .WhereIf(input.HideInactive, x => x.Status != QuoteStatus.Inactive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListDto<QuoteSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new QuoteSelectListInfoDto
                    {
                        ProjectId = x.ProjectId,
                        ContactId = x.ContactId,
                        Directions = x.Directions,
                        PONumber = x.PONumber,
                        SpectrumNumber = x.SpectrumNumber,
                        Status = x.Status,
                        CustomerId = x.CustomerId,
                        ChargeTo = x.ChargeTo,
                        FuelSurchargeCalculationId = x.FuelSurchargeCalculationId,
                        FuelSurchargeCalculationName = x.FuelSurchargeCalculation.Name,
                        BaseFuelCost = x.BaseFuelCost,
                        CanChangeBaseFuelCost = x.FuelSurchargeCalculation.CanChangeBaseFuelCost
                    }
                })
                .ToListAsync();
            return new ListResultDto<SelectListDto>(quotes);
        }

        public async Task<PagedResultDto<SelectListDto>> GetQuoteSalesrepSelectList(GetSelectListInput input)
        {
            var query = _quoteRepository.GetAll()
                .Select(x => x.SalesPerson)
                .Distinct()
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name + " " + x.Surname
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<QuoteEditDto> GetQuoteForEdit(GetQuoteForEditInput input)
        {
            QuoteEditDto quoteEditDto;

            if (input.Id.HasValue)
            {
                quoteEditDto = await _quoteRepository.GetAll()
                    .Select(quote => new QuoteEditDto
                    {
                        Id = quote.Id,
                        ProjectId = quote.ProjectId,
                        ProjectName = quote.Project.Name,
                        Name = quote.Name,
                        CustomerId = quote.CustomerId,
                        CustomerName = quote.Customer.Name,
                        ContactId = quote.ContactId,
                        ContactName = quote.Contact.Name,
                        Description = quote.Description,
                        ProposalDate = quote.ProposalDate,
                        ProposalExpiryDate = quote.ProposalExpiryDate,
                        InactivationDate = quote.InactivationDate,
                        Status = quote.Status,
                        SalesPersonId = quote.SalesPersonId,
                        SalesPersonName = quote.SalesPerson.Name + " " + quote.SalesPerson.Surname,
                        PONumber = quote.PONumber,
                        SpectrumNumber = quote.SpectrumNumber,
                        BaseFuelCost = quote.BaseFuelCost,
                        FuelSurchargeCalculationId = quote.FuelSurchargeCalculationId,
                        FuelSurchargeCalculationName = quote.FuelSurchargeCalculation.Name,
                        CanChangeBaseFuelCost = quote.FuelSurchargeCalculation.CanChangeBaseFuelCost,
                        Directions = quote.Directions,
                        Notes = quote.Notes,
                        ChargeTo = quote.ChargeTo,
                        HasOrders = quote.Orders.Any(),
                    })
                    .FirstAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                if (input.ProjectId.HasValue)
                {
                    quoteEditDto = await _projectRepository.GetAll()
                        .Where(x => x.Id == input.ProjectId.Value)
                        .Select(project => new QuoteEditDto
                        {
                            ProjectId = project.Id,
                            ProjectName = project.Name,
                            Name = project.Name,
                            Description = project.Description,
                            InactivationDate = null,
                            Status = project.Status,
                            PONumber = project.PONumber,
                            Directions = project.Directions,
                            Notes = project.Notes,
                            ChargeTo = project.ChargeTo
                        })
                        .FirstAsync();

                    if (!Session.OfficeCopyChargeTo)
                    {
                        quoteEditDto.ChargeTo = null;
                    }
                }
                else
                {
                    quoteEditDto = new QuoteEditDto
                    {
                        Notes = await SettingManager.GetSettingValueAsync(AppSettings.Quote.DefaultNotes)
                    };
                }

                var today = await GetToday();
                quoteEditDto.ProposalDate = today;
                quoteEditDto.ProposalExpiryDate = today.AddDays(30);

                quoteEditDto.FuelSurchargeCalculationId = await SettingManager.GetDefaultFuelSurchargeCalculationId();
                if (quoteEditDto.FuelSurchargeCalculationId > 0)
                {
                    var fuelSurchargeCalculation = await _fuelSurchargeCalculationRepository.GetAll()
                        .Where(x => x.Id == quoteEditDto.FuelSurchargeCalculationId)
                        .Select(x => new
                        {
                            x.Name,
                            x.CanChangeBaseFuelCost,
                            x.BaseFuelCost
                        })
                        .FirstOrDefaultAsync();

                    quoteEditDto.FuelSurchargeCalculationName = fuelSurchargeCalculation.Name;
                    quoteEditDto.CanChangeBaseFuelCost = fuelSurchargeCalculation.CanChangeBaseFuelCost;
                    quoteEditDto.BaseFuelCost = fuelSurchargeCalculation.BaseFuelCost;
                }
            }

            if (!quoteEditDto.SalesPersonId.HasValue)
            {
                var userFullName = await UserManager.Users.Where(x => x.Id == AbpSession.UserId).Select(x => x.Name + " " + x.Surname).FirstOrDefaultAsync();
                quoteEditDto.SalesPersonId = AbpSession.UserId;
                quoteEditDto.SalesPersonName = userFullName;
            }

            return quoteEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task<int> EditQuote(QuoteEditDto model)
        {
            var quote = model.Id.HasValue ? await _quoteRepository.GetAsync(model.Id.Value) : new Quote();

            var fieldDiffs = new List<QuoteFieldDiff>();

            if (quote.ProjectId != model.ProjectId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Project, quote.ProjectId, model.ProjectId));
                }
                quote.ProjectId = model.ProjectId;
            }

            if (quote.Name != model.Name)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Name, quote.Name, model.Name));
                }
                quote.Name = model.Name;
            }

            if (quote.CustomerId != model.CustomerId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Customer, quote.CustomerId, model.CustomerId));
                }
                quote.CustomerId = model.CustomerId;
            }

            if (quote.ContactId != model.ContactId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Contact, quote.ContactId, model.ContactId));
                }
                quote.ContactId = model.ContactId;
            }

            if (quote.Description != model.Description)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Description, quote.Description, model.Description));
                }
                quote.Description = model.Description;
            }

            if (quote.ProposalDate?.Date != model.ProposalDate?.Date)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.ProposalDate, quote.ProposalDate?.ToShortDateString(), model.ProposalDate?.ToShortDateString()));
                }
                quote.ProposalDate = model.ProposalDate?.Date;
            }

            if (quote.ProposalExpiryDate?.Date != model.ProposalExpiryDate?.Date)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.ProposalExpiryDate, quote.ProposalExpiryDate?.ToShortDateString(), model.ProposalExpiryDate?.ToShortDateString()));
                }
                quote.ProposalExpiryDate = model.ProposalExpiryDate?.Date;
            }

            if (quote.InactivationDate?.Date != model.InactivationDate?.Date)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.InactivationDate, quote.InactivationDate?.ToShortDateString(), model.InactivationDate?.ToShortDateString()));
                }
                quote.InactivationDate = model.InactivationDate?.Date;
            }

            if (quote.Status != model.Status)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Status, (int)quote.Status, quote.Status.GetDisplayName(), (int)model.Status, model.Status.GetDisplayName()));
                }
                quote.Status = model.Status;
            }

            if (quote.SalesPersonId != model.SalesPersonId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.SalesPerson, (int?)quote.SalesPersonId, (int?)model.SalesPersonId));
                }
                quote.SalesPersonId = model.SalesPersonId;
            }

            if (quote.PONumber != model.PONumber)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.PoNumber, quote.PONumber, model.PONumber));
                }
                quote.PONumber = model.PONumber;
            }

            if (quote.SpectrumNumber != model.SpectrumNumber)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.SpectrumNumber, quote.SpectrumNumber, model.SpectrumNumber));
                }
                quote.SpectrumNumber = model.SpectrumNumber;
            }

            if (quote.BaseFuelCost != model.BaseFuelCost)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.BaseFuelCost, quote.BaseFuelCost?.ToString(), model.BaseFuelCost?.ToString()));
                }
                quote.BaseFuelCost = model.BaseFuelCost;
            }

            if (quote.FuelSurchargeCalculationId != model.FuelSurchargeCalculationId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.FuelSurchargeCalculation, (int?)quote.FuelSurchargeCalculationId, (int?)model.FuelSurchargeCalculationId));
                }
                quote.FuelSurchargeCalculationId = model.FuelSurchargeCalculationId;
            }

            if (quote.Directions != model.Directions)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Directions, quote.Directions, model.Directions));
                }
                quote.Directions = model.Directions;
            }

            if (quote.Notes != model.Notes)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Notes, quote.Notes, model.Notes));
                }
                quote.Notes = model.Notes;
            }

            if (Session.OfficeCopyChargeTo)
            {
                if (quote.ChargeTo != model.ChargeTo)
                {
                    if (quote.CaptureHistory)
                    {
                        fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.ChargeTo, quote.ChargeTo, model.ChargeTo));
                    }
                    quote.ChargeTo = model.ChargeTo;
                }
            }

            await UpdateDiffDisplayValues(quote.Id, false, fieldDiffs);

            if (quote.Id == 0)
            {
                if (model.ProjectId.HasValue)
                {
                    //get project services to add to the new quote
                    var projectServices = await _projectServiceRepository.GetAll()
                        .Where(x => x.ProjectId == model.ProjectId)
                        .ToListAsync();
                    projectServices.Select(x => new QuoteService
                    {
                        LoadAtId = x.LoadAtId,
                        DeliverToId = x.DeliverToId,
                        ServiceId = x.ServiceId,
                        MaterialUomId = x.MaterialUomId,
                        FreightUomId = x.FreightUomId,
                        Designation = x.Designation,
                        PricePerUnit = x.PricePerUnit,
                        FreightRate = x.FreightRate,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        Note = x.Note
                    })
                    .ToList().ForEach(x =>
                    {
                        quote.QuoteServices.Add(x);
                        _quoteServiceRepository.Insert(x);
                    });

                    var project = await _projectRepository.GetAsync(model.ProjectId.Value);
                    if (project.Status == QuoteStatus.Pending)
                    {
                        project.Status = QuoteStatus.Active;
                        await _projectRepository.UpdateAsync(project);
                    }
                }

                await _quoteRepository.InsertAsync(quote);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            await UpdateDiffDisplayValues(quote.Id, true, fieldDiffs);

            await InsertQuoteHistory(fieldDiffs, QuoteChangeType.QuoteBodyEdited, quote.Id, quote.CreatorUserId);

            return quote.Id;
        }

        private async Task InsertQuoteHistory(List<QuoteFieldDiff> fieldDiffs, QuoteChangeType changeType, int quoteId, long? creatorId = null)
        {
            if (fieldDiffs.Any())
            {
                var quoteHistory = new QuoteHistoryRecord
                {
                    DateTime = Clock.Now,
                    ChangeType = changeType,
                    QuoteId = quoteId
                };
                _quoteHistoryRepository.Insert(quoteHistory);

                fieldDiffs.ForEach(x =>
                {
                    x.QuoteHistoryRecord = quoteHistory;
                    _quoteFieldDiffRepository.Insert(x);
                });

                await CurrentUnitOfWork.SaveChangesAsync();

                await SendQuoteChangedEmail(quoteId, quoteHistory.Id, creatorId);
            }
        }

        private async Task SendQuoteChangedEmail(int quoteId, int quoteHistoryId, long? creatorId = null)
        {
            creatorId = creatorId ?? await _quoteRepository.GetAll().Where(x => x.Id == quoteId).Select(x => x.CreatorUserId).FirstOrDefaultAsync();
            if (Session.UserId != creatorId)
            {
                var creatorEmail = await _userRepository.GetAll().Where(x => x.Id == creatorId).Select(x => x.EmailAddress).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(creatorEmail))
                {
                    return;
                }

                var changedByUser = await _userRepository.GetAll()
                   .Where(x => x.Id == Session.UserId)
                   .Select(x => new
                   {
                       FirstName = x.Name,
                       LastName = x.Surname,
                       PhoneNumber = x.PhoneNumber
                   })
                   .FirstAsync();

                var quote = await _quoteRepository.GetAll()
                    .Where(x => x.Id == quoteId)
                    .Select(x => new
                    {
                        CustomerName = x.Customer.Name
                    })
                    .FirstAsync();

                var siteUrl = _webUrlService.GetSiteRootAddress();

                var subject = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.SubjectTemplate);
                subject = subject
                    .Replace("{Quote.Id}", quoteId.ToString());

                var body = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.BodyTemplate);
                body = body
                    .Replace("{Quote.Id}", quoteId.ToString())
                    .Replace("{Quote.Url}", siteUrl + "app/quotes/details/" + quoteId)
                    .Replace("{QuoteHistory.Url}", siteUrl + "app/QuoteHistory/Index/" + quoteHistoryId)
                    .Replace("{Customer.Name}", quote.CustomerName)
                    .Replace("{ChangedByUser.FirstName}", changedByUser.FirstName)
                    .Replace("{ChangedByUser.LastName}", changedByUser.LastName)
                    .Replace("{ChangedByUser.PhoneNumber}", changedByUser.PhoneNumber);


                var message = new MailMessage
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(creatorEmail);
                await _emailSender.SendAsync(message);
            }
        }

        private async Task UpdateDiffDisplayValues(int recordId, bool updateNewValues, List<QuoteFieldDiff> diffs)
        {
            diffs.RemoveAll(x => (x.OldDisplayValue != null || x.NewDisplayValue != null)
                            && ((x.OldDisplayValue ?? string.Empty) == (x.NewDisplayValue ?? string.Empty)));

            var quoteFieldsWithDisplayValue = new[]
            {
                QuoteFieldEnum.Customer,
                QuoteFieldEnum.Contact,
                QuoteFieldEnum.Project,
                QuoteFieldEnum.SalesPerson,
                QuoteFieldEnum.FuelSurchargeCalculation,
            };

            if (diffs.Any(x => quoteFieldsWithDisplayValue.Contains(x.Field)))
            {
                var quoteDisplayValues = await _quoteRepository.GetAll()
                    .Where(x => x.Id == recordId)
                    .Select(x => new
                    {
                        CustomerName = x.Customer.Name,
                        ContactName = x.Contact.Name,
                        ProjectName = x.Project.Name,
                        SalesPersonName = x.SalesPerson.Name + " " + x.SalesPerson.Surname,
                        FuelSurchargeCalculationName = x.FuelSurchargeCalculation.Name,
                    })
                    .FirstOrDefaultAsync();

                if (updateNewValues)
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.Customer).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.CustomerName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.Contact).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.ContactName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.Project).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.ProjectName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.SalesPerson).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.SalesPersonName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.FuelSurchargeCalculation).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.FuelSurchargeCalculationName);
                }
                else
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.Customer).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.CustomerName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.Contact).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.ContactName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.Project).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.ProjectName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.SalesPerson).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.SalesPersonName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.FuelSurchargeCalculation).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.FuelSurchargeCalculationName);
                }
            }

            var quoteServiceFieldsWithDisplayValue = new[]
            {
                QuoteFieldEnum.LineItemService,
                QuoteFieldEnum.LineItemLoadAt,
                QuoteFieldEnum.LineItemDeliverTo,
                QuoteFieldEnum.LineItemMaterialUom,
                QuoteFieldEnum.LineItemFreightUom
            };

            if (diffs.Any(x => quoteServiceFieldsWithDisplayValue.Contains(x.Field)))
            {
                var displayValues = await _quoteServiceRepository.GetAll()
                    .Where(x => x.Id == recordId)
                    .Select(x => new
                    {
                        ServiceName = x.Service.Service1,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomName = x.FreightUom.Name,
                    })
                    .FirstOrDefaultAsync();

                if (updateNewValues)
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemService).ToList().ForEach(x => x.NewDisplayValue = displayValues?.ServiceName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemLoadAt).ToList().ForEach(x => x.NewDisplayValue = displayValues?.LoadAt?.FormattedAddress);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemDeliverTo).ToList().ForEach(x => x.NewDisplayValue = displayValues?.DeliverTo?.FormattedAddress);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemMaterialUom).ToList().ForEach(x => x.NewDisplayValue = displayValues?.MaterialUomName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemFreightUom).ToList().ForEach(x => x.NewDisplayValue = displayValues?.FreightUomName);
                }
                else
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemService).ToList().ForEach(x => x.OldDisplayValue = displayValues?.ServiceName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemLoadAt).ToList().ForEach(x => x.OldDisplayValue = displayValues?.LoadAt?.FormattedAddress);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemDeliverTo).ToList().ForEach(x => x.OldDisplayValue = displayValues?.DeliverTo?.FormattedAddress);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemMaterialUom).ToList().ForEach(x => x.OldDisplayValue = displayValues?.MaterialUomName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemFreightUom).ToList().ForEach(x => x.OldDisplayValue = displayValues?.FreightUomName);
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task<int> CopyQuote(EntityDto input)
        {
            var quote = await _quoteRepository.GetAll()
                .AsNoTracking()
                .Include(x => x.QuoteServices)
                .FirstAsync(x => x.Id == input.Id);

            var today = await GetToday();

            var newQuote = new Quote
            {
                ProjectId = quote.ProjectId,
                CustomerId = quote.CustomerId,
                ContactId = quote.ContactId,
                Name = quote.Name,
                Description = quote.Description,
                ProposalDate = today,
                ProposalExpiryDate = today.AddDays(30),
                Status = quote.Status,
                SalesPersonId = quote.SalesPersonId,
                PONumber = quote.PONumber,
                SpectrumNumber = quote.SpectrumNumber,
                BaseFuelCost = quote.BaseFuelCost,
                FuelSurchargeCalculationId = quote.FuelSurchargeCalculationId,
                Directions = quote.Directions,
                Notes = quote.Notes,
                CaptureHistory = false
            };

            if (Session.OfficeCopyChargeTo)
            {
                newQuote.ChargeTo = quote.ChargeTo;
            }

            quote.QuoteServices.Select(s => new QuoteService
            {
                LoadAtId = s.LoadAtId,
                DeliverToId = s.DeliverToId,
                ServiceId = s.ServiceId,
                MaterialUomId = s.MaterialUomId,
                FreightUomId = s.FreightUomId,
                Designation = s.Designation,
                PricePerUnit = s.PricePerUnit,
                FreightRate = s.FreightRate,
                LeaseHaulerRate = s.LeaseHaulerRate,
                FreightRateToPayDrivers = s.FreightRateToPayDrivers,
                MaterialQuantity = s.MaterialQuantity,
                FreightQuantity = s.FreightQuantity,
                JobNumber = s.JobNumber,
                Note = s.Note,
                Quote = newQuote
            }).ToList().ForEach(x =>
            {
                _quoteServiceRepository.Insert(x);
            });

            return await _quoteRepository.InsertAndGetIdAsync(newQuote);
        }

        [AbpAuthorize(
            AppPermissions.Pages_Quotes_Edit,
            AppPermissions.Pages_Orders_Edit,
            AppPermissions.Pages_Projects,
            RequireAllPermissions = true
        )]
        public async Task<int> CreateQuoteFromOrder(CreateQuoteFromOrderInput input)
        {
            var order = await _orderRepository
                .GetAllIncluding(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == input.OrderId);

            if (order == null)
            {
                throw new UserFriendlyException("Order with the specified Id wasn't found");
            }

            if (order.LocationId != Session.OfficeId)
            {
                throw new AbpAuthorizationException("A user is not allowed to edit the Order from another office.");
            }

            if (!order.Customer.IsActive)
            {
                throw new UserFriendlyException("Quotes can't be created for inactive customers.");
            }

            var orderLines = await _orderLineRepository.GetAll()
                .Where(x => x.OrderId == input.OrderId)
                .Select(x => new
                {
                    x.Designation,
                    x.FreightPrice,
                    x.FreightPricePerUnit,
                    x.IsFreightPricePerUnitOverridden,
                    x.IsMaterialPricePerUnitOverridden,
                    x.LeaseHaulerRate,
                    x.FreightRateToPayDrivers,
                    x.LineNumber,
                    x.MaterialPrice,
                    x.MaterialPricePerUnit,
                    x.JobNumber,
                    x.Note,
                    x.MaterialQuantity,
                    x.FreightQuantity,
                    x.ServiceId,
                    x.LoadAtId,
                    x.DeliverToId,
                    x.MaterialUomId,
                    x.FreightUomId
                })
                .OrderBy(x => x.LineNumber)
                .ToListAsync();

            var today = await GetToday();

            var quote = new Quote
            {
                ContactId = order.ContactId,
                CustomerId = order.CustomerId,
                Directions = order.Directions,
                Name = input.QuoteName,
                PONumber = order.PONumber,
                Status = QuoteStatus.Active,
                SalesPersonId = AbpSession.UserId,
                Notes = await SettingManager.GetSettingValueAsync(AppSettings.Quote.DefaultNotes),
                ProposalDate = today,
                ProposalExpiryDate = today.AddDays(30)
            };

            if (Session.OfficeCopyChargeTo)
            {
                quote.ChargeTo = order.ChargeTo;
            }

            quote.Id = await _quoteRepository.InsertAndGetIdAsync(quote);

            var quoteServices = orderLines.Select(x => new QuoteService
            {
                QuoteId = quote.Id,
                Designation = x.Designation,
                JobNumber = x.JobNumber,
                Note = x.Note,
                FreightRate = x.FreightPricePerUnit,
                PricePerUnit = x.MaterialPricePerUnit,
                LeaseHaulerRate = x.LeaseHaulerRate,
                FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                MaterialQuantity = x.MaterialQuantity,
                FreightQuantity = x.FreightQuantity,
                ServiceId = x.ServiceId,
                LoadAtId = x.LoadAtId,
                DeliverToId = x.DeliverToId,
                MaterialUomId = x.MaterialUomId,
                FreightUomId = x.FreightUomId
            }).ToList();

            foreach (var quoteService in quoteServices)
            {
                quoteService.Id = await _quoteServiceRepository.InsertAndGetIdAsync(quoteService);
            }

            order.QuoteId = quote.Id;

            return quote.Id;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit, AppPermissions.Pages_Orders_Edit)]
        public async Task SetQuoteStatus(SetQuoteStatusInput model)
        {
            var quote = await _quoteRepository.GetAsync(model.Id);
            quote.Status = model.Status;
            if (quote.Status == QuoteStatus.Active && quote.ProjectId.HasValue)
            {
                var project = await _projectRepository.GetAsync(quote.ProjectId.Value);
                project.Status = quote.Status;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task<bool> CanDeleteQuote(EntityDto input)
        {

            var quote = await _quoteRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.CaptureHistory
                }).SingleAsync();

            if (quote.CaptureHistory)
            {
                return false;
            }

            var hasOrders = await _orderRepository.GetAll().AnyAsync(x => x.QuoteId == input.Id);
            if (hasOrders)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task DeleteQuote(EntityDto input)
        {
            var canDelete = await CanDeleteQuote(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _quoteServiceRepository.DeleteAsync(x => x.QuoteId == input.Id);
            await _quoteRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Projects)]
        public async Task InactivateQuote(EntityDto input)
        {
            var quote = await _quoteRepository.GetAsync(input.Id);
            quote.InactivationDate = await GetToday();
            quote.Status = QuoteStatus.Inactive;
        }

        //*********************//

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PagedResultDto<QuoteServiceDto>> GetQuoteServices(GetQuoteServicesInput input)
        {
            var query = _quoteServiceRepository.GetAll();

            var totalCount = await query.CountAsync();

            var items = await query
                .Where(x => x.QuoteId == input.QuoteId)
                .WhereIf(input.LoadAtId.HasValue || input.ForceDuplicateFilters,
                         x => x.LoadAtId == input.LoadAtId)
                .WhereIf(input.DeliverToId.HasValue || input.ForceDuplicateFilters,
                         x => x.DeliverToId == input.DeliverToId)
                .WhereIf(input.ServiceId.HasValue,
                         x => x.ServiceId == input.ServiceId)
                .WhereIf(input.MaterialUomId.HasValue,
                         x => x.MaterialUomId == input.MaterialUomId)
                .WhereIf(input.FreightUomId.HasValue,
                         x => x.FreightUomId == input.FreightUomId)
                .WhereIf(input.Designation.HasValue,
                         x => x.Designation == input.Designation)
                .Select(x => new QuoteServiceDto
                {
                    Id = x.Id,
                    LoadAtNamePlain = x.LoadAt.Name + x.LoadAt.StreetAddress + x.LoadAt.City + x.LoadAt.State, //for sorting
                    LoadAt = x.LoadAt == null ? null : new LocationNameDto
                    {
                        Name = x.LoadAt.Name,
                        StreetAddress = x.LoadAt.StreetAddress,
                        City = x.LoadAt.City,
                        State = x.LoadAt.State
                    },
                    DeliverToNamePlain = x.LoadAt.Name + x.LoadAt.StreetAddress + x.LoadAt.City + x.LoadAt.State, //for sorting
                    DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                    {
                        Name = x.DeliverTo.Name,
                        StreetAddress = x.DeliverTo.StreetAddress,
                        City = x.DeliverTo.City,
                        State = x.DeliverTo.State
                    },
                    ServiceName = x.Service.Service1,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightUomName = x.FreightUom.Name,
                    Designation = x.Designation,
                    PricePerUnit = x.PricePerUnit,
                    FreightRate = x.FreightRate,
                    LeaseHaulerRate = x.LeaseHaulerRate,
                    FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                    MaterialQuantity = x.MaterialQuantity,
                    FreightQuantity = x.FreightQuantity
                })
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            return new PagedResultDto<QuoteServiceDto>(
                totalCount,
                items);
        }

        public async Task<PagedResultDto<QuoteDeliveryDto>> GetQuoteServiceDeliveries(EntityDto input)
        {
            var quoteService = await _quoteServiceRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.QuoteId,
                    x.ServiceId,
                    Tickets = x.OrderLines
                        .SelectMany(ol => ol.Tickets)
                        .Select(t => new QuoteDeliveryRawDto
                        {
                            Date = t.OrderLine.Order.DeliveryDate,
                            Designation = t.OrderLine.Designation,
                            MaterialUomId = t.OrderLine.MaterialUomId,
                            FreightUomId = t.OrderLine.FreightUomId,
                            TicketUomId = t.UnitOfMeasureId,
                            Quantity = t.Quantity
                        })
                })
                .FirstAsync();

            var groupedItems = quoteService.Tickets.GroupBy(x => new { x.Date, x.Designation })
                .Select(g => new QuoteDeliveryDto
                {
                    Date = g.Key.Date,
                    Designation = g.Key.Designation,
                    ActualFreightQuantity = g.Sum(t => t.GetFreightQuantity() ?? 0),
                    ActualMaterialQuantity = g.Sum(t => t.GetMaterialQuantity() ?? 0),
                }).ToList();

            return new PagedResultDto<QuoteDeliveryDto>(groupedItems.Count, groupedItems);
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<QuoteServiceEditDto> GetQuoteServiceForEdit(GetQuoteServiceForEditInput input)
        {
            QuoteServiceEditDto quoteServiceEditDto;

            if (input.Id.HasValue)
            {
                quoteServiceEditDto = await _quoteServiceRepository.GetAll()
                    .Select(x => new QuoteServiceEditDto
                    {
                        Id = x.Id,
                        QuoteId = x.QuoteId,
                        LoadAtId = x.LoadAtId,
                        LoadAt = x.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = x.LoadAt.Name,
                            StreetAddress = x.LoadAt.StreetAddress,
                            City = x.LoadAt.City,
                            State = x.LoadAt.State
                        },
                        DeliverToId = x.DeliverToId,
                        DeliverTo = x.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = x.DeliverTo.Name,
                            StreetAddress = x.DeliverTo.StreetAddress,
                            City = x.DeliverTo.City,
                            State = x.DeliverTo.State
                        },
                        ServiceId = x.ServiceId,
                        ServiceName = x.Service.Service1,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        Designation = x.Designation,
                        PricePerUnit = x.PricePerUnit,
                        FreightRate = x.FreightRate,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        JobNumber = x.JobNumber,
                        Note = x.Note
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else if (input.QuoteId.HasValue)
            {
                quoteServiceEditDto = new QuoteServiceEditDto
                {
                    QuoteId = input.QuoteId.Value
                };
            }
            else
            {
                throw new ArgumentNullException(nameof(input.QuoteId));
            }

            return quoteServiceEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit, AppPermissions.Pages_Quotes_Items_Create)]
        public async Task EditQuoteService(QuoteServiceEditDto model)
        {
            var quoteService = model.Id.HasValue ? await _quoteServiceRepository.GetAsync(model.Id.Value) : new QuoteService();
            var isNew = quoteService.Id == 0;

            if (!isNew)
            {
                await PermissionChecker.AuthorizeAsync(AppPermissions.Pages_Quotes_Edit);
            }

            if (isNew)
            {
                quoteService.QuoteId = model.QuoteId;
            }

            var fieldDiffs = await UpdateValuesAndGetDiff(quoteService, model);

            await UpdateDiffDisplayValues(quoteService.Id, false, fieldDiffs);

            if (isNew)
            {
                await _quoteServiceRepository.InsertAsync(quoteService);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            await UpdateDiffDisplayValues(quoteService.Id, true, fieldDiffs);

            await InsertQuoteHistory(fieldDiffs, isNew ? QuoteChangeType.LineItemAdded : QuoteChangeType.LineItemEdited, quoteService.QuoteId);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task DeleteQuoteServices(IdListInput input)
        {
            if (await _quoteServiceRepository.GetAll()
                    .AnyAsync(x => input.Ids.Contains(x.Id) && x.OrderLines.Any()))
            {
                throw new UserFriendlyException(L("UnableToDeleteQuoteServiceWithAssociatedData"));
            }

            var quoteServices = await _quoteServiceRepository.GetAll()
                .Where(x => input.Ids.Contains(x.Id))
                .ToListAsync();

            foreach (var quoteService in quoteServices)
            {
                var fieldDiffs = await UpdateValuesAndGetDiff(quoteService.Clone(), new QuoteServiceEditDto());
                await UpdateDiffDisplayValues(quoteService.Id, false, fieldDiffs);
                await InsertQuoteHistory(fieldDiffs, QuoteChangeType.LineItemDeleted, quoteService.QuoteId);
                await _quoteServiceRepository.DeleteAsync(quoteService);
            }
        }

        private async Task<List<QuoteFieldDiff>> UpdateValuesAndGetDiff(QuoteService quoteService, QuoteServiceEditDto model)
        {
            var captureHistory = await _quoteRepository.GetAll()
                                     .Where(x => x.Id == quoteService.QuoteId)
                                     .Select(x => x.CaptureHistory)
                                     .FirstOrDefaultAsync();

            var fieldDiffs = new List<QuoteFieldDiff>();

            if (quoteService.LoadAtId != model.LoadAtId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemLoadAt, quoteService.LoadAtId, model.LoadAtId));
                }
                quoteService.LoadAtId = model.LoadAtId;
            }

            if (quoteService.DeliverToId != model.DeliverToId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemDeliverTo, quoteService.DeliverToId, model.DeliverToId));
                }
                quoteService.DeliverToId = model.DeliverToId;
            }

            if (quoteService.ServiceId != model.ServiceId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemService, quoteService.ServiceId, model.ServiceId));
                }
                quoteService.ServiceId = model.ServiceId;
            }

            if (quoteService.MaterialUomId != model.MaterialUomId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemMaterialUom, quoteService.MaterialUomId, model.MaterialUomId));
                }
                quoteService.MaterialUomId = model.MaterialUomId;
            }

            if (quoteService.FreightUomId != model.FreightUomId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightUom, quoteService.FreightUomId, model.FreightUomId));
                }
                quoteService.FreightUomId = model.FreightUomId;
            }

            if (quoteService.Designation != model.Designation)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemDesignation, (int)quoteService.Designation, quoteService.Designation.GetDisplayName(), (int)model.Designation, model.Designation.GetDisplayName()));
                }
                quoteService.Designation = model.Designation;
            }

            if (quoteService.PricePerUnit != model.PricePerUnit)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemPricePerUnit, quoteService.PricePerUnit?.ToString(), model.PricePerUnit?.ToString()));
                }
                quoteService.PricePerUnit = model.PricePerUnit;
            }

            if (quoteService.FreightRate != model.FreightRate)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightRate, quoteService.FreightRate?.ToString(), model.FreightRate?.ToString()));
                }
                quoteService.FreightRate = model.FreightRate;
            }

            if (quoteService.LeaseHaulerRate != model.LeaseHaulerRate)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemLeaseHaulerRate, quoteService.LeaseHaulerRate?.ToString(), model.LeaseHaulerRate?.ToString()));
                }
                quoteService.LeaseHaulerRate = model.LeaseHaulerRate;
            }

            if (quoteService.FreightRateToPayDrivers != model.FreightRateToPayDrivers)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightRateToPayDrivers, quoteService.FreightRateToPayDrivers?.ToString(), model.FreightRateToPayDrivers?.ToString()));
                }
                quoteService.FreightRateToPayDrivers = model.FreightRateToPayDrivers;
            }

            if (quoteService.MaterialQuantity != model.MaterialQuantity)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemMaterialQuantity, quoteService.MaterialQuantity?.ToString(), model.MaterialQuantity?.ToString()));
                }
                quoteService.MaterialQuantity = model.MaterialQuantity;
            }

            if (quoteService.FreightQuantity != model.FreightQuantity)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightQuantity, quoteService.FreightQuantity?.ToString(), model.FreightQuantity?.ToString()));
                }
                quoteService.FreightQuantity = model.FreightQuantity;
            }

            if (quoteService.JobNumber != model.JobNumber)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemNote, quoteService.JobNumber, model.JobNumber));
                }
                quoteService.JobNumber = model.JobNumber;
            }

            if (quoteService.Note != model.Note)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemNote, quoteService.Note, model.Note));
                }
                quoteService.Note = model.Note;
            }

            return fieldDiffs;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<byte[]> GetQuoteReport(GetQuoteReportInput input)
        {
            var data = await _quoteRepository.GetAll()
                .Where(x => x.Id == input.QuoteId)
                .Select(x => new QuoteReportDto
                {
                    ContactAttn = x.Contact.Name,
                    ContactPhoneNumber = x.Contact.PhoneNumber,
                    CustomerName = x.Customer.Name,
                    CustomerAddress1 = x.Customer.Address1,
                    CustomerAddress2 = x.Customer.Address2,
                    CustomerCity = x.Customer.City,
                    CustomerState = x.Customer.State,
                    CustomerZipCode = x.Customer.ZipCode,
                    CustomerCountryCode = x.Customer.CountryCode,
                    ProjectName = x.Project.Name,
                    QuotePoNumber = x.PONumber,
                    QuoteId = x.Id,
                    QuoteName = x.Name,
                    QuoteNotes = x.Notes,
                    QuoteBaseFuelCost = x.BaseFuelCost,
                    QuoteProposalDate = x.ProposalDate,
                    QuoteProposalExpiryDate = x.ProposalExpiryDate,
                    SalesPersonId = x.SalesPersonId,
                    Items = x.QuoteServices.Select(s => new QuoteReportItemDto
                    {
                        MaterialQuantity = s.MaterialQuantity,
                        FreightQuantity = s.FreightQuantity,
                        MaterialUomName = s.MaterialUom.Name,
                        FreightUomName = s.FreightUom.Name,
                        FreightRate = s.FreightRate,
                        LeaseHaulerRate = s.LeaseHaulerRate,
                        FreightRateToPayDrivers = s.FreightRateToPayDrivers,
                        PricePerUnit = s.PricePerUnit,
                        Code = s.Service.Service1,
                        Description = s.Service.Description,
                        Designation = s.Designation,
                        LoadAt = s.LoadAt == null ? null : new LocationNameDto
                        {
                            Name = s.LoadAt.Name,
                            StreetAddress = s.LoadAt.StreetAddress,
                            City = s.LoadAt.City,
                            State = s.LoadAt.State
                        },
                        DeliverTo = s.DeliverTo == null ? null : new LocationNameDto
                        {
                            Name = s.DeliverTo.Name,
                            StreetAddress = s.DeliverTo.StreetAddress,
                            City = s.DeliverTo.City,
                            State = s.DeliverTo.State
                        },
                        JobNumber = s.JobNumber,
                        Note = s.Note
                    }).ToList()
                })
                .FirstAsync();

            var user = await _userRepository.GetAll()
                .Where(x => x.Id == data.SalesPersonId || x.Id == Session.UserId)
                .Select(x => new
                {
                    Id = x.Id,
                    Email = x.EmailAddress,
                    FullName = x.Name + " " + x.Surname,
                    SignaturePictureId = x.SignaturePictureId
                })
                .OrderByDescending(x => x.Id == data.SalesPersonId) //get sales rep, but fallback to the current user if creator is not set
                .FirstAsync();

            data.UserEmail = user.Email;
            data.UserFullName = user.FullName;
            data.LogoPath = await GetLogoBase64String();
            //data.LogoPath = (await GetCurrentTenantAsync()).LogoPath();
            data.SignaturePath = await GetSignaturePictureTempPath(user.SignaturePictureId);
            data.Today = await GetToday();
            data.CompanyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
            data.CurrencyCulture = await SettingManager.GetCurrencyCultureAsync();
            data.HideLoadAt = input.HideLoadAt;
            data.QuoteGeneralTermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Quote.GeneralTermsAndConditions);

            data.QuoteGeneralTermsAndConditions = data.QuoteGeneralTermsAndConditions
                .Replace("{CompanyName}", data.CompanyName)
                .Replace("{CompanyNameUpperCase}", data.CompanyName.ToUpper());

            await SetQuoteCaptureHistory(input.QuoteId);

            var result = QuoteReportGenerator.GenerateReport(data);

            try
            {
                FileHelper.DeleteIfExists(data.SignaturePath);
            }
            catch (IOException)
            {
            }

            return result;
        }
        private async Task<string> GetLogoBase64String()
        {
            var tenant = await TenantManager.GetByIdAsync(AbpSession.GetTenantId());
            if (tenant.ReportsLogoId == null || tenant.ReportsLogoFileType == null)
            {
                return null;
            }

            var logoObject = await _binaryObjectManager.GetOrNullAsync(tenant.ReportsLogoId.Value);
            if (logoObject == null)
            {
                return null;
            }
            return "base64:" + Convert.ToBase64String(logoObject.Bytes);
        }

        private async Task<string> GetSignaturePictureTempPath(Guid? signaturePictureId)
        {
            var signatureFile = signaturePictureId.HasValue ? await _binaryObjectManager.GetOrNullAsync(signaturePictureId.Value) : null;
            if (signatureFile == null || signatureFile.Bytes.Length == 0)
            {
                return null;
            }

            var tempFilePath = Path.Combine(_appFolders.TempFileDownloadFolder, signatureFile.Id.ToString());
            try
            {
                File.WriteAllBytes(tempFilePath, signatureFile.Bytes);
            }
            catch (IOException)
            {
            }
            return tempFilePath;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<EmailQuoteReportDto> GetEmailQuoteReportModel(EntityDto input)
        {
            var user = await _userRepository.GetAll()
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    PhoneNumber = x.PhoneNumber
                })
                .FirstAsync();

            var quote = await _quoteRepository.GetAll()
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    ProjectName = x.Project.Name,
                    ContactEmail = x.Contact.Email,
                })
                .FirstAsync();

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate);
            subject = ReplaceEmailSubjectTemplateTokens(subject, quote.ProjectName);

            var body = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailBodyTemplate);
            body = ReplaceEmailBodyTemplateTokens(body, user.FirstName, user.LastName, user.PhoneNumber, companyName);

            return new EmailQuoteReportDto
            {
                QuoteId = input.Id,
                From = user.Email,
                To = quote.ContactEmail,
                CC = user.Email,
                Subject = subject,
                Body = body
            };
        }

        public static string ReplaceEmailSubjectTemplateTokens(string subjectTemplate, string quoteProjectName)
        {
            return subjectTemplate
                .Replace("{Project.Name}", quoteProjectName);
        }

        public static string ReplaceEmailBodyTemplateTokens(string bodyTemplate, string userFirstName, string userLastName, string userPhoneNumber, string companyName)
        {
            return bodyTemplate
                .Replace("{User.FirstName}", userFirstName)
                .Replace("{User.LastName}", userLastName)
                .Replace("{User.PhoneNumber}", userPhoneNumber)
                .Replace("{CompanyName}", companyName);
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task EmailQuoteReport(EmailQuoteReportDto input)
        {
            var reportBytes = await GetQuoteReport(new GetQuoteReportInput { QuoteId = input.QuoteId, HideLoadAt = input.HideLoadAt });
            var message = new MailMessage
            {
                From = new MailAddress(input.From),
                Subject = input.Subject,
                Body = input.Body,
                IsBodyHtml = false
            };
            foreach (var to in EmailHelper.SplitEmailAddresses(input.To))
            {
                message.To.Add(to);
            }
            foreach (var cc in EmailHelper.SplitEmailAddresses(input.CC))
            {
                message.CC.Add(cc);
            }

            var quoteDetails = await _quoteRepository.GetAll()
                .Where(x => x.Id == input.QuoteId)
                .Select(x => new
                {
                    ProjectName = x.Project.Name
                })
                .FirstAsync();

            var filename = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate);
            filename = ReplaceEmailSubjectTemplateTokens(filename, quoteDetails.ProjectName);
            filename = Utilities.RemoveInvalidFileNameChars(filename);
            filename += ".pdf";

            using (var stream = new MemoryStream(reportBytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                message.Attachments.Add(new Attachment(stream, filename));

                var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);
                var quote = await _quoteRepository.GetAsync(input.QuoteId);
                quote.LastQuoteEmailId = trackableEmailId;
                await _quoteEmailRepository.InsertAsync(new QuoteEmail
                {
                    EmailId = trackableEmailId,
                    QuoteId = quote.Id
                });
            }
            await SetQuoteCaptureHistory(input.QuoteId);
        }

        private async Task SetQuoteCaptureHistory(int quoteId)
        {
            var quote = await _quoteRepository.GetAsync(quoteId);
            quote.CaptureHistory = true;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task ActivateQuote(EntityDto input)
        {
            var quote = await _quoteRepository.GetAsync(input.Id);
            quote.InactivationDate = null;
            quote.Status = QuoteStatus.Active;
        }
    }
}
