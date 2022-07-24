using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veg.API.Client;
using Veg.App.Pages;
using Veg.Entities;

namespace Veg.Client.Pages
{
    public class ProfileBase : ComponentBase
    {
        public User CurrentUser { get; set; }

        public Member Member { get; set; }

        [Inject]
        public ILoginRepository LoginRepository { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public MembersClient MembersClient { get; set; }

        [Inject]
        public ProductsClient ProductsClient { get; set; }

        [Parameter]
        public Guid IdOfUser { get; set; }

        public bool NoUserFound { get; set; }

        public bool ProfileForCurrentUser { get; set; }
        public List<ProductReview> ReviewsForUser { get; private set; }
        public int TotalReviewsForUserCount { get; set; }
        public bool LoadingReviews { get; private set; }
        public int pageCount = 1;
        public async Task LoadNextReviews()
        {
            if (!LoadingReviews)
            {
                LoadingReviews = true;
                var nextReviews = await ProductsClient.GetReviewsForMemberAsync(Member, pageCount++);
                ReviewsForUser.AddRange(nextReviews);
                LoadingReviews = false;
                StateHasChanged();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            CurrentUser = await LoginRepository.GetCurrentUser();
            if (IdOfUser != null && IdOfUser != Guid.Empty)
            {
                Member = await MembersClient.GetItemAsync(IdOfUser);
                if (Member == null)
                {
                    NoUserFound = true;
                }
                else if (CurrentUser != null && CurrentUser.MemberId == Member.ID)
                {
                    ProfileForCurrentUser = true;
                }
                ReviewsForUser = new List<ProductReview>(await ProductsClient.GetReviewsForMemberAsync(Member, 0));
                TotalReviewsForUserCount = await ProductsClient.GetReviewsForMemberCountAsync(Member);
            }
        }

        public async Task ChangePassword()
        {
            await LoginRepository.SendPasswordResetCode(CurrentUser.EmailAdress, true);
            NavigationManager.NavigateTo("/ResetPasswordWithCode/" + CurrentUser.EmailAdress + "/" + true);
        }
    }
}
