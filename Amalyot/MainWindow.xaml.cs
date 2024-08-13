using Amalyot.Entities;
using Amalyot.Service.Categories;
using Amalyot.Service.Products;
using Amalyot.UIController;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing.Printing;
using System.Windows.Media;

namespace Amalyot
{
    public partial class MainWindow : Window
    {
        private List<CategoryUi> categories;
        private List<ProductUi> products;
        private ObservableCollection<Order> Orders = new ObservableCollection<Order>();
        int currentIndex = 0;
        int color = 0;
        int MaxviewCategories = 5;
        CategoryService categoryService = new CategoryService();
        ProductService productService = new ProductService();
        private string baseUrlCategory = "https://f74b-213-230-69-5.ngrok-free.app/api/category";
        private string baseUrlProduct = "https://f74b-213-230-69-5.ngrok-free.app/api/product/index";
        private readonly string categorylar = "https://f74b-213-230-69-5.ngrok-free.app/api/category/index";
        private readonly string category = $"https://f74b-213-230-69-5.ngrok-free.app/api/category/view/{1}";

        public MainWindow()
        {
            InitializeComponent();
            InitializeCategories();
            Orders = new ObservableCollection<Order>();
            LoadPrinters();

            DataContext = this;
        }

        
        private async void InitializeCategories()
        {
            categories = new List<CategoryUi>();
            var ct = new CategoryService();
            var categoryData = await ct.FetchCategoriesFromApi();
            foreach (var category in categoryData)
            {
                var categoryControl = new CategoryUi();
                categoryControl.LbCategory.Content = category.Name;
                categoryControl.Tag = category.Id;
                categoryControl.battonCategory.Tag = category.Id;
                categoryControl.MouseLeftButtonUp += CategoryControl_MouseLeftButtonUp;
                categories.Add(categoryControl);
                categoryControl.battonCategory.Click += Button_Click_Category;
            }
            currentIndex = 0;
            UpdateCategoryDisplay();

            // Select the first category by default
            if (categories.Count > 0)
            {
                categories[0].categoryBorder.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#EC7403"));
                FetchProducts((int)categories[0].Tag);
            }
        }

