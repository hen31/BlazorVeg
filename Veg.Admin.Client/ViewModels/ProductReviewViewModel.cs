using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.Entities;

namespace Veg.Admin.Client.ViewModels
{
    public class ProductReviewViewModel : ViewModel
    {
        private ProductReviewWindow _productReviewView;
        private ProductsClient _productClient;

        public AsyncCommand SaveCommand { get; }
        public ObservableCollection<ReviewImage> ReviewImages { get; }
        public RelayCommand DeleteImageCommand { get; }
        public RelayCommand GoToUserCommand { get; }

        private ImagesClient _imageClient;

        public ProductReviewViewModel(ProductReview review, ProductReviewWindow productReviewView)
        {
            _productReviewView = productReviewView;
            ProductReview = review;
            _productClient = App.Container.GetInstance<ProductsClient>();
            SaveCommand = new AsyncCommand(Save);
            ReviewImages = new ObservableCollection<ReviewImage>(ProductReview.ReviewImages);
            DeleteImageCommand = new RelayCommand(() =>
            {
                if (SelectedImage != null)
                {
                    ProductReview.ReviewImages.Remove(ProductReview.ReviewImages.Where(b=> b.ID == SelectedImage.ID).FirstOrDefault());
                    ReviewImages.Remove(ReviewImages.Where(b => b.ID == SelectedImage.ID).FirstOrDefault());
                }
            });

            GoToUserCommand = new RelayCommand(() =>
            {
                MemberWindow memberWindow = new MemberWindow();
                memberWindow.Owner = _productReviewView;
                UserViewModel userViewModel = new UserViewModel(ProductReview.Member, memberWindow);
                memberWindow.DataContext = userViewModel;
                memberWindow.Show();
            });
        }
      
        public string Title
        {
            get
            {
                return ProductReview == null ? "Review wijzigen" : ProductReview.Member.UserName + " review - wijzigen";
            }
        }

        ReviewImage _selectedImage;
        public ReviewImage SelectedImage
        {
            get
            {
                return _selectedImage;
            }
            set
            {
                _selectedImage = value;
                OnPropertyChanged();
            }
        }


        private async Task Save(object arg)
        {
            ProductReview = await _productClient.UpdateReviewAsync(ProductReview);
            _productReviewView.Close();
        }

        ProductReview _productReview;
        public ProductReview ProductReview
        {
            get
            {
                return _productReview;
            }
            set
            {
                _productReview = value;
                OnPropertyChanged();
            }
        }
    }
}
