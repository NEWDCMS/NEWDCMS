namespace DCMS.Services.Finances
{
    public interface ICalculationService
    {
        decimal GetCustomerOweCash(int? customerId);
    }
}