using Microsoft.AspNetCore.Components;
using Veg.App.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Pages.LoginArea
{
    public class ConfirmCodeBase : BasePage
    {


        public bool IsConfirming { get; set; }
        public ConfirmEmailResult ConfirmResult { get; set; }

        [Parameter]
        public string Email { get; set; }
        [Parameter]
        public string ConfirmCode { get; set; }
        public string ErrorMessage { get; private set; }

        public async Task ConfirmExecuteAsync()
        {
            if (!IsConfirming && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(ConfirmCode) )
            {
                    IsConfirming = true;
                    ConfirmResult = await LoginRepository.ConfirmEmailAdress(Email, ConfirmCode);
                    if (!ConfirmResult.IsError)
                    {
                        UriHelper.NavigateTo("login");
                    }
                    IsConfirming = false;
            }
        }

        protected override string GetTitleOfPage()
        {
            return "Emailcode";
        }
    }
}
