using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Veg.API.Client;
using Veg.Client.Services;
using Veg.Entities;

namespace Veg.Client.Pages
{
    public class ProductsPageBase : BasePage
    {
        [Inject]
        public VegImageService ImageService { get; set; }

        [Parameter]
        public string ProductSearchText { get; set; }
        [Parameter]
        public string SelectedCategoryId { get; set; }
        [Parameter]
        public bool VeganOrVega { get; set; }
        [Parameter]
        public string? CurrentSort { get; set; }
        [Parameter]
        public int CurrentPage { get; set; }

        [Inject]
        public ProductsClient ProductsClient { get; set; }

        [Parameter]
        public string LimitResultOptionsJson { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }
        protected override async Task OnInitializedAsync()
        {
            if (LimitResultOptionsJson == null)
            {
                LimitResultOptions = new LimitResultOptions();
                LimitResultOptions.Sorting = "default";
            }
            else
            {
                LimitResultOptions = JsonConvert.DeserializeObject<LimitResultOptions>(LimitResultOptionsJson);
                if (LimitResultOptions == null)
                {
                    LimitResultOptions = new LimitResultOptions();
                }
            }
            SelectedSorting = LimitResultOptions.Sorting;
            await base.OnInitializedAsync();
            CategoriesSelect = (await ProductsClient.GetCategoriesForAutoSelect()).ToList();
            CategoriesSelect.Insert(0, new ProductCategory() { ID = Guid.Empty, Name = "Alle categorieën" });
            if (VeganOrVega)
            {
                VeganOrVegaSelected = "Alleen vegan";
            }
            else
            {
                VeganOrVegaSelected = "Vega of vegan";
            }

            StateHasChanged();
        }


