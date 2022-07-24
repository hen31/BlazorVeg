using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Veg.API.Client;
using Veg.Entities;

namespace Veg.Admin.Client.ViewModels
{
    public class MainViewModel : ViewModel
    {
        public ProductsClient ProductsClient { get; set; }
        public MembersClient MembersClient { get; }
        public ImagesClient ImageClient { get; }
        public ReportsClient ReportsClient { get; }

        private MetroWindow _window;
        public MainViewModel(MetroWindow window)
        {
            ProductsClient = App.Container.GetInstance<ProductsClient>();
            MembersClient = App.Container.GetInstance<MembersClient>();
            ImageClient = App.Container.GetInstance<ImagesClient>();
            ReportsClient = App.Container.GetInstance<ReportsClient>();
            _window = window;
            SearchCommand = new AsyncCommand(ExecuteSearch);
            SearchUsersCommand = new AsyncCommand(ExecuteSearchUsers);
            SearchReportsCommand = new AsyncCommand(ExecuteReports);

            OnlyNotHandled = true;

            PreviousCommand = new AsyncCommand(async (_) => { if (CurrentPage != 0) { CurrentPage--; await ExecuteSearch(null); } });
            NextCommand = new AsyncCommand(async (_) => { if (CurrentSearchResults != null && CurrentPage != CurrentSearchResults.TotalItems / 30) { CurrentPage++; await ExecuteSearch(null); } });

            PreviousUsersCommand = new AsyncCommand(async (_) => { if (CurrentUserPage != 0) { CurrentUserPage--; await ExecuteSearchUsers(null); } });
            NextUsersCommand = new AsyncCommand(async (_) => { if (CurrentSearchUserResults != null && CurrentUserPage != TotalUserCount / 30) { CurrentUserPage++; await ExecuteSearchUsers(null); } });

            PreviousReportsCommand = new AsyncCommand(async (_) => { if (CurrentReportsPage != 0) { CurrentReportsPage--; await ExecuteReports(null); } });
            NextReportsCommand = new AsyncCommand(async (_) => { if (Reports != null && CurrentReportsPage != TotalReportsCount / 30) { CurrentReportsPage++; await ExecuteReports(null); } });

            EditImageCommand = new AsyncCommand(EditImageCommandExecute);

            DeleteCommand = new AsyncCommand(DeleteItemExecute);
            EditCommand = new AsyncCommand(EditCommandExecute);
            InitializeViewModel().FireAndForgetSafeAsync();

            VerifyCommand = new AsyncCommand(Verify);
            ChangeImageCommand = new AsyncCommand(ChangeImage);
            EmptyImageCommand = new AsyncCommand(EmptyImage);
        }



        public bool OnlyWithoutImage { get; set; }
        private async Task EditCommandExecute(object arg)
        {
            if (arg is Product product)
            {
                ProductWindow productView = new ProductWindow();
                productView.Owner = _window;
                productView.DataContext = new ProductViewModel(product, productView);
                productView.Show();
            }
            else if (arg is Member member)
            {
                MemberWindow memberWindow = new MemberWindow();
                memberWindow.Owner = _window;
                UserViewModel userViewModel = new UserViewModel(member, memberWindow);
                memberWindow.DataContext = userViewModel;
                memberWindow.Show();
            }
            else if (arg is Report report)
            {
                ReportWindow reportWindow = new ReportWindow();
                reportWindow.Owner = _window;
                ReportViewModel reportViewModel = new ReportViewModel(report, reportWindow);
                reportWindow.DataContext = reportViewModel;
                reportWindow.Show();
            }
        }


        private async Task EditImageCommandExecute(object arg)
        {
            if (arg is Product product)
            {
                var changedProduct = await UploadNewImage(product);
            }
        }



