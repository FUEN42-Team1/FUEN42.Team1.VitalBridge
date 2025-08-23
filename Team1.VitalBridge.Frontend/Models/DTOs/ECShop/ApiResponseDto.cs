namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
    public class ApiResponseDto<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public string Error { get; set; }
    }
}
