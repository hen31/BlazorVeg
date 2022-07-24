using Microsoft.AspNetCore.Components;
using Veg.API.Client;
using Veg.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Shared
{


    public class MainLayoutBase : LayoutComponentBase
    {

        [Inject]
        public Veg.App.Pages.ILoginRepository LoginRepository { get; set; }

        [Inject]
        public MembersClient MembersClient { get; set; }

        [Inject]
        public VegConfiguration VegConfiguration { get; set; }

        public Veg.App.Pages.User CurrentUser { get; set; }

        public Blazorise.Modal SetUserNameModal { get; set; }

        public bool SetUserNameError { get; set; } = false;

        public string UserNameError { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
           
            if (!VegConfiguration.IsLoaded)
            {
                await VegConfiguration.LoadVegConfiguration();
            }
            CurrentUser = await LoginRepository.GetCurrentUser();
            if (CurrentUser != null && string.IsNullOrWhiteSpace(CurrentUser.UserName))
            {
                SetUserNameModal.Show();
            }
            StateHasChanged();
        }

        public void HideModal()
        {
            SetUserNameModal.Hide();
        }

        public string UserName { get; set; }
        public async Task TrySavingUserName()
        {
            try
            {
                if (!string.IsNullOrEmpty(UserName) && UserName.Length >= 5 && UserName.Length <= 25)
                {
                    var member = await MembersClient.ChangeUsernameAsync(UserName);
                    if (member == null)
                    {
                        UserNameError = "Kan geen verbinding maken met server";
                        SetUserNameError = true;
                    }
                    else
                    {
                        await LoginRepository.UsernameChanged(UserName);
                        HideModal();
                    }
                }
                else
                {
                    UserNameError = "Gebruikersnaam moet minstens 5 karakters lang zijn en minder 25";
                    SetUserNameError = true;
                }

            }
            catch (Exception e)
            {
                UserNameError = e.Message;
                SetUserNameError = true;
            }

        }
    }
}
