using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veg.SSO.Data;
using Veg.SSO.Models;
using Microsoft.AspNetCore.Authorization;
using Veg.Classes;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Veg.SSO.Controllers
{
    [Route("api/LicenseKeys")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "token")]
    public class LicenseKeysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public LicenseKeysController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;

            /*  if (_context.Licenses.Count() == 0)
              {
                  for (int i = 0; i < 5; i++)
                  {
                      _context.LicenseKey.Add(LicenseKey.GenerateKey("Veg", 180));
                  }
                  _context.SaveChanges();
              }*/
        }

        // POST: api/LicenseKeys
        [Route("Bought")]
        [HttpPost]
        public async Task<IActionResult> PostBoughtLicenseKey([FromBody] BoughtLicense licenseKey)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (licenseKey != null && !string.IsNullOrWhiteSpace(licenseKey.Application))
            {

                var newGeneratedLicense = LicenseKey.GenerateKey(licenseKey.Application, 365);
                _context.LicenseKey.Add(newGeneratedLicense);
                await _context.SaveChangesAsync();

                var userIdGuid = HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Subject).Value;
                var user = await _userManager.FindByIdAsync(userIdGuid.ToString());

                await _emailSender.SendEmailAsync(user.Email, "Licentie Veg bezorging",
$"Geachte meneer/mevrouw," +
Environment.NewLine +
Environment.NewLine +
$"U kunt uw licentie gebruiken met de volgende code: {newGeneratedLicense.Key.ToUpperInvariant()}" +
Environment.NewLine +
  Environment.NewLine +
$"Met vriendelijke groet," +
Environment.NewLine +
"ToDo");

            }
            else
            {
                return Unauthorized();
            }
            return Ok(true);
        }

        [HttpPost]
        public async Task<IActionResult> PostLicenseKey([FromBody] LicenseKey licenseKey)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var result = new CheckLicenseResult();
            if (licenseKey != null && !string.IsNullOrWhiteSpace(licenseKey.Application) && !string.IsNullOrWhiteSpace(licenseKey.Key))
            {

                string lowerCaseLicenseKey = licenseKey.Key.ToLowerInvariant();
                string lowerCaseApplication = licenseKey.Application.ToLowerInvariant();
                var matchingLicense = await _context.LicenseKey.Where(b => b.Key == lowerCaseLicenseKey && b.Application == lowerCaseApplication && b.Used == false).SingleOrDefaultAsync();
                if (matchingLicense == null)
                {
                    result.ErrorType = CheckLicenseErrorType.ModelError;
                    result.IsError = true;
                }
                else
                {
                    result.Succes = true;
                    var userIdGuid = HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Subject).Value;
                    var endDateOfCurrentLicense = await _context.Licenses.Where(b => b.User.Id == userIdGuid).Select(x => x.EndDate).DefaultIfEmpty(DateTime.UtcNow).MaxAsync();
                    License newLicense = new License();
                    newLicense.Application = lowerCaseApplication;
                    newLicense.Key = lowerCaseLicenseKey;
                    newLicense.StartDate = endDateOfCurrentLicense;
                    newLicense.EndDate = newLicense.StartDate.AddDays(matchingLicense.DurationInDays);
                    newLicense.User = await _userManager.FindByIdAsync(userIdGuid.ToString());
                    matchingLicense.Used = true;
                    _context.Licenses.Add(newLicense);
                    await _context.SaveChangesAsync();
                    result.LicenseNowValidTill = newLicense.EndDate;
                }
            }
            else
            {
                result.ErrorType = CheckLicenseErrorType.ModelError;
                result.IsError = true;
            }

            return Ok(result);
        }

        private bool LicenseKeyExists(long id)
        {
            return _context.LicenseKey.Any(e => e.ID == id);
        }
    }
}