using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.Entities;

namespace Veg.Client.Components
{
    public class ShowReviewsBase : ComponentBase
    {
        private Product lastProduct;
        protected override async Task OnInitializedAsync()
        {
            if (lastProduct != ReviewsFor)
            {
                lastProduct = ReviewsFor;
            }
            if (ReviewsFor != null)
            {
                await LoadNextReviews();
            }
            await base.OnInitializedAsync();
        }
        public int TotalCount { get; set; }
        public async Task LoadNextReviews(bool clear =false)
        {
            LoadingReviews = true;
            if(clear)
            {
                Reviews.Clear();
                PageCount = 0;
            }
            ReviewsSearchResults = await ProductsClient.GetReviewsForProductAsync(ReviewsFor, PageCount, SelectedSorting);
            Reviews.AddRange(ReviewsSearchResults.Reviews);
            PageCount++;
            LoadingReviews = false;
            StateHasChanged();
        }

        [Inject]
        public ProductsClient ProductsClient { get; set; }

        public int PageCount { get; set; } = 0;

        [Parameter]
        public Product ReviewsFor { get; set; }

        public bool LoadingReviews { get; set; } = true;

        public ReviewSearchResultViewModel ReviewsSearchResults { get; set; }

        public int SelectedSorting { get; set; }

        public List<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        public async Task SelectedValueSortingChanged(int newSelection)
        {
            SelectedSorting = newSelection;
            Reviews = new List<ProductReview>();
            PageCount = 0;
            await LoadNextReviews(true);
        }
    }
}
