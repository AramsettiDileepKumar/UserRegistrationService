using System.Text.Json.Serialization;

namespace UserManagementService.Model
{
    public class ResponseModel<T>
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
