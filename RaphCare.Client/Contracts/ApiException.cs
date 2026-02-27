namespace RaphCare.Client.Contracts;

public class ApiException : Exception
{
    public int? StatusCode { get; }
    public string? ResponseBody { get; }

    public ApiException(string message, int? statusCode = null, string? responseBody = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
