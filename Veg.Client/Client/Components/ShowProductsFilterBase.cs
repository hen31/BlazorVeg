using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veg.Client.Pages;
using Veg.Entities;

namespace Veg.Client.Components
{
    public class ShowProductsFilterBase : ComponentBase
    {
        int _currentPage;
           [Parameter]
        public int CurrentPage { get
            {
                return _currentPage;
            }
            set
            {
                if (_currentPage == value) return;
                _currentPage = value;
                CurrentPageChanged.InvokeAsync(value);
            }
        }

        [Parameter]
        public EventCallback<int> CurrentPageChanged { get; set; }
        [Parameter]
        public SearchResultsViewmodel CurrentSearchResults { get; set; }

        [Parameter]
        public LimitResultOptions LimitResultOptions { get; set; }

        public bool FilterVisible { get; set; }
        public bool MoreCategoriesVisible { get; set; }
        public bool MoreBrandsVisible { get; set; }
        public bool MoreTagsVisible { get; set; }
        [Parameter]
        public Action NavigateToUpdatedLink { get; set; }
        [Parameter]
        public Func<Task> SearchAgainLink { get; set; }
        
        public async Task TagChangedAsync(Tag tag, bool newValue)
        {
            CurrentPage = 0;

            if (newValue)
            {
                LimitResultOptions.Tags.Add(tag.ID);
            }
            else
            {
                LimitResultOptions.Tags.Remove(tag.ID);
            }
            NavigateToUpdatedLink();
            await SearchAgainLink.Invoke();
        }

        public async Task BrandChangedAsync(Brand brand, bool newValue)
        {
            CurrentPage = 0;

            if (newValue)
            {
                LimitResultOptions.Brands.Add(brand.ID);
            }
            else
            {
                LimitResultOptions.Brands.Remove(brand.ID);
            }
            NavigateToUpdatedLink();
            await SearchAgainLink.Invoke();
        }
        public async Task ProductCategoryChangedAsync(ProductCategory category, bool newValue)
        {
            if (newValue)
            {
                LimitResultOptions.ProductCategories.Add(category.ID);
            }
            else
            {
                LimitResultOptions.ProductCategories.Remove(category.ID);
            }
            CurrentPage = 0;
            NavigateToUpdatedLink();
            await SearchAgainLink.Invoke(); ;
        }

    }
}
