using ECommerce.Models.DTO;
using ECommerce.Models.ViewModels;
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
    public class AuthService
    {
        private readonly string _route;
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _route = configuration["APIRoutes:Auth"];
        }

        public async Task<AuthResult> Login(LoginViewModel loginInput)
        {
            var body = JsonSerializer.Serialize<LoginViewModel>(loginInput);
            var data = new StringContent(body, Encoding.UTF8, SD.CONTENT_JSON);
            var response = await _httpClient.PostAsync($"{_route}/login", data);

            var authResult = await JsonSerializer.DeserializeAsync<AuthResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return authResult;
        }

        public async Task<AuthResult> Register(RegisterViewModel registerInput)
        {
            var data = new StringContent(JsonSerializer.Serialize<RegisterViewModel>(registerInput), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await _httpClient.PostAsync($"{_route}/register", data);

            response.EnsureSuccessStatusCode();

            var authResult = await JsonSerializer.DeserializeAsync<AuthResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return authResult;
        }

        public async Task<Boolean> ChangePassword(ChangePasswordModel changePasswordInput)
        {
            var data = new StringContent(JsonSerializer.Serialize<ChangePasswordModel>(changePasswordInput), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await _httpClient.PostAsync($"{_route}/password_change", data);

            response.EnsureSuccessStatusCode();

            var result = await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
    }
}
