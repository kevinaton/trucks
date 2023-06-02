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
        public static string ReadAndExecuteSql<MigrationBuilderType>(this MigrationBuilderType migrationBuilder, string upOrDownScaleFilePrefix = "")
            where MigrationBuilderType : MigrationBuilder
        {
            var assembly = Assembly.GetExecutingAssembly();
            var type = typeof(MigrationBuilderType);
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

            migrationBuilder.Sql(sqlScript);

            return sqlScript;
        }

    }
}
