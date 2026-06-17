namespace RaphCare.Client.Contracts;

public class ApiException(string message, int? statusCode = null, string? responseBody = null, Exception? inner = null) : Exception(message, inner)
{
    public int? StatusCode { get; } = statusCode;
    public string? ResponseBody { get; } = responseBody;
}
