using Amalyot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Amalyot.Service.Categories
{
    public class CategoryService
    {
        private string baseUrlCategory = "https://f74b-213-230-69-5.ngrok-free.app/api/category";
        private string baseUrlProduct = "https://f74b-213-230-69-5.ngrok-free.app/api/product";
        private readonly string categorylar = "https://f74b-213-230-69-5.ngrok-free.app/api/category/index";
        private readonly string product = $"https://f74b-213-230-69-5.ngrok-free.app/api/product/index";
        public async Task<List<Category>> FetchCategoriesFromApi()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(categorylar);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ApiResponse<Category>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return apiResult.Resoult.Data;
                }
                else
                {
                    MessageBox.Show("Failed to fetch categories from API.");
                    return new List<Category>();
                }
            }
        }
    }
}
