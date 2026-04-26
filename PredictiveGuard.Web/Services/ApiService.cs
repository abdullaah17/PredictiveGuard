using System.Net.Http.Json;
using PredictiveGuard.Data.Models;

namespace PredictiveGuard.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }

    // Assets
    public async Task<List<Asset>> GetAssetsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Asset>>("api/asset") ?? new();
    }

    public async Task<Asset> GetAssetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Asset>($"api/asset/{id}");
    }

    public async Task<Asset> CreateAssetAsync(string name, string location, string type)
    {
        var dto = new { name, location, type };
        var response = await _httpClient.PostAsJsonAsync("api/asset", dto);
        return await response.Content.ReadFromJsonAsync<Asset>();
    }

    // Sensor Readings
    public async Task<List<SensorReading>> GetSensorReadingsAsync(int assetId, int hours = 24)
    {
        return await _httpClient.GetFromJsonAsync<List<SensorReading>>($"api/sensorreading/{assetId}?hours={hours}") ?? new();
    }

    public async Task<SensorReading> CreateSensorReadingAsync(int assetId, double temp, double vibration, double load)
    {
        var dto = new { assetId, temperature = temp, vibration, load };
        var response = await _httpClient.PostAsJsonAsync("api/sensorreading", dto);
        return await response.Content.ReadFromJsonAsync<SensorReading>();
    }

    // Maintenance Tickets
    public async Task<List<MaintenanceTicket>> GetTicketsAsync(string status = null)
    {
        var url = "api/maintenanceticket";
        if (!string.IsNullOrEmpty(status))
            url += $"?status={status}";
        return await _httpClient.GetFromJsonAsync<List<MaintenanceTicket>>(url) ?? new();
    }

    public async Task<MaintenanceTicket> GetTicketByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<MaintenanceTicket>($"api/maintenanceticket/{id}");
    }

    public async Task<MaintenanceTicket> AssignTicketAsync(int ticketId, int userId)
    {
        var dto = new { userId };
        var response = await _httpClient.PatchAsJsonAsync($"api/maintenanceticket/{ticketId}/assign", dto);
        return await response.Content.ReadFromJsonAsync<MaintenanceTicket>();
    }

    public async Task<MaintenanceTicket> UpdateTicketStatusAsync(int ticketId, string status)
    {
        var dto = new { status };
        var response = await _httpClient.PatchAsJsonAsync($"api/maintenanceticket/{ticketId}/status", dto);
        return await response.Content.ReadFromJsonAsync<MaintenanceTicket>();
    }

    // Users
    public async Task<List<User>> GetUsersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<User>>("api/user") ?? new();
    }

    public async Task<User> CreateUserAsync(string googleId, string email, string fullName, string profilePictureUrl)
    {
        var dto = new { googleId, email, fullName, profilePictureUrl };
        var response = await _httpClient.PostAsJsonAsync("api/user", dto);
        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<User>($"api/user/{id}");
    }

    // Team Members
    public async Task<List<AssetTeamMember>> GetTeamMembersAsync(int assetId)
    {
        return await _httpClient.GetFromJsonAsync<List<AssetTeamMember>>($"api/assetteammember/{assetId}") ?? new();
    }

    public async Task<AssetTeamMember> AddTeamMemberAsync(int assetId, int userId, string role = "Engineer")
    {
        var dto = new { assetId, userId, role };
        var response = await _httpClient.PostAsJsonAsync("api/assetteammember", dto);
        return await response.Content.ReadFromJsonAsync<AssetTeamMember>();
    }
}
