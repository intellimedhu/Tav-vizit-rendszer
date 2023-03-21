using System;

namespace IntelliMed.Core.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> whether the value is null
        /// </summary>
        /// <param name="value">The value to check.</param>
        public static void ThrowIfNull(this object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
