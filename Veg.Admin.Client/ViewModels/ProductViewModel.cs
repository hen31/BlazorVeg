using MahApps.Metro.Controls.Dialogs;
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
    public class ProductViewModel : ViewModel
    {
        private Product _product;
        private Product _initialProduct;
        private ProductWindow _productView;
        private ProductsClient _productClient;
        private ImagesClient _imageClient;

        public ProductViewModel(Product product, ProductWindow productView)
        {
            _initialProduct = product;
            _productView = productView;
            _productClient = App.Container.GetInstance<ProductsClient>();
            _imageClient = App.Container.GetInstance<ImagesClient>();
            InitializeViewModel().FireAndForgetSafeAsync();
            AddTagCommand = new AsyncCommand(AddTag);
            DeleteTagCommand = new RelayCommand(DeleteTag);
            AddStoreCommand = new RelayCommand(AddStore);
            AddStoreDialogCommand = new AsyncCommand(AddStoreDialog);
            DeleteStoreCommand = new RelayCommand(DeleteStore);
            SaveCommand = new AsyncCommand(Save);
            ChangeImageCommand = new AsyncCommand(ChangeImage);
            DeleteImageCommand = new AsyncCommand(DeleteImage);
            DeleteCommand = new AsyncCommand(DeleteItemExecute);
            EditCommand = new AsyncCommand(EditCommandExecute);
            GoToUserCommand = new RelayCommand(() =>
            {
                MemberWindow memberWindow = new MemberWindow();
                memberWindow.Owner = _productView;
                UserViewModel userViewModel = new UserViewModel(Product.AddedByMember, memberWindow);
                memberWindow.DataContext = userViewModel;
                memberWindow.Show();
            });
            PreviousCommand = new AsyncCommand(async (_) => { if (CurrentPage != 0) { CurrentPage--; await ExecuteSearch(null); } });
            NextCommand = new AsyncCommand(async (_) => { if (CurrentSearchResults != null && CurrentPage != CurrentSearchResults.TotalCount / 30) { CurrentPage++; await ExecuteSearch(null); } });
        }

        private async Task AddStoreDialog(object arg)
        {
            string storeName = await MahApps.Metro.Controls.Dialogs.DialogManager.ShowInputAsync(_productView, "Winkel toevoegen", "Winkelnaam opgeven");
            if (!string.IsNullOrWhiteSpace(storeName))
            {
                AvailableAt productAvailableAt = new AvailableAt();
                productAvailableAt.Store = new Store()
                {
                    Name = storeName
                };
                Product.StoresAvailable.Add(productAvailableAt);
                AvaibleAt.Add(productAvailableAt);
            }

        }

        private async Task ExecuteSearch(object p)
        {
            CurrentSearchResults = await _productClient.GetReviewsForProductAsync(Product, CurrentPage, 0);

        }

        private async Task ChangeImage(object arg)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Title = "Afbeelding voor product selecteren";
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                string uploadedFilename = await _imageClient.UploadImage(File.ReadAllBytes(fileName));
                if (!string.IsNullOrWhiteSpace(uploadedFilename))
                {
                    Product = await _productClient.AddImageForProductAsync(Product, uploadedFilename);
                }
            }
        }

        private async Task DeleteImage(object arg)
        {
            Product = await _productClient.RemoveImageForProductAsync(Product);
        }

        private async Task Save(object arg)
        {
            Product.IsVegetarian = !Product.IsVegan;
            Product = await _productClient.EditAsync(Product.ID, Product);
            _productView.Close();
        }

        private void DeleteStore()
        {
            if (SelectedStore != null)
            {
                Product.StoresAvailable.Remove(SelectedStore);
                AvaibleAt.Remove(SelectedStore);
            }
        }

        private void AddStore()
        {
            if (SelectedStoreToAdd != null)
            {
                AvailableAt productAvailableAt = new AvailableAt();
                productAvailableAt.StoreId = SelectedStoreToAdd.ID;
                productAvailableAt.Store = SelectedStoreToAdd;
                Product.StoresAvailable.Add(productAvailableAt);
                AvaibleAt.Add(productAvailableAt);
            }
        }

        private async Task AddTag(object arg)
        {
            string result = await MahApps.Metro.Controls.Dialogs.DialogManager.ShowInputAsync(_productView, "Tag toevoegen", "Naam van tag");
            if (result != null && !Product.Tags.Any(b => b.Tag.Name.Equals(result, StringComparison.OrdinalIgnoreCase)))
            {
                ProductTagLink productTagLink = new ProductTagLink();
                productTagLink.Tag = new Tag();
                productTagLink.Tag.Name = result;
                Product.Tags.Add(productTagLink);
                Tags.Add(productTagLink);
            }
        }

        public ProductTagLink SelectedTag { get; set; }

        private void DeleteTag()
        {
            Product.Tags.Remove(SelectedTag);
            Tags.Remove(SelectedTag);
        }

        ObservableCollection<ProductTagLink> _tags;
        public ObservableCollection<ProductTagLink> Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                _tags = value;
                OnPropertyChanged();
            }
        }

        ObservableCollection<AvailableAt> _avaibleAt;
        public ObservableCollection<AvailableAt> AvaibleAt
        {
            get
            {
                return _avaibleAt;
            }
            set
            {
                _avaibleAt = value;
                OnPropertyChanged();
            }
        }
        public async Task InitializeViewModel()
        {
            var categories = new List<ProductCategory>(await _productClient.GetCategoriesForAutoSelect());
            ProductCategories = categories;
            Brands = new List<Brand>(await _productClient.GetBrandsForSelection());
            Product = await _productClient.GetItemAsync(_initialProduct.ID);
            Stores = await _productClient.GetStoresForAutoComplete();
            foreach (var store in await _productClient.GetStoresForSelection())
            {
                Stores.Add(store);
            }
            SelectedBrandIndex = Brands.IndexOf(Brands.First(b => b.ID == Product.BrandId));
            SelectedCategoryIndex = ProductCategories.IndexOf(ProductCategories.First(b => b.ID == Product.CategoryId));
            Tags = new ObservableCollection<ProductTagLink>(Product.Tags);
            AvaibleAt = new ObservableCollection<AvailableAt>(Product.StoresAvailable);
            OnPropertyChanged(nameof(Title));

            await ExecuteSearch(null);
        }

        public int CurrentPage { get; set; }

        public Store SelectedStoreToAdd { get; set; }

        public AvailableAt SelectedStore { get; set; }

        int _selectedBrandIndex;
        public int SelectedBrandIndex
        {
            get
            {
                return _selectedBrandIndex;
            }
            set
            {
                _selectedBrandIndex = value;
                if (_selectedBrandIndex != -1)
                {
                    Product.Brand = Brands.ElementAt(_selectedBrandIndex);
                }
                OnPropertyChanged();
            }
        }

        int _selectedCategoryIndex;
        public int SelectedCategoryIndex
        {
            get
            {
                return _selectedCategoryIndex;
            }
            set
            {
                _selectedCategoryIndex = value;
                if (_selectedCategoryIndex != -1)
                {
                    Product.Category = ProductCategories.ElementAt(_selectedCategoryIndex);
                }
                OnPropertyChanged();
            }
        }


        public Product Product
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
                OnPropertyChanged();
            }
        }

        ICollection<Store> _stores;
        public ICollection<Store> Stores
        {
            get

            {
                return _stores;
            }
            set
            {
                _stores = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                return _product == null ? "Product wijzigen" : _product.Brand.Name + " " + _product.Name + "- wijzigen";
            }
        }

        List<ProductCategory> _productCategories;
        public List<ProductCategory> ProductCategories
        {
            get
            {
                return _productCategories;
            }
            set
            {
                _productCategories = value;
                OnPropertyChanged();
            }
        }

        List<Brand> _brands;
        public List<Brand> Brands
        {
            get
            {
                return _brands;
            }
            set
            {
                _brands = value;
                OnPropertyChanged();
            }
        }

        AsyncCommand _addTagCommand;
        public AsyncCommand AddTagCommand
        {
            get
            {
                return _addTagCommand;
            }
            set
            {
                _addTagCommand = value;
                OnPropertyChanged();
            }
        }
        AsyncCommand _saveCommand;
        public AsyncCommand SaveCommand
        {
            get
            {
                return _saveCommand;
            }
            set
            {
                _saveCommand = value;
                OnPropertyChanged();
            }
        }

        public AsyncCommand ChangeImageCommand { get; }

        RelayCommand _deleteTagCommand;
        public RelayCommand DeleteTagCommand
        {
            get
            {
                return _deleteTagCommand;
            }
            set
            {
                _deleteTagCommand = value;
                OnPropertyChanged();
            }
        }
        RelayCommand _addStoreCommand;

        public RelayCommand AddStoreCommand
        {
            get
            {
                return _addStoreCommand;
            }
            set
            {
                _addStoreCommand = value;
                OnPropertyChanged();
            }
        }

        public AsyncCommand AddStoreDialogCommand { get; }

        RelayCommand _deleteStoreCommand;
        public RelayCommand DeleteStoreCommand
        {
            get
            {
                return _deleteStoreCommand;
            }
            set
            {
                _deleteStoreCommand = value;
                OnPropertyChanged();
            }
        }

        public AsyncCommand DeleteImageCommand { get; }
        public AsyncCommand DeleteCommand { get; }

        ReviewSearchResultViewModel _currentSearchResults;
        public ReviewSearchResultViewModel CurrentSearchResults
        {
            get
            {
                return _currentSearchResults;
            }
            set
            {
                _currentSearchResults = value;
                OnPropertyChanged();
            }
        }
        public AsyncCommand EditCommand { get; }
        public RelayCommand GoToUserCommand { get; }
        public AsyncCommand PreviousCommand { get; }
        public AsyncCommand NextCommand { get; }

        private async Task EditCommandExecute(object arg)
        {
            ProductReviewWindow productReviewView = new ProductReviewWindow();
            productReviewView.Owner = _productView;
            productReviewView.DataContext = new ProductReviewViewModel(arg as ProductReview, productReviewView);
            productReviewView.Show();
        }

        private async Task DeleteItemExecute(object parameter)
        {
            if (parameter is ProductReview productReview && (await MahApps.Metro.Controls.Dialogs.DialogManager.ShowMessageAsync(_productView, "Verwijderen", $"Weet u zeker dat u review wilt verwijderen?"
                , MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Ja", NegativeButtonText = "Nee" })) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                await _productClient.DeleteReviewAsync(productReview.ID);
                await ExecuteSearch(null);
            }
        }
    }
}
