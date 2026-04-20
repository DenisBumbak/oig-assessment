using OIG.Application.Abstractions.Auth;
using OIG.Domain.Users;

namespace OIG.WebApi.Auth;

public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _http;

    public HttpCurrentUserAccessor(IHttpContextAccessor http) => _http = http;

    public UserId? UserId
    {
        get
        {
            var header = _http.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
            return Guid.TryParse(header, out var id) ? new UserId(id) : null;
        }
    }
}
