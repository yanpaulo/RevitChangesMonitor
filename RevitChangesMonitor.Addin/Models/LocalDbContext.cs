namespace RevitChangesMonitor.Addin.Models
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    class SqlCeDbConfiguration : DbConfiguration
    {
        public SqlCeDbConfiguration()
        {
            SetDefaultConnectionFactory(new System.Data.Entity.Infrastructure.SqlCeConnectionFactory("System.Data.SqlServerCe.4.0"));
            SetProviderFactory("System.Data.SqlServerCe.4.0", System.Data.SqlServerCe.SqlCeProviderFactory.Instance);
            SetProviderServices("System.Data.SqlServerCe.4.0", System.Data.Entity.SqlServerCompact.SqlCeProviderServices.Instance);

            SetDatabaseInitializer(new MigrateDatabaseToLatestVersion<LocalDbContext, Migrations.Configuration>());
        }
    }

    [DbConfigurationType(typeof(SqlCeDbConfiguration))]
    public class LocalDbContext : DbContext
    {

        // Your context has been configured to use a 'LocalDbContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'RevitChangesMonitor.Addin.Models.LocalDbContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'LocalDbContext' 
        // connection string in the application configuration file.
        public LocalDbContext()
            : base($@"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\RCM\data.sdf;Persist Security Info=False;")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<AppConfiguration> AppConfiguration { get; set; }

        public virtual DbSet<LoginInformation> LoginInformation { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    
}