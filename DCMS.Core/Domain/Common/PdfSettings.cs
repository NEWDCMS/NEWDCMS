using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Common
{

    public class PdfSettings : ISettings
    {

        public int LogoPictureId { get; set; }


        public bool LetterPageSizeEnabled { get; set; }

        public bool RenderOrderNotes { get; set; }


        public bool DisablePdfInvoicesForPendingOrders { get; set; }


        public string FontFileName { get; set; }

        public string InvoiceFooterTextColumn1 { get; set; }

        public string InvoiceFooterTextColumn2 { get; set; }
    }
}