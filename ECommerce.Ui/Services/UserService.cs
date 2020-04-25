using ECommerce.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.Ui.Services
{
    public class UserService
    {
        private readonly string _route;
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _route = configuration["APIRoutes:User"];
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers(string role)
        {
            var response = await _httpClient.GetAsync($"{_route}/{role}");
            response.EnsureSuccessStatusCode();

            var users = await JsonSerializer.DeserializeAsync<IEnumerable<ApplicationUser>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return users;
        }
    }
}
