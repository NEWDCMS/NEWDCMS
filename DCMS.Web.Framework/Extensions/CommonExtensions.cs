using Humanizer;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DCMS.Web.Framework.Extensions
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// 是否不能进行实际选择
        /// </summary>
        /// <param name="items">Items</param>
        /// <param name="ignoreZeroValue">是否应忽略值为“0”的项</param>
        /// <returns>是否不能进行真正的选择</returns>
        public static bool SelectionIsNotPossible(this IList<SelectListItem> items, bool ignoreZeroValue = true)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            //忽略值为“0”的项？通常是“全选”之类的
            return items.Count(x => !ignoreZeroValue || !x.Value.ToString().Equals("0")) < 2;
        }

        /// <summary>
        /// 日期时间的相对格式（例如2小时前、一个月前）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public static string RelativeFormat(this DateTime source, string languageCode = "zh-CN")
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - source.Ticks);
            var delta = ts.TotalSeconds;

            CultureInfo culture = null;
            try
            {
                culture = new CultureInfo(languageCode);
            }
            catch (CultureNotFoundException)
            {
                culture = new CultureInfo("zh-CN");
            }
            return TimeSpan.FromSeconds(delta).Humanize(precision: 1, culture: culture, maxUnit: TimeUnit.Year);
        }
    }
}