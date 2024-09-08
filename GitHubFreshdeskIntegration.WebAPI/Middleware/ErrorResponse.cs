namespace GitHubFreshdeskIntegration.WebAPI.Middleware
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Detailed { get; set; }
        public string? ExceptionType { get; set; }
    }

}
