using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliMed.Core.Services
{
    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        private readonly IOptions<IdentityOptions> _identityOptions;


        public PasswordGeneratorService(IOptions<IdentityOptions> identityOptions)
        {
            _identityOptions = identityOptions;
        }


        public string GenerateRandomPassword(int length = 0)
        {
            var finalLength = length == 0 ? _identityOptions.Value.Password.RequiredLength : length;
            var randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
            var rand = new Random(Environment.TickCount);
            var chars = new List<char>();

            if (_identityOptions.Value.Password.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (_identityOptions.Value.Password.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (_identityOptions.Value.Password.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            if (_identityOptions.Value.Password.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }

            for (var i = chars.Count; i < finalLength || chars.Distinct().Count() < _identityOptions.Value.Password.RequiredUniqueChars; i++)
            {
                var rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
