namespace DCMS.ViewModel.Models
{
    public class TableResultModel
    {
        public int? draw { get; set; } = 0;
        public int? recordsTotal { get; set; } = 0;
        public int? recordsFiltered { get; set; } = 0;
        public object data { get; set; }
    }
}