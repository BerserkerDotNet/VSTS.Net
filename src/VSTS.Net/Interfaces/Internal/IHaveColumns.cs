using System.Collections.Generic;
using VSTS.Net.Models.Response;

namespace VSTS.Net.Interfaces.Internal
{
    internal interface IHaveColumns
    {
        IEnumerable<ColumnReference> Columns { get; set; }
    }
}
