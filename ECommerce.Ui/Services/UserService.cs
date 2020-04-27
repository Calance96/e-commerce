using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        public async Task<ApplicationUser> GetUserById(string userId)
        {
            var response = await _httpClient.GetAsync($"{_route}/info/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var user = await JsonSerializer.DeserializeAsync<ApplicationUser>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return user;
            } 
            else
            {
                return null;
            }
        }

        public async Task Update(ApplicationUser user)
        {
            var data = new StringContent(JsonSerializer.Serialize<ApplicationUser>(user), Encoding.UTF8, SD.CONTENT_JSON);
            await _httpClient.PutAsync($"{_route}", data);
        }
    }
}
