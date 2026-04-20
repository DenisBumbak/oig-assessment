namespace OIG.BlazorApp.Services;

public class AuthHeaderHandler(CurrentUserState currentUser) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId.HasValue)
            request.Headers.Add("X-User-Id", currentUser.UserId.Value.ToString());

        return base.SendAsync(request, cancellationToken);
    }
}
