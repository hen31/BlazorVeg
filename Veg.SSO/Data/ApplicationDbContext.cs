using System;
using System.Collections.Generic;
using System.Text;
using Veg.SSO.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Veg.SSO.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<License> Licenses { get; set; }

        public DbSet<Veg.SSO.Models.LicenseKey> LicenseKey { get; set; }
    }
}
