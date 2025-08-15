using System.Text.RegularExpressions;

namespace InterneTaakAfhandeling.Common.Helpers
{
    public static partial class ValidationRegexHelper
    {
        [GeneratedRegex(
    @"^([-!#$%&'*+/=?^_`{}|~0-9A-Z]+(\.[-!#$%&'*+/=?^_`{}|~0-9A-Z]+)*)@((?:[A-Z0-9](?:[A-Z0-9-]{0,61}[A-Z0-9])?\.)+(?:[A-Z0-9-]{2,63}(?<!-)))$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        public static partial Regex EmailValidator();
    }
}
