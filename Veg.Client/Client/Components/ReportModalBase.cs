using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.Entities;

namespace Veg.Client.Components
{
    public class ReportModalBase : BaseComponent
    {
        [Parameter]
        public Product Product { get; set; }
        [Parameter]
        public ProductReview ProductReview { get; set; }

        [Inject]
        public ReportsClient ReportsClient { get; set; }

        protected Modal ModalRef { get; set; }

        [Parameter]
        public string[] Reasons
        {
            get;set;
        }
        public string SelectedReason { get; set; }


        public string ExtraInformation { get; set; }

        protected void CloseModal()
        {
            ModalRef.Hide();
        }

        public void ShowModal()
        {
            ModalRef.Show();
        }
        protected async Task SaveReportItem()
        {
            if(!string.IsNullOrWhiteSpace(SelectedReason))
            {
                Report report = new Report();
                if(Product != null)
                {
                    report.ProductId = Product.ID;
                }
                else if(ProductReview !=null)
                {
                    report.ProductReviewId = ProductReview.ID;
                }
                report.Reason = SelectedReason;
                report.ExtraInformation = ExtraInformation;
                await ReportsClient.AddAsync(report);
                CloseModal();
            }
        }
    }
}
