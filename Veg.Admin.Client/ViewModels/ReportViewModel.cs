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
    public class ReportViewModel : ViewModel
    {
        private Report _report;
        private Report _initialReport;
        private ReportWindow _reportView;
        private ReportsClient _reportClient;

        public ReportViewModel(Report report, ReportWindow reportView)
        {
            _initialReport = report;
            _reportView = reportView;
            _reportClient = App.Container.GetInstance<ReportsClient>();
            InitializeViewModel().FireAndForgetSafeAsync();
            SaveCommand = new AsyncCommand(Save);
            GoToUserCommand = new RelayCommand(() =>
            {
                MemberWindow memberWindow = new MemberWindow();
                memberWindow.Owner = _reportView;
                UserViewModel userViewModel = new UserViewModel(Report.AddedByMember, memberWindow);
                memberWindow.DataContext = userViewModel;
                memberWindow.Show();
            });
            OpenProductCommand = new RelayCommand(() =>
            {
                if (Report.Product != null)
                {
                    ProductWindow productView = new ProductWindow();
                    productView.Owner = _reportView;
                    productView.DataContext = new ProductViewModel(Report.Product, productView);
                    productView.Show();
                }
                else if (report.ProductReview != null)
                {
                    ProductReviewWindow productReviewView = new ProductReviewWindow();
                    productReviewView.Owner = _reportView;
                    productReviewView.DataContext = new ProductReviewViewModel(report.ProductReview, productReviewView);
                    productReviewView.Show();
                }

            });
        }


        private async Task Save(object arg)
        {
            Report = await _reportClient.EditAsync(Report.ID, Report);
            _reportView.Close();
        }

        public async Task InitializeViewModel()
        {
            Report = await _reportClient.GetItemAsync(_initialReport.ID);
            OnPropertyChanged(nameof(Title));
        }

        public Report Report
        {
            get
            {
                return _report;
            }
            set
            {
                _report = value;
                OnPropertyChanged();
            }
        }


        public string Title
        {
            get
            {
                return _report == null ? "Report wijzigen" : _report.Reason + "- wijzigen";
            }
        }


        public AsyncCommand SaveCommand { get; }
        public RelayCommand GoToUserCommand { get; }
        public RelayCommand OpenProductCommand { get; }
    }
}
