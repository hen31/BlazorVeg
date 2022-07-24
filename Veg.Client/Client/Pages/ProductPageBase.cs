using Blazorise;
using Blazorise.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Infrastructure;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Veg.API.Client;
using Veg.App.Pages;
using Veg.Client.Components;
using Veg.Entities;

namespace Veg.Client.Pages
{
    public class ProductPageBase : BasePage
    {
        [Parameter]
        public string Barcode { get; set; }
        [Parameter]
        public string BarcodeForSearch { get; set; }
        [Inject]
        public ProductsClient ProductsClient { get; set; }

        public string[] Reasons { get; private set; }

        public void GoToLogin()
        {
            NavigationManager.NavigateTo($"Login/{System.Web.HttpUtility.UrlEncode(NavigationManager.Uri.ToString().Replace(NavigationManager.BaseUri.ToString(), ""))}");
        }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            ReloadReviews = new Action(async () => await ReviewsComponent.LoadNextReviews(true));
            AlreadyAddedReviewChanged = new Action<bool>((bool isReviewed) => AlreadyHasReviewed = isReviewed);
            CurrentUser = await LoginRepository.GetCurrentUser();
            if (!string.IsNullOrWhiteSpace(BarcodeForSearch))
            {
                var productFromBarcode = await ProductsClient.FindItemByBarcodeAsync(BarcodeForSearch);
                if (productFromBarcode == null)
                {
                    NoProductFound = true;
                }
                else
                {
                    CurrentProduct = productFromBarcode;
                }
            }
            else if (Guid.TryParse(ID, out Guid IdOfProduct))
            {
                var productFromService = await ProductsClient.GetItemAsync(IdOfProduct);
                if (productFromService != null)
                {
                    NoProductFound = false;
                    CurrentProduct = productFromService;

                }
                else
                {
                    NoProductFound = true;
                }
            }

            if (!NoProductFound)
            {
                var currentCategory = CurrentProduct.Category;
                Categories = new List<ProductCategory>();
                while (currentCategory != null)
                {
                    Categories.Insert(0, currentCategory);
                    currentCategory = currentCategory.ParentCategory;
                }
                await UpdateTitleAsync();


                Reasons = new string[] {
                            "Verkeerde naam",
                            "Verkeerde barcode",
                            CurrentProduct.IsVegan ? "Is niet vegan" : "Is wel vegan",
                            "Is niet vegetarisch",
                            "Niet verkrijgbaar in bepaalde winkel",
                            "Ook verkrijgbaar bij andere winkel",
                            "Verkeerde tag",
                            "Verkeerde categorie",
                            "Anders"
                            };

                if (CurrentUser != null)
                {
                    var reviewOfUser = await ProductsClient.GetMyReviewForProductAsync(CurrentProduct);
                    if (reviewOfUser != null)
                    {
                        AlreadyHasReviewed = true;
                    }
                    else
                    {
                        InitialAdd = true;
                    }
                    if (!string.IsNullOrWhiteSpace(Barcode) && string.IsNullOrWhiteSpace(CurrentProduct.Barcode))
                    {
                        var barcodeProduct = await ProductsClient.AddBarcodeToProduct(CurrentProduct, Barcode);
                        if (barcodeProduct == null)
                        {
                            BarcodeError = true;
                        }
                        else
                        {
                            CurrentProduct = barcodeProduct;
                            NavigationManager.NavigateTo($"Product/{CurrentProduct.ID.ToString("D")}");
                            AddedBarcode = true;
                        }
                    }
                }
                else
                {
                    AlreadyHasReviewed = true;
                }
            }
        }
        public void ReportItem()
        {
            if (CurrentUser != null)
            {
                ReportModal.ShowModal();
            }
            else
            {
                GoToLogin();
            }
        }
        public bool AddedBarcode { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public Action ReloadReviews { get; set; }
        public ShowReviews ReviewsComponent { get; set; }

        public Action<bool> AlreadyAddedReviewChanged { get; private set; }
        public ReportModal ReportModal { get; set; }
        public bool NoProductFound { get; set; }

        public Product CurrentProduct { get; set; }
        public User CurrentUser { get; set; }

        public FileEdit FileEditPhoto { get; set; }

        public async Task AddPhotoToProduct()
        {
            var lDotNetReference = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("GLOBAL.SetDotnetReference", lDotNetReference);
            await JSRuntime.InvokeAsync<object>("TriggerClickOfElement", FileEditPhoto.ElementId);
        }
        public bool InitialAdd { get; set; }
        public bool AlreadyHasReviewed { get; set; }
        [Inject]
        public ILoginRepository LoginRepository { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        public string GetSearchForBrand(Brand brand)
        {
            var categoryIdAsString = Guid.Empty.ToString("D");
            string veganOrVega = false.ToString(CultureInfo.InvariantCulture);
            var brands = new List<Guid>() { brand.ID };
            return $"Products/{categoryIdAsString}/{veganOrVega}/0/{HttpUtility.UrlEncode(JsonConvert.SerializeObject(new LimitResultOptions() { Brands = brands }))}";
        }

        public string GetSearchForCategory(ProductCategory category)
        {
            var categoryIdAsString = category.ID.ToString("D");
            string veganOrVega = false.ToString(CultureInfo.InvariantCulture);
            var categories = new List<Guid>() { };
            return $"Products/{categoryIdAsString}/{veganOrVega}/0/{HttpUtility.UrlEncode(JsonConvert.SerializeObject(new LimitResultOptions() { ProductCategories = categories }))}";
        }

        protected override string GetTitleOfPage()
        {
            if (CurrentProduct == null)
            {
                return "Product inzien";
            }
            return CurrentProduct.Brand.Name + " " + CurrentProduct.Name;
        }
        public Modal BarcodeModal { get; set; }
        public async Task ScanForBarcode()
        {
            NavigationManager.NavigateTo("scanbarcode/" + HttpUtility.UrlEncode("Product/" + CurrentProduct.ID.ToString("D") + "/addbarcode"));
        }
        public string CustomBarcode { get; set; }
        public async Task SaveBarcodeManual()
        {
            if (!string.IsNullOrWhiteSpace(CustomBarcode))
            {
                var barcodeProduct = await ProductsClient.AddBarcodeToProduct(CurrentProduct, CustomBarcode);
                if (barcodeProduct == null)
                {
                    BarcodeError = true;
                }
                else
                {
                    CurrentProduct = barcodeProduct;
                    NavigationManager.NavigateTo($"Product/{CurrentProduct.ID.ToString("D")}");
                    AddedBarcode = true;
                    HideModal();
                }
            }
        }

        public void HideModal()
        {
            BarcodeModal.Hide();
        }
        public void ShowDialogForBarcode()
        {
            BarcodeModal.Show();
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

                        var resultProduct = await ProductsClient.AddImageForProductAsync(CurrentProduct, await ImagesClient.UploadImage(stream.ToArray()));
                        CurrentProduct.ProductImage = resultProduct.ProductImage;
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

        public List<ProductCategory> Categories { get; set; }

        [Inject]
        public ImagesClient ImagesClient { get; set; }

        [Parameter]
        public string ID { get; set; }
        public bool BarcodeError { get; set; }


    }
}
