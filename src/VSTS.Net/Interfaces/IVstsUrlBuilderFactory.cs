using System;
using VSTS.Net.Types;

namespace VSTS.Net.Interfaces
{
    public interface IVstsUrlBuilderFactory
    {
        VstsUrlBuilder Create(string subDomain = "");
    }
}
