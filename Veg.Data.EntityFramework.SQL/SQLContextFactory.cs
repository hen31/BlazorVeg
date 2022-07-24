using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Data.EntityFramework.SQL
{
    internal class SQLContextFactory : IDesignTimeDbContextFactory<SQLContext>
    {
        public SQLContextFactory()
        {
            // A parameter-less constructor is required by the EF Core CLI tools.
        }

        public SQLContext CreateDbContext(string[] args)
        {
            return new SQLContext("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Veg-Dev;Integrated Security=True;");
        }
    }
}
