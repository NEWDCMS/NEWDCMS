using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using System.Collections.Generic;

namespace DCMS.Services.Configuration
{
    public interface IPrintTemplateService
    {
        void DeletePrintTemplate(PrintTemplate printTemplates);
        IList<PrintTemplate> GetAllPrintTemplates(int? store);
        IPagedList<PrintTemplate> GetAllPrintTemplates(int? store, int? type, int pageIndex = 0, int pageSize = int.MaxValue);
        PrintTemplate GetPrintTemplateById(int? store, int printTemplatesId);
        IList<PrintTemplate> GetPrintTemplatesByIds(int[] sIds);
        void InsertPrintTemplate(PrintTemplate printTemplates);
        void UpdatePrintTemplate(PrintTemplate printTemplates);
    }
}