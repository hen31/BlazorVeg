using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.App.Pages;
using Veg.Entities;

namespace Veg.Client.Components
{
    public class AddReviewBase : ComponentBase
    {
        const string EmptyStar = "star_outline";
        const string FilledStar = "star";


        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);
            if (ProductReviewing != null
                && CurrentUser != null)
            {
                var reviewOfUser = await ProductsClient.GetMyReviewForProductAsync(ProductReviewing);
                if (reviewOfUser != null)
                {
                    AlreadyHasReview = true;

                    ContentOfReview = reviewOfUser.Content;
                    switch (reviewOfUser.Rating)
                    {
                        case 2:
                            Star2Clicked();
                            break;
                        case 3:
                            Star3Clicked();
                            break;
                        case 4:
                            Star4Clicked();
                            break;
                        case 5:
                            Star5Clicked();
                            break;
                    }

                    foreach (var image in reviewOfUser.ReviewImages)
                    {
                        Images.Add(new Entities.Image() { Data = await ImagesClient.DownloadImage("Original", image.ImageName) });
                    }
                    StateHasChanged();
                }
                else
                {
                    AlreadyHasReview = false;
                }
            }
            else
            {
                AlreadyHasReview = false;
            }
        }


        public bool AlreadyHasReview { get; set; } = false;

        [Inject]
        public ILoginRepository LoginRepository { get; set; }
        [Inject]
        public ProductsClient ProductsClient { get; set; }

        [Inject]
        public ImagesClient ImagesClient { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Star1Clicked();
            CurrentUser = await LoginRepository.GetCurrentUser();
        }

        [Parameter]
        public Action ReloadRefresh { get; set; }

        public List<Veg.Entities.Image> Images { get; set; } = new List<Veg.Entities.Image>();

        public void Star1Clicked()
        {
            Star1 = FilledStar;
            Star2 = EmptyStar;
            Star3 = EmptyStar;
            Star4 = EmptyStar;
            Star5 = EmptyStar;
        }
        public void Star2Clicked()
        {
            Star1 = FilledStar;
            Star2 = FilledStar;
            Star3 = EmptyStar;
            Star4 = EmptyStar;
            Star5 = EmptyStar;
        }
        public void Star3Clicked()
        {
            Star1 = FilledStar;
            Star2 = FilledStar;
            Star3 = FilledStar;
            Star4 = EmptyStar;
            Star5 = EmptyStar;
        }
        public void Star4Clicked()
        {
            Star1 = FilledStar;
            Star2 = FilledStar;
            Star3 = FilledStar;
            Star4 = FilledStar;
            Star5 = EmptyStar;
        }
        public void Star5Clicked()
        {
            Star1 = FilledStar;
            Star2 = FilledStar;
            Star3 = FilledStar;
            Star4 = FilledStar;
            Star5 = FilledStar;
        }

        public string Star1 { get; set; }
        public string Star2 { get; set; }
        public string Star3 { get; set; }
        public string Star4 { get; set; }
        public string Star5 { get; set; }
        public string ContentOfReview { get; set; }

        [Parameter]
        public Product ProductReviewing { get; set; }

        [Parameter]
        public bool ProductIsSet { get; set; }
        public bool IsNewProduct { get; set; } = false;
        public string ProductName { get; set; }

        public User CurrentUser { get; set; }

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
                        Images.Add(image);
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

        public bool IsAddingReview { get; set; }

        public async Task AddReviewClicked()
        {
            if (Star1 == FilledStar)
            {
                HasError = false;
                Added = false;
                IsAddingReview = true;
                ProductReview productReview = new ProductReview();
                productReview.ProductId = ProductReviewing.ID;
                productReview.Content = ContentOfReview;
                SetRatingOfProductReview(productReview);
                productReview.ReviewImages = new List<ReviewImage>();
                foreach (var image in Images)
                {
                    string imagePath = await ImagesClient.UploadImage(Convert.FromBase64String(image.Data));
                    productReview.ReviewImages.Add(new ReviewImage() { ImageName = imagePath });
                }
                var addedReview = await ProductsClient.AddReview(ProductReviewing.ID, productReview);
                if (addedReview == null)
                {
                    HasError = true;
                }
                else
                {
                    Added = true;
                    IsAddingReview = false;
                    if (ReloadRefresh != null)
                    {
                        ReloadRefresh.Invoke();
                    }
                }
                StateHasChanged();
            }
        }
        public bool Added { get; set; }
        public bool HasError { get; set; }

        private void SetRatingOfProductReview(ProductReview productReview)
        {
            if (Star5 == FilledStar)
            {
                productReview.Rating = 5;
            }
            else if (Star4 == FilledStar)
            {
                productReview.Rating = 4;
            }
            else if (Star3 == FilledStar)
            {
                productReview.Rating = 3;
            }
            else if (Star2 == FilledStar)
            {
                productReview.Rating = 2;
            }
            else
            {
                productReview.Rating = 1;
            }
        }
    }
}
