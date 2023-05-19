using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DispatcherWeb.Helpers
{
    internal static class Extensions
    {
        /// <summary>
        /// Read a SQL script that is embedded into a resource.
        /// </summary>
        /// <param name="TMigrationSQLScriptType">The migration type the SQL file script is attached to.</param>
        /// <param name="upOrDownScaleFilePrefix">Optional parameter providing a strategy to distinguish between UP or DOWN SQL scripts.</param>
        /// <returns>The content of the SQL file.</returns>
        public static string ReadAndExecuteSql<TMigrationSQLScriptType>(this TMigrationSQLScriptType migrationSQLScriptType, string upOrDownScaleFilePrefix = "")
            where TMigrationSQLScriptType : class
        {
            var assembly = Assembly.GetExecutingAssembly();
            var type = typeof(TMigrationSQLScriptType);
            var migrationAttributeName = type.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(MigrationAttribute)).ConstructorArguments[0].Value.ToString();

            if (!string.IsNullOrEmpty(upOrDownScaleFilePrefix))
            {
                migrationAttributeName += upOrDownScaleFilePrefix;
            }

            var sqlFile = assembly.GetManifestResourceNames().FirstOrDefault(scriptFile => scriptFile.Contains(migrationAttributeName));
            if (string.IsNullOrEmpty(sqlFile))
                return string.Empty;

            using var stream = assembly.GetManifestResourceStream(sqlFile);
            using StreamReader reader = new(stream);
            var sqlScript = reader.ReadToEnd();
            return sqlScript;
        }

    }
}
