//using Veg.API.Client;
//using Veg.Data;
//using Veg.SSO.Controllers;
using Veg.API.Client;
using Veg.App.Pages;
using System.Threading.Tasks;

namespace Veg.App.Pages
{
    public interface ILoginRepository
    {
        Task<bool> CheckIfMeExists(User user);
        Task<CheckLicenseResult> CheckLicense(string licenseKey);
        Task<ConfirmEmailResult> ConfirmEmailAdress(string emailAdress, string code);
        Task<User> GetCurrentUser();
        Task<bool> GetHasTokenAsync();
        Task<string> GetTokenAsync();
        Task<LoginResult> Login(string emailAdress, string password, MembersClient membersClient);
        Task<ConfirmEmailResult> SendPasswordResetCode(string emailAdress, bool changePassword);
        Task<bool> Logout();
        Task<RegisterResult> Register(string emailAdress, string password);
        Task<ConfirmEmailResult> ResetPasswordAsync(string emailAdress, string resetCode, string password);
        Task UsernameChanged(string userName);
    }
}