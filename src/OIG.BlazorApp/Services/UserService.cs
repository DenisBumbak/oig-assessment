namespace OIG.BlazorApp.Services;

public class UserService(HttpClient http)
{
    public async Task<UsersListResponse?> GetListAsync(Guid? organizationId = null, string? search = null)
    {
        var url = "api/users?";
        if (organizationId.HasValue) url += $"organizationId={organizationId}&";
        if (!string.IsNullOrWhiteSpace(search)) url += $"search={Uri.EscapeDataString(search)}&";
        return await http.GetFromJsonAsync<UsersListResponse>(url.TrimEnd('&', '?'));
    }

    public async Task<UserDetailDto?> GetByIdAsync(Guid id)
        => await http.GetFromJsonAsync<UserDetailDto>($"api/users/{id}");

    public async Task<CreateUserResponse?> CreateAsync(CreateUserRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/users", req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<CreateUserResponse>();
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest req)
    {
        var resp = await http.PutAsJsonAsync($"api/users/{id}", req);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id)
    {
        var resp = await http.DeleteAsync($"api/users/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
