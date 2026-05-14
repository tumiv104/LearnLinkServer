
namespace Application.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }

        public ApiResponse(bool success, string message, T? data = default, string? errorCode = null)
        {
            Success = success;
            Message = message;
            Data = data;
            ErrorCode = errorCode;  
        }
    }
}
