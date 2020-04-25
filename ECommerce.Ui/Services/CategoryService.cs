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
    public class CategoryService
    {
        private readonly string _route;
        private readonly HttpClient _httpClient;

        public CategoryService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _route = configuration["APIRoutes:Category"];
        }

        public async Task<IEnumerable<Category>> GetAll()
        {
            var response = await _httpClient.GetAsync(_route);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStreamAsync();
            var categories = await JsonSerializer.DeserializeAsync<IEnumerable<Category>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return categories;
        } 

        public async Task<Category> GetById(long id)
        {
            var response = await _httpClient.GetAsync($"{_route}/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStreamAsync();
            var category = await JsonSerializer.DeserializeAsync<Category>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return category;
        }

        public async Task Update(Category category)
        {
            var data = new StringContent(JsonSerializer.Serialize<Category>(category), Encoding.UTF8, SD.CONTENT_JSON);
            await _httpClient.PutAsync($"{_route}/{category.Id}", data);
        }

        public async Task Add(Category category)
        {
            var data = new StringContent(JsonSerializer.Serialize<Category>(category), Encoding.UTF8, SD.CONTENT_JSON);
            await _httpClient.PostAsync(_route, data);
        }

        public async Task Delete(long id)
        {
            await _httpClient.DeleteAsync($"{_route}/{id}");
        }
    }
}
