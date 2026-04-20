namespace OIG.BlazorApp.Services;

public class OrganizationService(HttpClient http)
{
    public async Task<OrganizationHierarchyResponse?> GetHierarchyAsync()
        => await http.GetFromJsonAsync<OrganizationHierarchyResponse>("api/organizations/hierarchy");

    public async Task<OrganizationDto?> GetByIdAsync(Guid id)
        => await http.GetFromJsonAsync<OrganizationDto>($"api/organizations/{id}");

    public async Task<OrganizationDto?> CreateAsync(CreateOrganizationRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/organizations", req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<OrganizationDto>();
    }

    public async Task<OrganizationDto?> UpdateAsync(Guid id, UpdateOrganizationRequest req)
    {
        var resp = await http.PutAsJsonAsync($"api/organizations/{id}", req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<OrganizationDto>();
    }

    public async Task DeleteAsync(Guid id)
    {
        var resp = await http.DeleteAsync($"api/organizations/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
