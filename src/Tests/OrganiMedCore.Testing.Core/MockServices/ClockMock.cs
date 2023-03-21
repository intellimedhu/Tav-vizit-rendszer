using OrchardCore.Modules;
using System;

namespace OrganiMedCore.Testing.Core.MockServices
{
    public class ClockMock : IClock
    {
        private readonly DateTime? _utcNow;


        public DateTime UtcNow => _utcNow ?? DateTime.UtcNow;


        public ClockMock(DateTime? utcNow = null)
        {
            _utcNow = utcNow;
        }


        public DateTimeOffset ConvertToTimeZone(DateTimeOffset dateTimeOffset, ITimeZone timeZone)
        {
            throw new NotImplementedException();
        }

        public ITimeZone GetSystemTimeZone()
        {
            throw new NotImplementedException();
        }

        public ITimeZone GetTimeZone(string timeZoneId)
        {
            throw new NotImplementedException();
        }

        public ITimeZone[] GetTimeZones()
        {
            throw new NotImplementedException();
        }
    }
}