        private void CategoryControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is CategoryUi categoryControl && categoryControl.Tag is int categoryId)
            {
                FetchProducts(categoryId);
            }
        }

        private async void FetchProducts(int categoryId, string search = null)
        {
            products = new List<ProductUi>();
            var productData = await FetchProductsFromApi(categoryId, search);
            foreach (var product in productData)
            {
                var productControl = new ProductUi();
                productControl.ProductName.Content = product.Name;
                productControl.ProductDesc.Content = $"Price: {product.Price}";
                productControl.Tag = product.Id;

                products.Add(productControl);

                productControl.battonProduct.Tag = product.Id;
                productControl.battonProduct.Click += Button_Click_Product;
            }
            UpdateProductDisplay();
        }
        private async Task<List<Product>> FetchProductsFromApi(int categoryId, string search = null)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://f74b-213-230-69-5.ngrok-free.app/api/product/index?searchable={search}&category_id={categoryId}&page=1&perPage=200";


                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ApiResponse<Product>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return apiResult.Resoult.Data;
                }
                else
                {
                    MessageBox.Show("Failed to fetch products from API.");
                    return new List<Product>();
                }
            }
        }


        private void Button_Click_Category(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is int categoryId)
            {
                if (categoryId > 0 && categoryId <= categories.Count)
                {
                    if (color >= 0 && color < categories.Count)
                    {
                        categories[color].categoryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEFEFE"));
                    }

                    color = categoryId - 1;
                    categories[color].categoryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EC7403"));

                    FetchProducts(categoryId);
                }
            }
        }

        private void UpdateCategoryDisplay()
        {
            CategoryBar.Children.Clear();
            int categoriesToShow = Math.Min(MaxviewCategories, categories.Count - currentIndex);
            for (int i = currentIndex; i < currentIndex + categoriesToShow; i++)
            {
                CategoryBar.Children.Add(categories[i]);
            }
            PreviewCategorie.Visibility = currentIndex > 0 ? Visibility.Visible : Visibility.Hidden;
            otherCategories.Visibility = currentIndex + categoriesToShow < categories.Count ? Visibility.Visible : Visibility.Hidden;
        }

        private void UpdateProductDisplay()
        {
            productBar.Children.Clear();
            foreach (var product in products)
            {
                productBar.Children.Add(product);
            }
        }


        //private void UpdateUserBasketDisplay()
        //{
        //    // Clear current basket display
        //    BasketContainer.Children.Clear();

        //    decimal total = 0;



        //    foreach (var basketItem in Orders)
        //    {
        //        var basketControl = new BackedUi
        //        {
        //            ProductId = basketItem.ProductId,
        //            BasketlbName = { Content = basketItem.Name },
        //            BasketlbCount = { Content = basketItem.Count.ToString() },
        //            BasketlbTotal = { Content = (basketItem.Price * basketItem.Count).ToString() }
        //        };

        //        total += basketItem.Price * basketItem.Count;

        //        basketControl.BasketbtnMinus.Tag = basketItem.ProductId;
        //        basketControl.BasketbtnMinus.Click += Button_Click_Basket_Minus;
        //        basketControl.BasketbtnPlus.Tag = basketItem.ProductId;
        //        basketControl.BasketbtnPlus.Click += Button_Click_Basket_Plus;
        //        basketControl.BasketbtnTrash.Tag = basketItem.ProductId;
        //        basketControl.BasketbtnTrash.Click += Button_Click_Basket_Trash;

        //        BasketContainer.Children.Add(basketControl);

        //        lbTotalBalance.Content = total.ToString() + "$";
        //    }
        //}


        private async Task UpdateUserBasketDisplay()
        {
            dataGrid.ItemsSource = Orders;
        }




        private void Button_Click_Basket_Plus(object sender, RoutedEventArgs e)
        {
            decimal total = 0;
            if (sender is Button button && button.CommandParameter is Order korzinaEntity)
            {
                var existingBasketItem = Orders.FirstOrDefault(b => b.Id == korzinaEntity.Id);
                if (existingBasketItem != null)
                {
                     existingBasketItem.Count++;

                     UpdateUserBasketDisplay();
                    

                }

                foreach (var item in Orders)
                {
                    total += item.Price * item.Count;
                }

                lbTotalBalance.Content = $"{total.ToString()} $";

            }
        }

        private void Button_Click_Basket_Minus(object sender, RoutedEventArgs e)
        {
            decimal total = 0;
            if (sender is Button button && button.CommandParameter is Order korzinaEntity)
            {
                var mahsulot = Orders.FirstOrDefault(mah => mah.Id == korzinaEntity.Id);
                if (mahsulot.Count > 1)
                {
                    mahsulot.Count--;
                    UpdateUserBasketDisplay();
                }
                else if (mahsulot.Count == 1)
                {
                    Orders.Remove(korzinaEntity);
                    UpdateUserBasketDisplay();
                }

                foreach (var item in Orders)
                {
                    total += item.Price * item.Count;
                }

                lbTotalBalance.Content = $"{total.ToString()} $";

            }
        }

        private void Button_Click_Basket_Trash(object sender, RoutedEventArgs e)
        {
            decimal total = 0;
            if (sender is Button button && button.CommandParameter is Order productId)
            {
                var existingBasketItem = Orders.FirstOrDefault(b => b.Id == productId.Id);

                if (existingBasketItem != null)
                {
                    Orders.Remove(existingBasketItem);

                    UpdateUserBasketDisplay();
                }

                foreach (var item in Orders)
                {
                    total += item.Price * item.Count;
                }

                lbTotalBalance.Content = $"{total.ToString()} $";

            }
        }

        private void BasketControl_OnMinusClicked(object sender, int productId)
        {
            var basketItem = Orders.FirstOrDefault(item => item.ProductId == productId);
            if (basketItem != null && basketItem.Count > 1)
            {
                basketItem.Count--;
                UpdateUserBasketDisplay();
            }
        }

        private void BasketControl_OnPlusClicked(object sender, int productId)
        {
            var basketItem = Orders.FirstOrDefault(item => item.ProductId == productId);
            if (basketItem != null)
            {
                basketItem.Count++;
                UpdateUserBasketDisplay();
            }
        }

        private void BasketControl_OnTrashClicked(object sender, int productId)
        {
            var basketItem = Orders.FirstOrDefault(item => item.ProductId == productId);
            if (basketItem != null)
            {
                Orders.Remove(basketItem);
                UpdateUserBasketDisplay();
            }
        }

        private void Button_Click_Product(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            decimal total = 0;
            if (clickedButton != null && clickedButton.Tag is int productId)
            {
                var product = products.FirstOrDefault(p => (int)p.Tag == productId);

                if (product != null)
                {
                    var productName = product.ProductName.Content.ToString();
                    var productPrice = decimal.Parse(product.ProductDesc.Content.ToString().Replace("Price: ", ""));

                    var existingBasketItem = Orders.FirstOrDefault(b => b.ProductId == productId);
                    if (existingBasketItem != null)
                    {
                        existingBasketItem.Count++;
                    }
                    else
                    {
                        var userBasketItem = new Order
                        {
                            ProductId = productId,
                            Name = productName,
                            Count = 1,
                            Rate = 5.0,
                            Desc = 0,
                            Price = productPrice
                        };
                        Orders.Add(userBasketItem);
                    }
                }
            }
            foreach (var item in Orders)
            {
                total += item.Price * item.Count;
            }

            lbTotalBalance.Content = $"{total.ToString()} $";
            UpdateUserBasketDisplay();
        }


        private void ClickButtonGEtInvoice(object sender, RoutedEventArgs e)
        {
            if (printerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a printer.");
                return;
            }

            string selectedPrinterName = printerComboBox.SelectedItem.ToString();

            PrintInvoice(selectedPrinterName);
        }

        private void PrintInvoice(string printerName)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            printDocument.PrintPage += (sender, e) =>
            {
                System.Drawing.Font titleFont = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                System.Drawing.Font headerFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                System.Drawing.Font bodyFont = new System.Drawing.Font("Arial", 12);
                float lineHeight = bodyFont.GetHeight(e.Graphics) + 4;
                float x = 100;
                float y = 100;

                e.Graphics.DrawString("Invoice", titleFont, System.Drawing.Brushes.Black, x, y);
                y += lineHeight * 2;

                e.Graphics.DrawString("ProductId", headerFont, System.Drawing.Brushes.Black, x, y);
                e.Graphics.DrawString("Name", headerFont, System.Drawing.Brushes.Black, x + 100, y);
                e.Graphics.DrawString("Count", headerFont, System.Drawing.Brushes.Black, x + 250, y);
                e.Graphics.DrawString("Price", headerFont, System.Drawing.Brushes.Black, x + 550, y);
                y += lineHeight;

                foreach (var item in Orders)
                {
                    e.Graphics.DrawString(item.Id.ToString(), bodyFont, System.Drawing.Brushes.Black, x, y);
                    e.Graphics.DrawString(item.Name, bodyFont, System.Drawing.Brushes.Black, x + 100, y);
                    e.Graphics.DrawString(item.Count.ToString(), bodyFont, System.Drawing.Brushes.Black, x + 250, y);
                    e.Graphics.DrawString(item.Price.ToString("C"), bodyFont, System.Drawing.Brushes.Black, x + 550, y);
                    y += lineHeight;
                }

                y += lineHeight;
                decimal total = 0;
                foreach (var item in Orders)
                {
                    total += item.Price * item.Count;
                }
                e.Graphics.DrawString($"Total: {total:C}", headerFont, System.Drawing.Brushes.Black, x, y);
            };

            try
            {
                printDocument.Print();
                MessageBox.Show("Data has been sent to the printer.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to print: {ex.Message}");
            }
        }

        private void LoadPrinters()
        {
            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;

            if (printers.Count == 0)
            {
                MessageBox.Show("Unfortunately, you do not have a printer device connected.");
                return;
            }

            foreach (string printer in printers)
            {
                printerComboBox.Items.Add(printer);
            }

            if (printerComboBox.Items.Count > 0)
            {
                printerComboBox.SelectedIndex = 0;
            }
        }



        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void otherCategories_Click(object sender, RoutedEventArgs e)
        {
                if (currentIndex + MaxviewCategories < categories.Count)
                {
                    currentIndex++;
                    UpdateCategoryDisplay();
                }

        }
        private void PreviewCategorie_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                UpdateCategoryDisplay();
            }
        }

        private void PrinterClick(object sender, RoutedEventArgs e)
        {

        }
        private void brDragable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private async Task<List<Product>> FetchSearchProductsFromApi(string search = null)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://f74b-213-230-69-5.ngrok-free.app/api/product/index?searchable={search}&page=1&perPage=200";


                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ApiResponse<Product>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return apiResult.Resoult.Data;
                }
                else
                {
                    MessageBox.Show("Failed to fetch products from API.");
                    return new List<Product>();
                }
            }
        }

        private async void SerarchButton(object sender, RoutedEventArgs e)
        {

            if(Searchtext.Text != null)
            {
                productBar.Children.Clear();
                var productData = await FetchSearchProductsFromApi(Searchtext.Text);

                foreach (var product in productData)
                {
                    var productControl = new ProductUi();
                    productControl.ProductName.Content = product.Name;
                    productControl.ProductDesc.Content = $"Price: {product.Price}";
                    productControl.Tag = product.Id;

                    products.Add(productControl);

                    productControl.battonProduct.Tag = product.Id;
                    productControl.battonProduct.Click += Button_Click_Product;
                }
                UpdateProductDisplay();
            }
        }
    }
}
