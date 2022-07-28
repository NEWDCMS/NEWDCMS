using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Threading.Tasks;

namespace DCMS.Web.Framework
{
    public class NullView : IView
    {
        public static readonly NullView Instance = new NullView();

        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.CompletedTask;
        }
    }
}