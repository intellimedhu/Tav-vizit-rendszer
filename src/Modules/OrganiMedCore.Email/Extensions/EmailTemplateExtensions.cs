using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Extensions
{
    public static class EmailTemplateExtensions
    {
        public static string ReplaceToken(this string subject, string token, string newValue)
            => subject.Replace($"-{token}-", newValue);

        public static async Task<string> ReplaceTokenAsync(this string subject, string token, Func<Task<string>> replacementProcess)
            => subject.IndexOf($"-{token}-") > -1
                ? subject.ReplaceToken(token, await replacementProcess())
                : subject;
    }
}
