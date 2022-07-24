using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.Entities;

namespace Veg.Client.Components
{
    public class AddProductBase : ComponentBase
    {
        [Inject]
        public ProductsClient ProductsClient { get; set; }
        [Inject]
        public ImagesClient ImagesClient { get; set; }
        [Parameter]
        public string Barcode { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var brands = await ProductsClient.GetBrandsForSelection();
            foreach (Brand brand in brands)
            {
                BrandCompleteItems.Add(new AutoCompleteItem<string>() { Value = brand.Name, Text = brand.Name });
            }

            CategoriesSelect = await ProductsClient.GetCategoriesForAutoSelect();

            var defaultStores = await ProductsClient.GetStoresForSelection();
            foreach (Store store in defaultStores)
            {
                AvailbleAtDefaultStores.Add(new DefaultStoreSelection() { Store = store, IsChecked = false });
            }

            var autocompleteStores = await ProductsClient.GetStoresForAutoComplete();
            foreach (Store store in autocompleteStores)
            {
                StoreCompleteItems.Add(new AutoCompleteItem<string>() { Value = store.Name, Text = store.Name });
            }
            AvailbleAtExtraStores.Add(new DefaultStoreSelection() { IsChecked = false, StoreName = "" });
            await base.OnInitializedAsync();
        }

        public ProductCategory SelectedCategory { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public void SelectedCategoryChanged(object selectedValue)
        {
            if (selectedValue is ProductCategory selectedValueAsCategory)
            {
                SelectedCategory = selectedValueAsCategory;
            }
        }
        public bool Saving { get; set; }
        public async Task AddProductPost()
        {
            if (!string.IsNullOrWhiteSpace(BrandName)
                && !string.IsNullOrWhiteSpace(ProductName)
                && CategoriesSelect != null
                && !AddedSucces && !Saving)
            {
                Saving = true;
                Product productCreating = new Product();
                productCreating.Name = ProductName;
                productCreating.Brand = new Brand() { Name = BrandName };
                productCreating.IsVegan = VeganOrVega == "Vegan";
                productCreating.IsVegetarian = VeganOrVega == "Vega";
                productCreating.StoresAvailable = new List<AvailableAt>();
                productCreating.CategoryId = SelectedCategory.ID;
                foreach (var atStore in AvailbleAtDefaultStores.Where(b => b.IsChecked).Select(b => b.Store))
                {
                    productCreating.StoresAvailable.Add(new AvailableAt() { Store = atStore });
                }
                foreach (var atStore in AvailbleAtExtraStores.Where(b => b.IsChecked).Select(b => b.StoreName))
                {
                    productCreating.StoresAvailable.Add(new AvailableAt() { Store = new Store() { Name = atStore } });
                }
                productCreating.Tags = new List<ProductTagLink>();
                foreach (string tag in Tags)
                {
                    productCreating.Tags.Add(new ProductTagLink() { Tag = new Tag() { Name = tag } });
                }

                //Save image and get name if
                if (ProductImage != null)
                {
                    string imagePathOnServer = await PostImage();
                    productCreating.ProductImage = imagePathOnServer;
                }
                var addItem = await ProductsClient.AddAsync(productCreating);
                if (!string.IsNullOrWhiteSpace(Barcode))
                {
                    await ProductsClient.AddBarcodeToProduct(addItem, Barcode);
                }
                AddedSucces = true;
                NavigationManager.NavigateTo($"Product/{addItem.ID.ToString("D")}");
            }
        }

        private async Task<string> PostImage()
        {
            return await ImagesClient.UploadImage(Convert.FromBase64String(ProductImage.Data));
        }

        internal List<DefaultStoreSelection> AvailbleAtDefaultStores { get; set; } = new List<DefaultStoreSelection>();
        internal List<DefaultStoreSelection> AvailbleAtExtraStores { get; set; } = new List<DefaultStoreSelection>();

        public List<AutoCompleteItem<string>> BrandCompleteItems { get; set; } = new List<AutoCompleteItem<string>>();
        public List<AutoCompleteItem<string>> CategoryCompleteItems { get; set; } = new List<AutoCompleteItem<string>>();

        public List<AutoCompleteItem<string>> StoreCompleteItems { get; set; } = new List<AutoCompleteItem<string>>();


        public string ExtraStoreName { get; set; } = "";
        public string ProductName { get; set; }
        public string BrandName { get; set; }


        public void BrandSearchChanged(string newSearch)
        {
            if (!BrandCompleteItems.Any(b => b.Text == newSearch) && newSearch != null)
            {
                BrandCompleteItems.Add(new AutoCompleteItem<string>() { Value = newSearch, Text = newSearch });
            }
        }

        public void ValueOfBrandNameChanged(object newValue)
        {
            BrandName = (string)newValue;
        }

        public Entities.Image ProductImage { get; set; }
        public bool AddedSucces { get; set; }
        public ICollection<ProductCategory> CategoriesSelect { get; private set; }

        public string VeganOrVega = "Vega";

        public string TagName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        public void AddTag()
        {
            if (Tags.Where(b => b.Trim().ToUpperInvariant() == TagName.Trim().ToUpperInvariant()).Count() == 0 && TagName.Trim().Length > 1)
            {
                Tags.Add(TagName);
            }
            TagName = "";
        }
        public void CheckedChanged(bool selected, DefaultStoreSelection selection)
        {
            selection.IsChecked = selected;

            if (!AvailbleAtExtraStores.Any(b => string.IsNullOrWhiteSpace(b.StoreName)))
            {
                AvailbleAtExtraStores.Add(new DefaultStoreSelection() { IsChecked = false, Store = new Store() { Name = "" }, StoreName = "" });
            }
        }

        public void SearchChanged(string newSearch)
        {
            if (!StoreCompleteItems.Any(b => b.Text == newSearch) && newSearch != null)
            {
                StoreCompleteItems.Add(new AutoCompleteItem<string>() { Value = newSearch, Text = newSearch });
            }
        }

        public void ValueOfStoreNameChanged(DefaultStoreSelection atStore, object newValue)
        {
            atStore.StoreName = (string)newValue;
            atStore.IsChecked = true;
            if (!AvailbleAtExtraStores.Any(b => string.IsNullOrWhiteSpace(b.StoreName)))
            {
                AvailbleAtExtraStores.Add(new DefaultStoreSelection() { IsChecked = false, Store = new Store() { Name = "" }, StoreName = "" });
            }
        }

        protected async Task OnImageChanged(FileChangedEventArgs e)
        {
            try
            {
                foreach (var file in e.Files)
                {
                    // A stream is going to be the destination stream we're writing to.                
                    using (var stream = new MemoryStream())
                    {
                        // Here we're telling the FileEdit where to write the upload result
                        await file.WriteToStreamAsync(stream);

                        // Once we reach this line it means the file is fully uploaded.
                        // In this case we're going to offset to the beginning of file
                        // so we can read it.
                        stream.Seek(0, SeekOrigin.Begin);

                        //Convert image
                        Veg.Entities.Image image = new Veg.Entities.Image();
                        image.Data = Convert.ToBase64String(stream.ToArray());
                        ProductImage = image;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
            finally
            {
                this.StateHasChanged();
            }
        }


        public class DefaultStoreSelection
        {
            public Store Store { get; set; }
            public string StoreName { get; set; }
            public bool IsChecked { get; set; }
        }

    }
}
