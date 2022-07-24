using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.App.Pages;

namespace Veg.Client.Pages
{
    public class ScanBarcodeBase : ComponentBase
    {

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            var lDotNetReference = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("GLOBAL.SetDotnetReference", lDotNetReference);
            await JSRuntime.InvokeAsync<string>("GetBarCode");
        }

        [Inject]
        public ProductsClient ProductsClient { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public User CurrentUser { get; set; }

        [Inject]
        public ILoginRepository LoginRepository { get; set; }


    }
}
