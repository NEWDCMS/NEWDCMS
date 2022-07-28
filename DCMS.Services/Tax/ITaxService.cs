namespace DCMS.Services.Tax
{
    public interface ITaxService
    {
        decimal GetTaxRateAmount(int? store, decimal amount);
    }
}