using AutoMapper;
using Veg.SSO.Data;
using Veg.SSO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Veg.SSO.Controllers
{
    [ApiController]
    [Route("api/Accounts")]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountsController(UserManager<ApplicationUser> userManager, ApplicationDbContext appDbContext, IEmailSender emailSender)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _emailSender = emailSender;
        }

        // POST api/accounts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdentity = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(userIdentity, model.Password);




            if (!result.Succeeded) return new BadRequestObjectResult(result.Errors);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(userIdentity);
            string confirmUrl = $"https://todo.com/confirm/{HttpUtility.UrlEncode(model.Email)}/{HttpUtility.UrlEncode(code)}/";

            await _appDbContext.SaveChangesAsync();
            await _emailSender.SendEmailAsync(model.Email, "ToDo - Emailadres bevestigen",
        $"Geachte meneer/mevrouw," +
        Environment.NewLine +
         Environment.NewLine +
        $"U kunt uw emailadres bevestigen met de volgende code: {code}" +
               Environment.NewLine +
            $"U kunt de code hier invullen:<a href=\"{confirmUrl}\">{confirmUrl}</a>" +
        Environment.NewLine +
            Environment.NewLine +
         $"Met vriendelijke groet," +
         Environment.NewLine +
         "ToDo");
            return new OkObjectResult("Account created");
        }

        [HttpPost]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> Post([FromBody] ConfirmEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return BadRequest();
            }
            var result = await _userManager.ConfirmEmailAsync(user, model.Code);

            if (!result.Succeeded) return new BadRequestObjectResult(result.Errors);

            await _appDbContext.SaveChangesAsync();

            return new OkObjectResult("Email confirmed");
        }


        [HttpPost]
        [Route("SendResetCode")]
        public async Task<IActionResult> Post([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return new OkObjectResult("Passwordreset code send");
            }
            var result = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _appDbContext.SaveChangesAsync();
            if (!model.ChangePassword)
            {
                string confirmUrl = $"https://todo.com/ResetPasswordWithCode/{HttpUtility.UrlEncode(model.Email)}/{HttpUtility.UrlEncode(result)}/";

                await _emailSender.SendEmailAsync(model.Email, "ToDo - Wachtwoord resetten",
                                                             $"Geachte meneer/mevrouw," +
                                                             Environment.NewLine +
                                                             Environment.NewLine +
                                                             $"U kunt uw wijzigen resetten met de volgende code: {result}" +
                                                                    Environment.NewLine +
                                                                $"U kunt de code hier invullen:<a href=\"{confirmUrl}\">{confirmUrl}</a>" +
                                                             Environment.NewLine +
                                                             Environment.NewLine +
                                                             $"Met vriendelijke groet," +
                                                             Environment.NewLine +
                                                             "ToDo");
            }
            else
            {
                string confirmUrl = $"https://todo.com/ResetPasswordWithCode/{HttpUtility.UrlEncode(model.Email)}/{HttpUtility.UrlEncode(result)}/";
                await _emailSender.SendEmailAsync(model.Email, "ToDo - Wachtwoord wijzigen",
                                                                $"Geachte meneer/mevrouw," +
                                                                Environment.NewLine +
                                                                Environment.NewLine +
                                                                $"U kunt uw wachtwoord wijzigen met de volgende code: {result}" +
                                                                       Environment.NewLine +
                                                                $"U kunt de code hier invullen:<a href=\"{confirmUrl}\">{confirmUrl}</a>" +

                                                                Environment.NewLine +
                                                                Environment.NewLine +
                                                                $"Met vriendelijke groet," +
                                                                Environment.NewLine +
                                                                "ToDo");

            }
            return new OkObjectResult("Passwordreset code send");
        }


        [HttpPost]
        [Route("ResetPasswordWithCode")]
        public async Task<IActionResult> Post([FromBody] ResetPasswordWithCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return BadRequest();
            }
            var result = await _userManager.ResetPasswordAsync(user, model.ResetCode, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(result.Errors);

            await _appDbContext.SaveChangesAsync();

            return new OkObjectResult("Password reset");
        }
    }
}
