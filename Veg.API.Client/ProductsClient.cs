using Newtonsoft.Json;
using Veg.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Web;

namespace Veg.API.Client
{
    public class ProductsClient : BaseClient<Product>
    {
        public ProductsClient(HttpClient client, ITokenProvider tokenProvider, IAPIClientSettings settings) : base(client, tokenProvider, settings, "products")
        {
        }


        public async Task<SearchResultsViewmodel> GetSearchForProducts(string searchTerm, Guid selectedCategory, bool onlyVegan, string sortBy, int page, int pageSize, LimitResultOptions limitResultOptions)
        {
            string url = string.Concat(await _settings.GetAPIUrl(), _area, "/searchproducts/", selectedCategory.ToString("D"), "/", onlyVegan.ToString(CultureInfo.InvariantCulture), "/", sortBy, "/", page.ToString(CultureInfo.InvariantCulture), "/", pageSize.ToString(CultureInfo.InvariantCulture), "/", HttpUtility.UrlEncode(JsonConvert.SerializeObject(limitResultOptions)));
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                url = string.Concat(url, "/", searchTerm);
            }
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(url).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<SearchResultsViewmodel>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<ProductReview> UpdateReviewAsync(ProductReview productReview)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PutAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/editreview/", productReview.ID.ToString()), new StringContent(JsonConvert.SerializeObject(productReview), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ProductReview>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return default(ProductReview);
        }

        public async Task<Product> FindItemByBarcodeAsync(string barcodeForSearch)
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/productbybarcode/", barcodeForSearch)).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<int> GetReviewsForMemberCountAsync(Member member)
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/reviewsformembercount/", member.ID.ToString("D"))).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return 0;
        }

        public async Task<ICollection<ProductReview>> GetReviewsForMemberAsync(Member reviewsFor, int pageCount)
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/reviewsformember/", reviewsFor.ID.ToString("D"), "/", pageCount.ToString(CultureInfo.InvariantCulture))).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<ProductReview>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }
        public async Task<int> GetProductsForMemberCountAsync(Member member)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/productsformembercount/", member.ID.ToString("D"))).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return 0;
        }

        public async Task<ICollection<Product>> GetProductsForMemberAsync(Member productsId, int pageCount)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/productsformember/", productsId.ID.ToString("D"), "/", pageCount.ToString(CultureInfo.InvariantCulture))).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<Product>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<ReviewSearchResultViewModel> GetReviewsForProductAsync(Product reviewsFor, int pageCount, int selectedSorting)
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/reviewsforproduct/", reviewsFor.ID.ToString("D"), "/", pageCount.ToString(CultureInfo.InvariantCulture), "/", selectedSorting.ToString(CultureInfo.InvariantCulture))).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ReviewSearchResultViewModel>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }
       
        public async Task<ProductReview> GetMyReviewForProductAsync(Product reviewFor)
        {
            var response = await (await GetHttpClient(true).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/myreviewforproduct/", reviewFor.ID.ToString("D"))).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ProductReview>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<Product> AddBarcodeToProduct(Product product, string barcode)
        {
            //addbarcode/{productid}/{barcode}
            var response = await (await GetHttpClient(true).ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/addbarcode/", product.ID.ToString("D"), "/", barcode), null).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }


        public async Task<ICollection<Store>> GetStoresForAutoComplete()
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/storesforautocomplete")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<Store>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<ICollection<Product>> GetLatestAddedProducts(int amount)
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/latestaddedproducts/", amount)).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<Product>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }


        public async Task<ProductReview> AddReview(Guid productId, ProductReview productReview)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/addreview/", productId.ToString("D")), new StringContent(JsonConvert.SerializeObject(productReview), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ProductReview>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }
        public async Task<Product> RemoveImageForProductAsync(Product product)
        {
            var response = await (await GetHttpClient(true).ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/removeimage/", product.ID.ToString("D")), null).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<Product> SetImageVerifiedForProductAsync(Product product)
        {
           var response = await (await GetHttpClient(true).ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/verifyimage/", product.ID.ToString("D")), null).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }
        
        public async Task<Product> AddImageForProductAsync(Product product, string photoName)
        {
            var response = await (await GetHttpClient(true).ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/addimage/", product.ID.ToString("D"), "/", photoName), null).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<ICollection<ProductCategory>> GetCategoriesForAutoSelect()
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/categoriesforautoselect")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<ProductCategory>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<ICollection<Store>> GetStoresForSelection()
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/storesforselection")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<Store>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }
        public async Task<ICollection<Brand>> GetBrandsForSelection()
        {
            var response = await (await GetHttpClient(false).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/brandsforselection")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<Brand>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task DeleteReviewAsync(Guid id)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).DeleteAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/deletereview/", id.ToString())).ConfigureAwait(false);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ThrowResponseException(response);
            }
        }
    }
}
