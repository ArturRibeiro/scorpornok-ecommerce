namespace Ecommerce.Scenarios.Integration.Spec.Tests.Hooks;

public class Result<T>(T value, HttpResponseMessage ensureSuccessStatusCode)
    where T : class
{
    public T Value { get; set; } = value;
    public HttpResponseMessage EnsureSuccessStatusCode { get; set; } = ensureSuccessStatusCode;
}