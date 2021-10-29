using Microsoft.SharePoint.Client;
using PnP.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CMA.OIM.AzureFunctions
{
    class Utilities
    {
        internal static async System.Threading.Tasks.Task<ClientContext> GetOIMPortalContextAsync()
        {
            string sharePointUrl = System.Environment.GetEnvironmentVariable("OIM_SITE_COLLECTION");
            return await GetContextAsync(sharePointUrl);
        }
        internal static async System.Threading.Tasks.Task<ClientContext> GetContextAsync(string sharePointUrl)
        {
            string clientId = System.Environment.GetEnvironmentVariable("CLIENT_ID");
            string tenantId = System.Environment.GetEnvironmentVariable("TENANT_ID");
            string certThumb = System.Environment.GetEnvironmentVariable("CERT_THUMBPRINT");

            AuthenticationManager auth = new AuthenticationManager(clientId, StoreName.My, StoreLocation.CurrentUser, certThumb, tenantId);

            return await auth.GetContextAsync(sharePointUrl);
        }

        internal static ClientContext GetOIMPortalContext()
        {
            string sharePointUrl = System.Environment.GetEnvironmentVariable("OIM_SITE_COLLECTION");
            return GetContext(sharePointUrl);
        }

        internal static ClientContext GetContext(string sharePointUrl)
        {
            string clientId = System.Environment.GetEnvironmentVariable("CLIENT_ID");
            string tenantId = System.Environment.GetEnvironmentVariable("TENANT_ID");
            string certThumb = System.Environment.GetEnvironmentVariable("CERT_THUMBPRINT");

            AuthenticationManager auth = new AuthenticationManager(clientId, StoreName.My, StoreLocation.CurrentUser, certThumb, tenantId);

            return auth.GetContext(sharePointUrl);
        }
    }
}
