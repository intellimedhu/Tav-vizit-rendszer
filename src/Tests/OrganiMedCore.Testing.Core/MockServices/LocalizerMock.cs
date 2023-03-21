using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public class LocalizerMock<T> : IHtmlLocalizer<T>, IStringLocalizer<T>
    {
        public LocalizedHtmlString this[string name]
            => new LocalizedHtmlString(name, name);

        public LocalizedHtmlString this[string name, params object[] arguments]
            => new LocalizedHtmlString(name, name);

        LocalizedString IStringLocalizer.this[string name]
            => new LocalizedString(name, name);

        LocalizedString IStringLocalizer.this[string name, params object[] arguments]
            => new LocalizedString(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return new LocalizedString[] { };
        }

        public LocalizedString GetString(string name)
            => new LocalizedString(name, name);

        public LocalizedString GetString(string name, params object[] arguments)
            => GetString(name);

        public IHtmlLocalizer WithCulture(CultureInfo culture)
            => this;

        IStringLocalizer IStringLocalizer.WithCulture(CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
