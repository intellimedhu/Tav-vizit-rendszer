using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliMed.Core.Services
{
    /// <summary>
    /// Help working with generated passwords.
    /// </summary>
    public interface IPasswordGeneratorService
    {
        /// <summary>
        /// Generates a password base of the password settings (IdentityOptions).
        /// </summary>
        /// <param name="length">Override the length in settings if necessary.</param>
        /// <returns>A random password.</returns>
        string GenerateRandomPassword(int length = 0);
    }
}
