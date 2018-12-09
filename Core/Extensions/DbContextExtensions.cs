using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GoldRush.Core.Models;
using GoldRush.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using StructureMap;

namespace GoldRush.Core.Extensions
{
    public static class DbContextExtensions
    {
        public static ModelBuilder ApplyDesignTimeConfigurations(this ModelBuilder builder, string dir, bool useRowVersion)
        {
            var di = new DirectoryInfo(dir);
            var files = di.GetFiles("*.dll");

            foreach (FileInfo fi in files)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(fi.FullName);
                    if (!(assembly.FullName == "GoldRush.Core" ||
                        assembly.FullName == "GoldRush.Data" ||
                        assembly.FullName == "GoldRush.Infrastructure"))
                    {
                        builder.ApplyConfigurations(useRowVersion, fi.FullName);
                    }
                }
                catch (Exception)
                {

                }
            }

            return builder;
        }
        public static ModelBuilder ApplyConfigurations(this ModelBuilder builder, bool useRowVersion, string path)
        {
            List<Type> configurations = GetEntityConfigurationsFromAssembly(path);

            foreach (Type m in configurations)
            {
                var c = Activator.CreateInstance(m, false);
                MethodInfo methodInfo = c.GetType().GetMethod("ApplyConfigurations");
                dynamic result = null;
                if (methodInfo != null)
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object[] parametersArray = new object[] { builder, useRowVersion };
                    object classInstance = Activator.CreateInstance(c.GetType(), null);
                    result = methodInfo.Invoke(classInstance, parameters.Length == 0 ? null : parametersArray);
                }
            }

            return builder;
        }

        public static List<Type> GetEntityConfigurationsFromAssembly(string assemblyFile)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var configurations = assembly.GetExportedTypes()
                .Where(e => e.GetInterfaces()
                    .Any(c => c.GetTypeInfo().IsAssignableFrom(typeof(IEntityTypeConfigurationModule))))
                    .ToList();
            return configurations;
        }

        public static List<Type> GetTypesFromAssembly(string assemblyFile, Type type)
        {
            var assembly = Assembly.LoadFrom(assemblyFile);
            var types = assembly.GetExportedTypes()
                .Where(e => e.GetInterfaces()
                    .Any(c => c.GetTypeInfo().IsAssignableFrom(type)))
                    .ToList();
            return types;
        }

        public static ModelBuilder ApplyGlobalFiltersFromAssembly(this ModelBuilder builder, DbContext context, Tenant tenant, string dir)
        {
            var di = new DirectoryInfo(dir);
            var files = di.GetFiles("*.dll");

            foreach (FileInfo fi in files)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(fi.FullName);
                    if (!(assembly.FullName == "GoldRush.Core" ||
                        assembly.FullName == "GoldRush.Data" ||
                        assembly.FullName == "GoldRush.Infrastructure"))
                    {
                        builder.ApplyGlobalFilter(context, tenant, fi.FullName);
                    }
                }
                catch (Exception)
                {

                }
            }

            return builder;
        }

        public static ModelBuilder ApplyGlobalFilter(this ModelBuilder builder, DbContext context, Tenant tenant, string path)
        {
            List<Type> entities = GetTypesFromAssembly(path, typeof(ITenantEntity<,>)).Where(e => !e.IsAbstract && e.IsClass).ToList();

            foreach (Type type in entities)
            {
                MethodInfo methodInfo = context.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQuery").MakeGenericMethod(type);
                methodInfo.Invoke(context, new object[] { builder, tenant });
            }

            return builder;
        }

        public static ModelBuilder ApplyConfigurationsFromContainer(this ModelBuilder builder, IContainer container, Tenant tenant)
        {
            if (container != null)
            {
                var entityTypeConfigurations = container.Model.AllInstances
                .Where(
                    i =>
                    i.PluginType.IsGenericType &&
                    i.PluginType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                )
                .Select(m => m.ReturnedType)
                .Where(e => !e.IsGenericTypeDefinition && !e.IsAbstract)
                .ToArray();

                var useRowVersion = true;
                if (tenant != null)
                {
                    useRowVersion = tenant.Engine == DatabaseEngine.SqlServer;
                }

                foreach (Type m in entityTypeConfigurations)
                {
                    var c = Activator.CreateInstance(m, false);
                    MethodInfo methodInfo = c.GetType().GetMethod("ApplyConfigurations");
                    dynamic result = null;
                    if (methodInfo != null)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        object[] parametersArray = new object[] { builder, useRowVersion };
                        object classInstance = Activator.CreateInstance(c.GetType(), null);
                        result = methodInfo.Invoke(classInstance, parameters.Length == 0 ? null : parametersArray);
                    }
                    // var c = Activator.CreateInstance(m, new object[] { useRowVersion });
                    // MethodInfo methodInfo = builder.GetType().GetMethod("ApplyConfiguration", new Type[] { typeof(IEntityTypeConfiguration<>) });
                    // dynamic result = null;
                    // if (methodInfo != null)
                    // {
                    //     ParameterInfo[] parameters = methodInfo.GetParameters();
                    //     object[] parametersArray = new object[] { c };
                    //     result = methodInfo.Invoke(builder, parametersArray);
                    // }
                }
            }

            return builder;
        }
    }
}