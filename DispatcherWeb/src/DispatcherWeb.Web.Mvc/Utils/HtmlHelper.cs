using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Authorization;
using Abp.Collections.Extensions;
using DispatcherWeb.Application.Infrastructure.Utilities;
using DispatcherWeb.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Utils
{
    public static class HtmlHelper
    {
        public static IEnumerable<SelectListItem> GetEnumSelectListWithDefaults<T>(
            this IHtmlHelper htmlHelper,
            params T[] defaults
            ) where T : struct
        {
            var list = htmlHelper.GetEnumSelectList<T>().ToList();
            foreach (var item in list)
            {

                bool selected = defaults.Contains((T)Enum.Parse(typeof(T), item.Value));
                item.Selected = selected;
            }

            return list;
        }

        public static IEnumerable<SelectListItem> GetReportTypeSelectList(
            this IHtmlHelper htmlHelper,
            IPermissionChecker permissionChecker
            )
        {
            var list = htmlHelper.GetEnumSelectList<ReportType>().Where(x => permissionChecker.IsGranted(((ReportType)Int32.Parse(x.Value)).GetPermissionName())).ToList();

            return list;
        }

        public static IEnumerable<SelectListItem> GetBillingTermsSelectList(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.GetEnumSelectList<BillingTermsEnum>()
                .ToList()
                .OrderBy(x =>
                {
                    if (x.Text == BillingTermsEnum.Net5.GetDisplayName())
                    {
                        return "Net 05";
                    }
                    return x.Text;
                });
        }

        public static IEnumerable<SelectListItem> GetServiceTypeSelectList(this IHtmlHelper htmlHelper, ServiceType? selectedValue)
        {
            var options = htmlHelper.GetEnumSelectList<ServiceType>()
                .ToList()
                .WhereIf(selectedValue != ServiceType.System, x => x.Value != ((int)ServiceType.System).ToString())
                .ToList();

            if (selectedValue.HasValue)
            {
                var selectedOption = options.FirstOrDefault(x => x.Value == ((int)selectedValue).ToString());
                if (selectedOption != null)
                {
                    selectedOption.Selected = true;
                }
            }
            else
            {
                options.Insert(0, new SelectListItem());
            }

            return options;
        }

        public static IEnumerable<SelectListItem> GetEnumSelectListOrderedByName<T>(this IHtmlHelper htmlHelper) where T : struct
        {
            return htmlHelper.GetEnumSelectList<T>()
                .ToList()
                .OrderBy(x => x.Text);
        }

        public static IEnumerable<SelectListItem> GetEnumSelectListOrdered<T>(this IHtmlHelper htmlHelper) where T : struct
        {
            Type myEnumType = typeof(T);
            //var enumValues = Enum.GetValues(myEnumType).Cast<T>().ToArray();
            var enumNames = Enum.GetNames(myEnumType);
            int[] enumPositions = Array.ConvertAll(enumNames, n =>
            {
                OrderAttribute orderAttr = (OrderAttribute)myEnumType.GetField(n)
                    .GetCustomAttributes(typeof(OrderAttribute), false)[0];
                return orderAttr.Order;
            });

            var list = htmlHelper.GetEnumSelectList<T>().ToArray();
            Array.Sort(enumPositions, list);

            return list;
        }

        public static IEnumerable<SelectListItem> GetSelectListItems(this IHtmlHelper htmlHelper, int? id, string name)
        {
            var list = new List<SelectListItem>();

            if (id != null && id != 0)
            {
                SelectListItem item = new SelectListItem()
                {
                    Selected = true,
                    Text = Sanitize(name),
                    Value = id.Value.ToString()
                };
                list.Add(item);
            }
            return list;
        }

        public static IEnumerable<SelectListItem> GetSelectListFromEnumArray(this IHtmlHelper htmlHelper, Enum[] enumArray)
        {
            var list = new List<SelectListItem>();
            foreach (var item in enumArray)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.ToString(),
                    Text = item.ToString()
                });
            }

            return list;
        }

        public static HtmlString QuestionMarkIcon(this IHtmlHelper htmlHelper, string titleText, bool showIcon = true)
        {
            if (!showIcon)
            {
                return new HtmlString("");
            }
            return new HtmlString($"<i class=\"fas fa-question-circle\" title=\"{titleText}\"></i>");
        }

        public static HtmlString StyleDisplayNone(this IHtmlHelper htmlHelper, bool displayNone = true)
        {
            if (!displayNone)
            {
                return new HtmlString("");
            }

            return new HtmlString("style=\"display: none\"");
        }

        public static string JoinEnumAsString(this IHtmlHelper htmlHelper, params Enum[] enumArray)
        {
            return enumArray.Select(x => Convert.ChangeType(x, x.GetTypeCode())).JoinAsString(",");
        }

        public static string GetLowercaseStringBooleanValue(this IHtmlHelper htmlHelper, bool value) => value ? "true" : "false";

        public static string Sanitize(string html)
        {
            return String.IsNullOrEmpty(html) ? "" : html.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
        }

        public static string EscapeJsString(string val)
        {
            if (val == null)
            {
                return "null";
            }
            return "'" + val.Replace(@"\", @"\\").Replace(@"'", @"\'") + "'";
        }
    }
}
