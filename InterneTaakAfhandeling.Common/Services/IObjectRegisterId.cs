using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Services
{
    public interface IObjectRegisterId
    {
        string CodeSoortObjectId { get; }
        string CodeObjecttype { get; }
        string CodeRegister { get; }
    }

    public static class ObjectRegisterIdExtensions
    {
        public static bool Matches([NotNullWhen(true)] this IObjectRegisterId? left, IObjectRegisterId right) =>
            left?.CodeRegister == right.CodeRegister &&
            left.CodeObjecttype == right.CodeObjecttype &&
            left.CodeSoortObjectId == right.CodeSoortObjectId;
    }
}
