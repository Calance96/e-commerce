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
    public class ProductService
    {
        private readonly string _route;
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _route = configuration["APIRoutes:Product"];
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            var response = await _httpClient.GetAsync(_route);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStreamAsync();
            var products = await JsonSerializer.DeserializeAsync<IEnumerable<Product>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return products;
        }

        public async Task<Product> GetById(long id)
        {
            var response = await _httpClient.GetAsync($"{_route}/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStreamAsync();
            var product = await JsonSerializer.DeserializeAsync<Product>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return product;
        }

        public async Task Update(Product product)
        {
            var data = new StringContent(JsonSerializer.Serialize<Product>(product), Encoding.UTF8, SD.CONTENT_JSON);
            await _httpClient.PutAsync($"{_route}/{product.Id}", data);
        }

        public async Task Add(Product product)
        {
            var data = new StringContent(JsonSerializer.Serialize<Product>(product), Encoding.UTF8, SD.CONTENT_JSON);
            await _httpClient.PostAsync(_route, data);
        }

        public async Task<Boolean> Delete(long id)
        {
            var response = await _httpClient.DeleteAsync($"{_route}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
    }
}