        private async Task DeleteItemExecute(object parameter)
        {
            if (parameter is Product productRemoving && (await MahApps.Metro.Controls.Dialogs.DialogManager.ShowMessageAsync(_window, "Verwijderen", $"Weet u zeker dat u {productRemoving.Name} wilt verwijderen?"
                , MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Ja", NegativeButtonText = "Nee" })) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                await ProductsClient.DeleteAsync(productRemoving.ID);
                CurrentSearchResults.Products.Remove(productRemoving);
            }
            else if (parameter is Report reportRemoving && (await MahApps.Metro.Controls.Dialogs.DialogManager.ShowMessageAsync(_window, "Verwijderen", $"Weet u zeker dat u report wilt verwijderen?"
                , MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Ja", NegativeButtonText = "Nee" })) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                await App.Container.GetInstance<ReportsClient>().DeleteAsync(reportRemoving.ID);
                Reports.Remove(reportRemoving);
            }
        }

        public int CurrentPage { get; set; }

        private async Task ExecuteSearch(object parameter)
        {
            if (parameter != null)
            {
                CurrentPage = 0;
            }
            CurrentSearchResults = await ProductsClient.GetSearchForProducts(SearchTerm, ProductCategory == null ? Guid.Empty : ProductCategory.ID, false, "default", CurrentPage, 30, new LimitResultOptions() { OnlyWithoutImage = OnlyWithoutImage });
        }

        private async Task ExecuteSearchUsers(object arg)
        {
            if (arg != null)
            {
                CurrentUserPage = 0;
            }
            var filterOptions = new FilterPagingOptions()
            {
                Page = CurrentUserPage + 1,
                ItemsPerPage = 30,
                FilterParameters = new Dictionary<string, string>() { { "SingleSearch", SearchTermUser } }
            };
            CurrentSearchUserResults = new ObservableCollection<Member>(await MembersClient.GetCollectionAsync(filterOptions));
            TotalUserCount = await MembersClient.GetCountAsync(filterOptions);
        }

        public int CurrentUserPage { get; set; } = 0;
        SearchResultsViewmodel _currentSearchResults;
        public SearchResultsViewmodel CurrentSearchResults
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


        public ProductCategory ProductCategory { get; set; }

        public async Task InitializeViewModel()
        {
            var categories = new List<ProductCategory>(await ProductsClient.GetCategoriesForAutoSelect());
            categories.Insert(0, new ProductCategory() { ID = Guid.Empty, Name = "Alle categorieën" });
            ProductCategories = categories;
            await ExecuteSearch(null);
            await ExecuteSearchUsers(null);
            await ExecuteReports(null);
            await GetProductForImage();
        }



        public bool OnlyNotHandled { get; set; }
        private async Task ExecuteReports(object p)
        {
            if (p != null)
            {
                CurrentReportsPage = 0;
            }
            var filterOptions = new FilterPagingOptions()
            {
                Page = CurrentReportsPage + 1,
                ItemsPerPage = 30,
                FilterParameters = new Dictionary<string, string>()
                { { "Handled", OnlyNotHandled.ToString()},
                },
                Includes = new string[] { "AddedByMember" },
                SortProperty = "AddedAt;Down"

            };
            Reports = new ObservableCollection<Report>(await ReportsClient.GetCollectionAsync(filterOptions));
            TotalReportsCount = await ReportsClient.GetCountAsync(filterOptions);
        }

        string _searchTerm;
        public string SearchTerm
        {
            get
            {
                return _searchTerm;
            }
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        public AsyncCommand SearchUsersCommand { get; set; }
        public AsyncCommand SearchReportsCommand { get; }
        public AsyncCommand PreviousUsersCommand { get; set; }
        public AsyncCommand NextUsersCommand { get; set; }
        public AsyncCommand PreviousReportsCommand { get; }
        public AsyncCommand NextReportsCommand { get; }
        public AsyncCommand EditImageCommand { get; }
        public string SearchTermUser { get; set; }

        public AsyncCommand SearchCommand { get; set; }
        public AsyncCommand PreviousCommand { get; set; }
        public AsyncCommand NextCommand { get; set; }
        public AsyncCommand DeleteCommand { get; }
        public AsyncCommand EditCommand { get; }

        ICollection<ProductCategory> _productCategories;
        public ICollection<ProductCategory> ProductCategories
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

        ObservableCollection<Member> _currentSearchUserResults;
        public ObservableCollection<Member> CurrentSearchUserResults
        {
            get
            {
                return _currentSearchUserResults;
            }
            set
            {
                _currentSearchUserResults = value;
                OnPropertyChanged();
            }
        }
        public int TotalUserCount { get; private set; }
        ObservableCollection<Report> _reports;
        public ObservableCollection<Report> Reports
        {
            get
            {
                return _reports;
            }
            set
            {
                _reports = value;
                OnPropertyChanged();
            }
        }
        public int TotalReportsCount { get; private set; }
        public int CurrentReportsPage { get; private set; }


        Product _productOfImage;
        public Product ProductOfImage
        {
            get
            {
                return _productOfImage;
            }
            set
            {
                _productOfImage = value;
                OnPropertyChanged();
            }
        }

        public AsyncCommand VerifyCommand { get; }
        public AsyncCommand ChangeImageCommand { get; }
        public AsyncCommand EmptyImageCommand { get; }

        private async Task EmptyImage(object arg)
        {
            await ProductsClient.RemoveImageForProductAsync(ProductOfImage);
            await GetProductForImage();
        }

        private async Task ChangeImage(object arg)
        {
            await UploadNewImage(ProductOfImage);
            await GetProductForImage();
        }

        private async Task<Product> UploadNewImage(Product forProduct)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Afbeeldingen|*.jpg;*.jpeg;*.png;";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Title = "Afbeelding voor product selecteren";
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                string uploadedFilename = await ImageClient.UploadImage(File.ReadAllBytes(fileName));
                if (!string.IsNullOrWhiteSpace(uploadedFilename))
                {
                    var changedImage = await ProductsClient.AddImageForProductAsync(forProduct, uploadedFilename);
                    changedImage = await ProductsClient.SetImageVerifiedForProductAsync(changedImage);
                    return changedImage;
                }
            }
            return null;
        }

        private async Task Verify(object arg)
        {
            await ProductsClient.SetImageVerifiedForProductAsync(ProductOfImage);
            await GetProductForImage();
        }

        private async Task GetProductForImage()
        {
            ProductOfImage = (await ProductsClient.GetSearchForProducts(string.Empty, Guid.Empty, false, "default", 0, 1, new LimitResultOptions() { OnlyNotVerified = true })).Products.FirstOrDefault();
        }
    }
}
