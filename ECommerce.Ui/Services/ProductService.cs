using ECommerce.Models;
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
    public class ProductService
    {
        private readonly string _route;
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _route = configuration["APIRoutes:Product"];
        }

        public async Task<IEnumerable<ProductViewModel>> GetAll()
        {
            var response = await _httpClient.GetAsync(_route);
            var products = Enumerable.Empty<ProductViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductViewModel>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return products;
        }

        public async Task<ProductViewModel> GetById(long id)
        {
            var response = await _httpClient.GetAsync($"{_route}/{id}");
            ProductViewModel productVM = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                productVM = await JsonSerializer.DeserializeAsync<ProductViewModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return productVM;
        }

        public async Task<Boolean> Update(ProductViewModel productVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<ProductViewModel>(productVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await _httpClient.PutAsync($"{_route}/{productVM.Product.Id}", data);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Boolean> Add(ProductViewModel productVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<ProductViewModel>(productVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await _httpClient.PostAsync(_route, data);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
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
