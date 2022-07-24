using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.Entities;

namespace Veg.Admin.Client.ViewModels
{
    class UserViewModel : ViewModel
    {
        private Member _member;
        private Member _initialMember;
        private MemberWindow _memberView;
        private MembersClient _memberClient;
        private ProductsClient _productClient;

        public UserViewModel(Member member, MemberWindow memberView)
        {
            _initialMember = member;
            _memberView = memberView;
            _memberClient = App.Container.GetInstance<MembersClient>();
            _productClient = App.Container.GetInstance<ProductsClient>();
            InitializeViewModel().FireAndForgetSafeAsync();
            SaveCommand = new AsyncCommand(Save);
            PreviousReviewsCommand = new AsyncCommand(async (_) => { if (CurrentReviewPage != 0) { CurrentReviewPage--; await ExecuteGetReviews(); } });
            NextReviewsCommand = new AsyncCommand(async (_) => { if (CurrentReviewPage != TotalReviewsCount / 10) { CurrentReviewPage++; await ExecuteGetReviews(); } });

            PreviousProductsCommand = new AsyncCommand(async (_) => { if (CurrentProductPage != 0) { CurrentProductPage--; await ExecuteGetProducts(); } });
            NextProductsCommand = new AsyncCommand(async (_) => { if (CurrentProductPage != TotalProductsCount / 10) { CurrentProductPage++; await ExecuteGetProducts(); } });

            DeleteCommand = new AsyncCommand(DeleteItemExecute);
            EditCommand = new AsyncCommand(EditCommandExecute);
        }

        private async Task ExecuteGetProducts()
        {
            Products = new ObservableCollection<Product>(await _productClient.GetProductsForMemberAsync(Member, CurrentProductPage));
        }

        private async Task Save(object arg)
        {
            Member = await _memberClient.EditAsync(Member.ID, Member);
            _memberView.Close();
        }

        public async Task InitializeViewModel()
        {
            Member = await _memberClient.GetItemAsync(_initialMember.ID);
            await ExecuteGetReviews();
            await ExecuteGetProducts();

            TotalReviewsCount = await _productClient.GetReviewsForMemberCountAsync(Member);
            TotalProductsCount = await _productClient.GetProductsForMemberCountAsync(Member);
            OnPropertyChanged(nameof(Title));
        }

        private async Task EditCommandExecute(object parameter)
        {
            if (parameter is ProductReview productReview)
            {
                ProductReviewWindow productReviewView = new ProductReviewWindow();
                productReviewView.Owner = _memberView;
                productReviewView.DataContext = new ProductReviewViewModel(productReview, productReviewView);
                productReviewView.Show();
            }
            else if (parameter is Product product)
            {
                ProductWindow productView = new ProductWindow();
                productView.Owner = _memberView;
                productView.DataContext = new ProductViewModel(product, productView);
                productView.Show();
            }
        }

        private async Task DeleteItemExecute(object parameter)
        {
            if (parameter is ProductReview productReview && (await MahApps.Metro.Controls.Dialogs.DialogManager.ShowMessageAsync(_memberView, "Verwijderen", $"Weet u zeker dat u review wilt verwijderen?"
                , MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Ja", NegativeButtonText = "Nee" })) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                await _productClient.DeleteReviewAsync(productReview.ID);
                await ExecuteGetReviews();
            }
            else if (parameter is Product product && (await MahApps.Metro.Controls.Dialogs.DialogManager.ShowMessageAsync(_memberView, "Verwijderen", $"Weet u zeker dat u {product.Name} wilt verwijderen?"
                , MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Ja", NegativeButtonText = "Nee" })) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                await _productClient.DeleteAsync(product.ID);
                await ExecuteGetProducts();
            }
        }

        private async Task ExecuteGetReviews()
        {
            Reviews = new ObservableCollection<ProductReview>(await _productClient.GetReviewsForMemberAsync(Member, CurrentReviewPage));
        }

        public int CurrentReviewPage { get; set; }


        public Member Member
        {
            get
            {
                return _member;
            }
            set
            {
                _member = value;
                OnPropertyChanged();
            }
        }

        ObservableCollection<ProductReview> _reviews;
        public ObservableCollection<ProductReview> Reviews
        {
            get
            {
                return _reviews;
            }
            set
            {
                _reviews = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                return _member == null ? "Gebruiker wijzigen" : _member.UserName + "- wijzigen";
            }
        }


        public AsyncCommand SaveCommand { get; }
        public AsyncCommand PreviousReviewsCommand { get; }
        public int TotalReviewsCount { get; private set; }
        public AsyncCommand NextReviewsCommand { get; }
        public AsyncCommand PreviousProductsCommand { get; }
        public AsyncCommand NextProductsCommand { get; }
        public AsyncCommand DeleteCommand { get; }
        public AsyncCommand EditCommand { get; }
        public int CurrentProductPage { get; private set; }
        public int TotalProductsCount { get; private set; }
        ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
    {
            get
            {
                return _products;
            }
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }
    }
}
