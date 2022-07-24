using Microsoft.AspNetCore.Components;
using Veg.API.Client;
using Veg.App.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Pages
{
    public class LoginBasePage : BasePage
    {
        User _currentUser;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _currentUser = (await LoginRepository.GetCurrentUser());
                if (_currentUser != null)
                {
                    Email = _currentUser.EmailAdress;
                    this.StateHasChanged();
                }
                await base.OnAfterRenderAsync(firstRender);
            }
        }


        [Inject]
        protected MembersClient MembersClient { get; set; }


        [Parameter]
        public string ReturnUrl { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsLoggingIn { get; set; }

        public LoginResult LoginResult { get; set; }

        public async Task LoginExecuteAsync()
        {
            if (!IsLoggingIn && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
            {
                IsLoggingIn = true;
                LoginResult = await LoginRepository.Login(Email, Password, MembersClient);
                if (!LoginResult.IsError)
                {
                    if (string.IsNullOrWhiteSpace(ReturnUrl))
                    {
                        UriHelper.NavigateTo("/");
                    }
                    else
                    {
                        UriHelper.NavigateTo(ReturnUrl);

                    }
                }
                IsLoggingIn = false;
            }
        }

        protected override string GetTitleOfPage()
        {
            return "Inloggen";
        }
    }
}
