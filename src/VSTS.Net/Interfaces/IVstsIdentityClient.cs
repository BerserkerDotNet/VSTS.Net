using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Models.Identity;
using VSTS.Net.Types;

namespace VSTS.Net.Interfaces
{
    public interface IVstsIdentityClient
    {
        Task<IEnumerable<Identity>> GetIdentitiesAsync(string filterValue, bool onlyActive = false, string searchFilter = Constants.IdentityDefaultSearchFilter, CancellationToken cancellationToken = default(CancellationToken));
    }
}
