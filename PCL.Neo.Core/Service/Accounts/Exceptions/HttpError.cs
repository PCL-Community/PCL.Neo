using System.Net;

namespace PCL.Neo.Core.Service.Accounts.Exceptions;

public record HttpError(
    HttpStatusCode? StatusCode,
    string Message,
    string? Content = null,
    Exception? Exception = null);
