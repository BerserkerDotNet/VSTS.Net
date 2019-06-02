using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSTS.Net.Interfaces;
using VSTS.Net.Models.Identity;
using VSTS.Net.Models.Response;
using VSTS.Net.Types;

namespace VSTS.Net
{
    public partial class VstsClient : IVstsClient
    {
        public async Task<IEnumerable<Identity>> GetIdentitiesAsync(string filterValue, bool onlyActive = false, string searchFilter = Constants.IdentityDefaultSearchFilter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(searchFilter))
            {
                throw new ArgumentNullException(nameof(searchFilter));
            }

            if (string.IsNullOrEmpty(filterValue))
            {
                throw new ArgumentNullException(nameof(filterValue));
            }

            var url = _urlBuilderFactory.Create(Constants.IdentitySubDomain)
                .ForIdentities()
                .WithQueryParameter("searchFilter", "General")
                .WithQueryParameter("filterValue", filterValue)
                .Build();

            var identitiesResponse = await _httpClient.ExecuteGet<CollectionResponse<Identity>>(url, cancellationToken);
            var identities = identitiesResponse?.Value ?? Enumerable.Empty<Identity>();
            if (onlyActive)
            {
                identities = identities.Where(i => i.IsActive).ToArray();
            }

            return identities;
        }
    }
}
