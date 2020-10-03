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
    public class OrderService
    {
        private readonly string _route;
        private readonly HttpClient _httpClient;

        public OrderService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("api");
            _route = configuration["APIRoutes:Order"];
        }

        public async Task<IEnumerable<Order>> GetAllOrders(string status)
        {
            var response = await _httpClient.GetAsync($"{_route}/{status}");
            IEnumerable<Order> orders = Enumerable.Empty<Order>();

            if (response.IsSuccessStatusCode)
            {
                orders = await JsonSerializer.DeserializeAsync<IEnumerable<Order>>(
                   await response.Content.ReadAsStreamAsync(),
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                   );
            }

            return orders;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersForUserId(string userId, string status)
        {
            Console.WriteLine($"Token: {_httpClient.DefaultRequestHeaders.Authorization}");
            var response = await _httpClient.GetAsync($"{_route}/user/{userId}/{status}");
            IEnumerable<Order> orders = Enumerable.Empty<Order>();

            if (response.IsSuccessStatusCode)
            {
                orders = await JsonSerializer.DeserializeAsync<IEnumerable<Order>>(
                    await response.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
            }
            return orders;
        }

        public async Task<Order> GetOrderSummaryByOrderId(long orderId)
        {
            var response = await _httpClient.GetAsync($"{_route}/summary/{orderId}");

            if (response.IsSuccessStatusCode)
            {
                var order = await JsonSerializer.DeserializeAsync<Order>(
                                        await response.Content.ReadAsStreamAsync(),
                                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return order;
            }
            else
            {
                return null;
            }
        }

        public async Task<OrderDetailsVM> GetOrderDetailsByOrderId(long orderId)
        {
            var response = await _httpClient.GetAsync($"{_route}/details/{orderId}");

            if (response.IsSuccessStatusCode)
            {
                var orderDetailsVM = await JsonSerializer.DeserializeAsync<OrderDetailsVM>(
                                        await response.Content.ReadAsStreamAsync(),
                                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return orderDetailsVM;
            }
            else
            {
                return null;
            }
        }

        public async Task<Order> Create(ShoppingCartVM shoppingCart)
        {
            var data = new StringContent(JsonSerializer.Serialize<ShoppingCartVM>(shoppingCart), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await _httpClient.PostAsync(_route, data);
            Order order = null;

            if (response.IsSuccessStatusCode)
            {
                order = await JsonSerializer.DeserializeAsync<Order>(await response.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }

            return order;
        }

        public async Task<Boolean> Update(Order order)
        {
            var data = new StringContent(JsonSerializer.Serialize<Order>(order), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await _httpClient.PutAsync($"{_route}/{order.Id}", data);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
