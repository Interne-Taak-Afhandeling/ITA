using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Services
{
    public interface IObjectRegisterId
    {
        string CodeSoortObjectId { get; init; }
        string CodeObjecttype { get; init; }
        string CodeRegister { get; init; }
    }
}
