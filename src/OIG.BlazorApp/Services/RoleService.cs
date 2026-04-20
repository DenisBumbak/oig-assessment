namespace OIG.BlazorApp.Services;

public class RoleService(HttpClient http)
{
    public async Task<RolesListResponse?> GetListAsync(Guid? organizationId = null, string? search = null)
    {
        var url = "api/roles?";
        if (organizationId.HasValue) url += $"organizationId={organizationId}&";
        if (!string.IsNullOrWhiteSpace(search)) url += $"search={Uri.EscapeDataString(search)}&";
        return await http.GetFromJsonAsync<RolesListResponse>(url.TrimEnd('&', '?'));
    }

    public async Task<RoleDetailDto?> GetByIdAsync(Guid id)
        => await http.GetFromJsonAsync<RoleDetailDto>($"api/roles/{id}");

    public async Task<RoleDetailDto?> CreateAsync(CreateRoleRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/roles", req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<RoleDetailDto>();
    }

    public async Task UpdateAsync(Guid id, UpdateRoleRequest req)
    {
        var resp = await http.PutAsJsonAsync($"api/roles/{id}", req);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id)
    {
        var resp = await http.DeleteAsync($"api/roles/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
