using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Machete.Data.Tenancy
{
    public interface ITenantIdentificationService
    {
        string GetCurrentTenant(HttpContext httpContext);
    }

    public class TenantIdentificationService : ITenantIdentificationService
    {
        private readonly TenantMapping _tenants;
        private bool _defaultAllowed;

        public TenantIdentificationService(IConfiguration configuration)
        {
            _tenants = configuration.GetTenantMapping();
            _defaultAllowed = _tenants.AllowDefault;
        }

        public string GetCurrentTenant(HttpContext context)
        {
            var host = context?.Request.Host.Value;
            
            var tenantName = _tenants.Tenants.FirstOrDefault(tenant => tenant.Key.Equals(host)).Value ?? _tenants.Default;
            
            if (tenantName == _tenants.Default && !_defaultAllowed)
                throw new UnauthorizedAccessException("Requested default provider, but default not allowed. You have probably misconfigured a production host. Contact your administrator for assistance.");
            
            return tenantName;
        }
    }
}
