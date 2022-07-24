using Microsoft.EntityFrameworkCore;
using Veg.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Data.EntityFramework.SQL
{
    public class SQLContext : VegDatabaseContext
    {
     
        public SQLContext()
        {
        }
        string _dataSource;


        public SQLContext(string dataSource)
        {
            _dataSource = dataSource;
            InitializeDbContext();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_dataSource, providerOptions => providerOptions.CommandTimeout(60))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
