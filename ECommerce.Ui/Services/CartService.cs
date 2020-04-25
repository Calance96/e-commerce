using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.Ui.Services
{
    public class CartService
    {
        private readonly string _route;
        private readonly HttpClient _httpClient;

        public CartService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _route = configuration["APIRoutes:Cart"];
        }

        public async Task<IEnumerable<CartItem>> GetAll(string userId)
        {
            var response = await _httpClient.GetAsync($"{_route}/{userId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStreamAsync();
            var cartItems = await JsonSerializer.DeserializeAsync<IEnumerable<CartItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return cartItems;
        }

        public async Task<CartItem> GetCartItemByUserIdAndProductId(CartItem cartItem)
        {
            var response = await _httpClient.GetAsync($"{_route}/{cartItem.UserId}/{cartItem.ProductId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                var item = await JsonSerializer.DeserializeAsync<CartItem>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return item;
            } 
            else
            {
                return null;
            }
        }

        public async Task Update(CartItem cartItem)
        {
            var data = new StringContent(JsonSerializer.Serialize<CartItem>(cartItem), Encoding.UTF8, AppConstant.CONTENT_JSON);
            await _httpClient.PutAsync($"{_route}/{cartItem.Id}", data);
        }

        public async Task Add(CartItem cartItem)
        {
            var data = new StringContent(JsonSerializer.Serialize<CartItem>(cartItem), Encoding.Default, AppConstant.CONTENT_JSON);
            await _httpClient.PostAsync(_route, data);
        }

        public async Task Delete(long id)
        {
            await _httpClient.DeleteAsync($"{_route}/{id}");
        }

        public async Task<Int32> GetItemsCount(string userId)
        {
            var response = await _httpClient.GetAsync($"{_route}/count/{userId}");
            response.EnsureSuccessStatusCode();

            var count = await JsonSerializer.DeserializeAsync<Int32>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return count;
        }
    }
}
