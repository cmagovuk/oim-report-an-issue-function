using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMA.OIM.AzureFunctions
{
    static class Test
    {
        static internal void List(Response response, ILogger log)
        {

            string submission_list = System.Environment.GetEnvironmentVariable("SUBMISSIONS_LIST");

            using (ClientContext ctx = Utilities.GetOIMPortalContext())
            {
                List list = ctx.Web.Lists.GetByTitle(submission_list);
                ctx.Load(list);
                ctx.ExecuteQueryRetry();

                // Return numbr of items
                response.data = $"Item count: {list.ItemCount}";
            }

            response.success = true;
        }
        private static string TranslateOne(string term, string translateVar)
        {
            return Translate(term, Translation(translateVar));
        }

        private static List<string> TranslateMulti(dynamic terms, string translateVar)
        {
            Dictionary<string, string> translations = Translation(translateVar);
            List<string> results = new List<string>();
            foreach (var item in terms)
            {
                if (!string.IsNullOrEmpty((string)item))
                {
                    results.Add(Translate((string)item, translations));
                }
            }
            return results;
        }

        private static string Translate(string term, Dictionary<string, string> translations)
        {
            if (translations.ContainsKey(term))
            {
                return translations[term];
            }
            return "Unknown value";
        }

        private static Dictionary<string, string> Translation(string translateVar)
        {
            Dictionary<string, string> terms = new Dictionary<string, string>();
            string translateStr = System.Environment.GetEnvironmentVariable(translateVar);
            if (!string.IsNullOrEmpty(translateStr))
            {
                foreach (string item in translateStr.Split(";;", StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] kvp = item.Split("::", StringSplitOptions.None);
                    if (!terms.ContainsKey(kvp[0]))
                    {
                        terms.Add(kvp[0], kvp[1]);
                    }
                }
            }
            return terms;
        }
    }
}
