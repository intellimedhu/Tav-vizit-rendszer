namespace IntelliMed.Core.Constants
{
    public static class RegexPatterns
    {
        public const string Taj = @"^(\d{3}\-){2}(\d{3})$";
        public const string Email = @"^(?![\.@])(""([^""\r\\]|\\[""\r\\])*""|([-\p{L}0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@([a-z0-9][\w-]*\.)+[a-z]{2,}$";
        public const string Phone = @"^\d{7,11}$";
        public const string DoctorStampNumber = @"^(\d{5}|\d{7})$";
        public const string AntszLicenseNumber = @"^[0-9A-Za-z]{1,20}$";
    }
}
