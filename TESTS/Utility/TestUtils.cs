using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace TESTS.Utility
{
    public class TestUtils<TContext> where TContext : DbContext
    {
        public TContext DbContextInstance { get; private set; }
        public IServiceProvider ServiceProviderInstance { get; }

        private readonly MsSqlContainer _mssql;

        public TestUtils(
            string mssqlImage = "mcr.microsoft.com/mssql/server:2019-latest",
            Action<DbContextOptionsBuilder> configureDbContext = null,
            Action<IServiceCollection> configureServices = null)
        {
            _mssql = new MsSqlBuilder()
                .WithImage(mssqlImage)
                .Build();

            var configureDbContext1 = configureDbContext;
            configureDbContext1 ??= optionsBuilder =>
            {
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                optionsBuilder.UseSqlServer(_mssql.GetConnectionString());
            };

            AsyncHelper.RunSync(() => _mssql.StartAsync());

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            configureDbContext1.Invoke(optionsBuilder);

            DbContextInstance = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options);

            AsyncHelper.RunSync(() => DbContextInstance.Database.EnsureCreatedAsync());

            var services = new ServiceCollection();
            services.AddSingleton(DbContextInstance ?? throw new InvalidOperationException("DbContextInstance is null"));

            configureServices?.Invoke(services);

            ServiceProviderInstance = services.BuildServiceProvider();
        }

        public async Task TearDown()
        {
            await DbContextInstance.Database.EnsureDeletedAsync();
            DbContextInstance.Dispose();
            DbContextInstance = null;
            await _mssql.StopAsync();
        }
    }

    public static class AsyncHelper
    {
        private static readonly TaskFactory MyTaskFactory = new
            TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return MyTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            MyTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}
