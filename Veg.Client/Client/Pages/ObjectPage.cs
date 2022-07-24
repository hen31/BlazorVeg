using Microsoft.AspNetCore.Components;
using Veg.API.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veg.Entities;

namespace Veg.Client.Pages
{
    public abstract class ObjectPage<T> : BasePage
        where T : BaseEntity, new()
    {

        public bool CanEditCurrentObject { get; set; }



        protected BaseClient<T> ApiClient { get; set; }

        public ObjectPage()
        {
            CurrentObject = new T();

        }

        private string lastInit = null;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            string currentRequest = UriHelper.ToBaseRelativePath(UriHelper.Uri);
            if (currentRequest != lastInit && CurrentObjectId != null && Guid.TryParse(CurrentObjectId, out Guid currentObjectIdAsGuid))
            {
                lastInit = currentRequest;
                CurrentObject = await ApiClient.GetItemAsync(currentObjectIdAsGuid);
                CanEditCurrentObject = CurrentUser.IsAdmin;
                StateHasChanged();
            }
        }


        [Parameter]
        public string CurrentObjectId { get; set; }

        public void GoBack()
        {
            /*UriHelper.NavigateTo(CollectionPage);*/
        }

        public T CurrentObject { get; set; }

        public bool adding = false;
        public async virtual Task AddCurrentObjectAsync()
        {
            if (!adding && CurrentObject.IsValidObject())
            {
                adding = true;
                try
                {
                    if (CurrentObjectId != null && Guid.TryParse(CurrentObjectId, out Guid currentObjectIdAsGuid))
                    {
                        T editedObject = await ApiClient.EditAsync(currentObjectIdAsGuid, CurrentObject);
                        if (editedObject != null)
                        {
                           // UriHelper.NavigateTo(CollectionPage);
                        }
                    }
                    else
                    {
                        T addedObject = await ApiClient.AddAsync(CurrentObject);
                        if (addedObject != null)
                        {
                            //UriHelper.NavigateTo(CollectionPage);
                        }
                    }
                }
                finally
                {
                    adding = false;
                }
            }
        }

    }
}