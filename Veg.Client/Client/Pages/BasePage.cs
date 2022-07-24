using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Veg.App.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Pages
{
    public abstract class BasePage : ComponentBase
    {

        protected override async Task OnInitializedAsync()
        {
            if(!VegConfiguration.IsLoaded)
            {
                await VegConfiguration.LoadVegConfiguration();
            }
         
        }

        protected override  async Task OnAfterRenderAsync(bool firstRender)
        {
            string currentRequest = UriHelper.ToBaseRelativePath(UriHelper.Uri);
            if (currentRequest != lastInit)
            {
                lastInit = currentRequest;
            }
            await UpdateTitleAsync();

        }

        private string lastInit = null;

        internal async Task UpdateTitleAsync()
        {
            string title = string.Empty;
            string pageTitle = GetTitleOfPage();

            if (!string.IsNullOrWhiteSpace(pageTitle))
            {
              
                    title = string.Format("{1} | {0}", VegConfiguration.SiteName, pageTitle);
            }
            await ChangeTitelOfPage(title);

        }
        [Inject]
        protected VegConfiguration VegConfiguration { get; set; }

        protected abstract string GetTitleOfPage();

        [Inject]
        protected ILoginRepository LoginRepository { get; set; }

        [Inject]
        protected NavigationManager UriHelper { get; set; }


        [Inject]
        protected IJSRuntime JSRuntime { get; set; }
        public User CurrentUser { get; private set; }

        public async Task ChangeTitelOfPage(string title)
        {
            await JSRuntime.InvokeAsync<object>("ChangeTitelOfPage", new object[] { title });
        }
    }
}
