using Microsoft.AspNetCore.Components;
using Veg.API.Client;
using Veg.App.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Pages
{
    public class ResetPasswordWithCodeBase : BasePage
    {
        [Inject]
        protected MembersClient MembersClient { get; set; }

        [Parameter]
        public string Email { get; set; }

        [Parameter]
        public bool FromMember { get; set; }

        [Parameter]
        public string Code { get; set; }
        public string Password { get; set; }
        public string PasswordComfirm { get; set; }

        public bool IsReseting { get; set; }

        public ConfirmEmailResult ResetResult { get; set; }

        public async Task ResetExecuteAsync()
        {
            if (!IsReseting && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
            {
                if (Password != PasswordComfirm)
                {
                    ResetResult = new ConfirmEmailResult() { IsError = true, ErrorType = ConfirmEmailErrorType.ModelError, Errors = new List<IdentityError>() { new IdentityError() {Code="C1", Description = "Wachtwoorden komen niet overeen" } } };
                }
                else
                {
                    IsReseting = true;
                    ResetResult = await LoginRepository.ResetPasswordAsync(Email, Code, Password);
                    if (!ResetResult.IsError)
                    {
                        UriHelper.NavigateTo("Login");
                    }
                    IsReseting = false;
                }
            }
        }

        protected override string GetTitleOfPage()
        {
            return "Wachtwoord resetten";
        }
    }
}
