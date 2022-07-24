using Microsoft.AspNetCore.Components;
using Veg.App.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Pages.LoginArea
{
    public class RegisterBase : BasePage
    {
        public bool IsRegistering { get; set; }
        public RegisterResult RegisterResult { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Password2 { get; set; }
        public string ErrorMessage { get; private set; }

        public async Task RegisterExecuteAsync()
        {
            if (!IsRegistering && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(Password2))
            {
                if(Password!= Password2)
                {
                    ErrorMessage = "Wachtwoorden komen niet overeen";
                }
                else
                {
                    IsRegistering = true;
                    RegisterResult = await LoginRepository.Register(Email, Password);
                    if (!RegisterResult.IsError)
                    {
                        UriHelper.NavigateTo("confirm/"+Email);
                    }
                    IsRegistering = false;
                }

            }
        }

        protected override string GetTitleOfPage()
        {
            return "Registeren";
        }
    }
}
