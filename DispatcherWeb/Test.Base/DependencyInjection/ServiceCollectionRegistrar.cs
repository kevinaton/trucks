﻿using Abp.Dependency;
using Castle.Windsor.MsDependencyInjection;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DispatcherWeb.Test.Base.DependencyInjection
{
    public static class ServiceCollectionRegistrar
    {
        public static void Register(IIocManager iocManager)
        {
            RegisterIdentity(iocManager);

            var builder = new DbContextOptionsBuilder<DispatcherWebDbContext>();

            // TODO: Added this on upgraqde to .NET 6. Need to make it work.
            //var inMemorySqlite = new SqliteConnection("Data Source=:memory:");
            //builder.UseSqlite(inMemorySqlite);

            //iocManager.IocContainer.Register(
            //    Component
            //        .For<DbContextOptions<DispatcherWebDbContext>>()
            //        .Instance(builder.Options)
            //        .LifestyleSingleton()
            //);

            //inMemorySqlite.Open();

            new DispatcherWebDbContext(builder.Options).Database.EnsureCreated();
        }

        private static void RegisterIdentity(IIocManager iocManager)
        {
            var services = new ServiceCollection();

            IdentityRegistrar.Register(services);

            WindsorRegistrationHelper.CreateServiceProvider(iocManager.IocContainer, services);
        }
    }
}
