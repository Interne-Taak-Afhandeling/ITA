using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Helpers
{
    public static partial class ValidationRegexHelper
    {
        [GeneratedRegex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            "en-US")]
        public static partial Regex EmailValidator();
    }
}