        private bool alreadySet = false;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!alreadySet && CategoriesSelect != null)
            {
                if (Guid.TryParse(SelectedCategoryId, out Guid categoryIdAsGuid) && SelectedCategory == null)
                {
                    SelectedCategory = CategoriesSelect.FirstOrDefault(b => b.ID == categoryIdAsGuid);
                }
                alreadySet = true;
                /*if (!string.IsNullOrWhiteSpace(ProductSearchText) || SelectedCategory != null)
                {
                    await SearchNow();
                }*/
            }

        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (!alreadySet && CategoriesSelect != null)
            {

                if (Guid.TryParse(SelectedCategoryId, out Guid categoryIdAsGuid) && SelectedCategory == null)
                {
                    SelectedCategory = CategoriesSelect.FirstOrDefault(b => b.ID == categoryIdAsGuid);
                }
                alreadySet = true;

            }
            if (NavigationManager.ToBaseRelativePath(NavigationManager.Uri.ToString()).ToString() == "")
            {
                SelectedCategory = null;
                LimitResultOptions?.ProductCategories?.Clear();
                LimitResultOptions?.Brands?.Clear();
                ProductSearchText = null;
                CurrentSearchResults = null;
            }
            else
            {

                await SearchNow();
            }

        }

        public List<ProductCategory> CategoriesSelect { get; set; }

        ProductCategory _selectedCategory;
        public ProductCategory SelectedCategory
        {
            get
            {
                return _selectedCategory;
            }
            set
            {
                _selectedCategory = value;

            }
        }

        public LimitResultOptions LimitResultOptions { get; set; }

        public string VeganOrVegaSelected { get; set; }
        public SearchResultsViewmodel CurrentSearchResults { get; set; }
        public bool Searching { get; set; }
        public string CurrentUrlWithoutPage { get; set; }

        public void SelectedCategoryChanged(object selectedValue)
        {
            if (selectedValue is ProductCategory selectedValueAsCategory)
            {
                LimitResultOptions.ProductCategories.Clear();
                /*  if (SelectedCategory != null)
                  {
                      LimitResultOptions.ProductCategories.Remove(SelectedCategory.ID);
                  }*/
                SelectedCategory = selectedValueAsCategory;
                /*  if (SelectedCategory.ID != Guid.Empty)
                  {
                      LimitResultOptions.ProductCategories.Add(SelectedCategory.ID);
                  }*/
                SelectedCategoryId = selectedValueAsCategory.ID.ToString("D");
            }
        }
        protected async Task SelectedValueSortingChanged(string newValue)
        {
            SelectedSorting = newValue;
            LimitResultOptions.Sorting = SelectedSorting;
            await SearchClicked();
        }

        public string SelectedSorting { get; set; }

        public async Task SearchClicked()
        {
            CurrentPage = 0;
            NavigateToUpdatedLink();
            await SearchNow();
            await ScrollToElementId("searchResult");
        }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        private async Task ScrollToElementId(string elementId)
        {
            await JSRuntime.InvokeAsync<bool>("scrollToElementId", elementId);
        }
        private async Task SearchNow()
        {
            Searching = true;
            CurrentSearchResults = await ProductsClient.GetSearchForProducts(ProductSearchText, SelectedCategory == null ? Guid.Empty : SelectedCategory.ID, VeganOrVega, "default", CurrentPage, 10, LimitResultOptions);
            Searching = false;
            StateHasChanged();
        }

        public Action UpdateUrlAction
        {
            get
            {
                return new Action(() => NavigateToUpdatedLink());
            }
        }

        public Func<Task> SearchAgainLink
        {
            get
            {
                return new Func<Task>(() => SearchNow());
            }
        }
        protected string GetNavigationLink(ProductCategory selectedCategory, bool onlyVegan, string productSearchText = null, int currentPage = 0)
        {
            var categoryIdAsString = selectedCategory != null ? selectedCategory.ID.ToString("D") : Guid.Empty.ToString("D");
            string veganOrVega = onlyVegan ? true.ToString(CultureInfo.InvariantCulture) : false.ToString(CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(productSearchText))
            {
                return $"Products/{categoryIdAsString}/{veganOrVega}/{currentPage}/{HttpUtility.UrlEncode(JsonConvert.SerializeObject(LimitResultOptions))}";
            }
            else
            {
                return $"Products/{productSearchText}/{categoryIdAsString}/{veganOrVega}/{currentPage}/{HttpUtility.UrlEncode(JsonConvert.SerializeObject(LimitResultOptions))}";
            }
        }
        protected void NavigateToUpdatedLink()
        {
            NavigationManager.NavigateTo(GetNavigationLink(SelectedCategory, VeganOrVegaSelected == "Alleen vegan", ProductSearchText, CurrentPage));
        }

        public string GetUrlWithPage(int pageCount)
        {
            var categoryIdAsString = SelectedCategory != null ? SelectedCategory.ID.ToString("D") : Guid.Empty.ToString("D");
            string veganOrVega = VeganOrVegaSelected == "Alleen vegan" ? true.ToString(CultureInfo.InvariantCulture) : false.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(ProductSearchText))
            {
                return $"Products/{ProductSearchText}/{categoryIdAsString}/{veganOrVega}/{pageCount}/{HttpUtility.UrlEncode(JsonConvert.SerializeObject(LimitResultOptions))}";
            }
            else
            {
                return $"Products/{categoryIdAsString}/{veganOrVega}/{pageCount}/{HttpUtility.UrlEncode(JsonConvert.SerializeObject(LimitResultOptions))}";
            }
        }

        public void VeganOrVegaSelectedChanged(object newValue)
        {
            VeganOrVegaSelected = newValue as string;
            VeganOrVega = VeganOrVegaSelected == "Alleen vegan";
        }


        public void GotoSearchWithBarcode()
        {
            NavigationManager.NavigateTo("scanbarcode/" + HttpUtility.UrlEncode("Product/frombarcode"));
        }

        protected override string GetTitleOfPage()
        {
            return "Producten zoeken";
        }
    }
}
