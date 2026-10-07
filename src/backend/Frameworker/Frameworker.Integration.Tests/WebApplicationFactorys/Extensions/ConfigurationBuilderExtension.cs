using System.IO;
using Microsoft.Extensions.Configuration;

namespace Frameworker.Integration.Tests.WebApplicationFactorys.Extensions
{
    public static class ConfigurationBuilderExtension
    {
        /// <summary>
        /// Valid the existence of appsseting
        /// </summary>
        /// <param name="this">Represents a type used to build application configuration.</param>
        /// <param name="fisicPath">Represents the physical path of the appsetting</param>
        /// <returns>Build application configuration</returns>
        /// <exception cref="FileNotFoundException">Appsetting does not exist</exception>
        public static IConfigurationBuilder ValidTheExistenceOfAppsseting(this IConfigurationBuilder @this, string fisicPath)
        {
            if (!File.Exists(fisicPath)) throw new FileNotFoundException(fisicPath);
            return @this;
        }


        /// <summary>
        /// Add configuration to the test
        /// </summary>
        /// <param name="this">Represents the root of an <see cref="IConfiguration"/>.</param>
        /// <typeparam name="T">Generic class to baind</typeparam>
        /// <returns></returns>
        public static T AddConfigurationToTheTest<T>(this IConfigurationRoot @this)
            where T : class, new()
        {
            var genericIntance = new T();
            var typeName = genericIntance.GetType().Name;
            @this.GetSection(typeName).Bind(genericIntance);
            return genericIntance;
        }
    }
}