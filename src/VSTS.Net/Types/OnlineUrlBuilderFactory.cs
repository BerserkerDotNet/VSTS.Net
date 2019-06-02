using VSTS.Net.Interfaces;

namespace VSTS.Net.Types
{
    public class OnlineUrlBuilderFactory : IVstsUrlBuilderFactory
    {
        private readonly string _instance;
        private readonly string _subDomain;

        public OnlineUrlBuilderFactory(string instance, string subDomain = "")
        {
            _instance = instance;
            _subDomain = subDomain;
        }

        public VstsUrlBuilder Create(string subDomain = "")
        {
            var sd = string.IsNullOrEmpty(subDomain) ? _subDomain : subDomain;
            return VstsUrlBuilder.Create(_instance, sd);
        }
    }
}
