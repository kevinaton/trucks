using System.Linq;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class ShiftExtensions
    {
        public static Shift?[] ToNullableArrayWithNullElementIfEmpty(this Shift[] shifts) =>
            shifts?.ToNullIfEmpty()?.Select(x => (Shift?)x).ToArray() ?? new Shift?[] { null };

        private static Shift[] ToNullIfEmpty(this Shift[] shifts) => shifts.Length != 0 ? shifts : null;
    }
}
