using System;
using System.Net.Mail;

namespace IntelliMed.Core.Extensions
{
    public static class StringExtensions
    {
        public static void ThrowIfNullOrEmpty(this string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value == string.Empty)
            {
                throw new ArgumentException("Argument must not be an empty string.", nameof(value));
            }
        }

        public static bool IsEmail(this string input)
        {
            try
            {
                new MailAddress(input);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
