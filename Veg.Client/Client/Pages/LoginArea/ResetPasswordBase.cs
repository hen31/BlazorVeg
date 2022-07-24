using Microsoft.AspNetCore.Components;
using Veg.API.Client;
using Veg.App.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Pages
{
    public class ResetPasswordBase : BasePage
    {
        [Inject]
        protected MembersClient MembersClient { get; set; }

        [Parameter]
        public string Email { get; set; }

        public bool IsReseting { get; set; }

        public ConfirmEmailResult ResetResult { get; set; }

        public async Task ResetExecuteAsync()
        {
            if (!IsReseting && !string.IsNullOrWhiteSpace(Email))
            {
                IsReseting = true;
                ResetResult = await LoginRepository.SendPasswordResetCode(Email,false);
                if (!ResetResult.IsError)
                {
                    UriHelper.NavigateTo("ResetPasswordWithCode/" + Email );
                }
                IsReseting = false;
            }
        }

        protected override string GetTitleOfPage()
        {
            return "Wachtwoord resetten";
        }
    }
}
