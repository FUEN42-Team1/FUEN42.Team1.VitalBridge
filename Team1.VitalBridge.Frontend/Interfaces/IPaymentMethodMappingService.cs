using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Interfaces
{
    public interface IPaymentMethodMappingService
    {
        string GetEcpayChoosePayment(string paymentMethodName);
        string GetEcpayChoosePaymentById(int paymentMethodId, AppDbContext context);
    }
}
