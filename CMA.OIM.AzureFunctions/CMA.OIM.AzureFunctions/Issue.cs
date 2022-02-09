using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMA.OIM.AzureFunctions
{
    static class Issue
    {
        static internal void Submit(Response response, ILogger log, dynamic record, dynamic documents)
        {
            log.LogInformation($"record = '{record}'");
            log.LogInformation($"documents = '{documents}'");

            string submission_list = System.Environment.GetEnvironmentVariable("SUBMISSIONS_LIST");

            using (ClientContext ctx = Utilities.GetOIMPortalContext())
            {
                List list = ctx.Web.Lists.GetByTitle(submission_list);

                ListItemCreationInformation lici = new ListItemCreationInformation();
                ListItem listItem = list.AddItem(lici);
                //listItem[Constants.FIELD_TITLE] = id;
                listItem[Constants.OIM_FIRST_NAME] = (string)record.first_name;
                listItem[Constants.OIM_LAST_NAME] = (string)record.last_name;
                if (!String.IsNullOrWhiteSpace((string)record.email)) {
                    listItem[Constants.OIM_EMAIL_ADDRESS] = ((string)record.email).ToLower();
                }
                listItem[Constants.OIM_TELEPHONE] = (string)record.telephone;
                string reporting_as = (string)record.reporting_as;
                listItem[Constants.OIM_REPORTING_AS] = TranslateOne(reporting_as, "TRANSLATE_REPORTING_AS");   // Requires translation
                listItem[Constants.OIM_REPORTING_AS_OTHER] = TruncateString((string)record.reporting_other);
                listItem[Constants.OIM_LOCATION_POSTCODE] = (string)record.addr_postcode;

                if (reporting_as != "ind" && reporting_as != "other")
                {
                    listItem[Constants.OIM_ORG_NAME] = (string)record.org_name;
                    listItem[Constants.OIM_AREA_OF_OPS] = TranslateMulti(record.area_of_ops, "TRANSLATE_AREA_OF_OPS"); // Requires translation
                    listItem[Constants.OIM_BUSINESS_SECTOR] = (string)record.business_sector;
                }

                listItem[Constants.OIM_ISSUE] = (string)record.issue;
                List<string> impact_areas = TranslateMulti(record.impact_area, "TRANSLATE_IMPACT_AREA");
                listItem[Constants.OIM_ISSUE_IMPACT] = impact_areas; // Requires translation
                listItem[Constants.OIM_ISSUE_IMPACT_OTHER] = TruncateString((string)record.impact_other);
                listItem[Constants.OIM_SUBMISSION_ORIGIN] = "External";  // Fixed == External?
                listItem[Constants.OIM_SUBMISSION_MANIFEST] = FormatDocuments(documents);

                listItem.Update();
                ctx.ExecuteQueryRetry();

                // Get list id
                response.data = $"OIM{listItem.Id}";
            }

            response.success = true;
        }

        private static string TruncateString(string text)
        {
            if (!string.IsNullOrEmpty(text) && text.Length > 255) {
                return text.Substring(0, 255).Trim();
            }
            return text;
        }

        private static string FormatDocuments(dynamic documents)
        {
            Dictionary<string, string> files = new Dictionary<string, string>();
            List<string> filenames = new List<string>();
            List<FileDetails> fileDetails = new List<FileDetails>();
            if (documents is Newtonsoft.Json.Linq.JArray)
            {
                foreach (dynamic item in documents as Newtonsoft.Json.Linq.JArray)
                {
                    if (item.ContainsKey("key") && item.ContainsKey("filename"))
                    {
                        int index = 1;
                        string filename = GetSafeFilename((string)item.filename);
                        string initFilename = System.IO.Path.GetFileNameWithoutExtension(filename);
                        string extension = System.IO.Path.GetExtension((string)item.filename);
                        while (filenames.Contains(filename))
                        {
                            filename = $"{initFilename} ({index++}){extension}";
                        }
                        filenames.Add(filename);
                        fileDetails.Add(new FileDetails() { Filename = filename, Key = (string)item.key });
                    }
                }
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(fileDetails);
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

        private static string GetSafeFilename(string filename)
        {
            string safeName = System.IO.Path.GetFileNameWithoutExtension(filename);
            char[] ends = { '.', ' ' };
            safeName = safeName.Trim(ends);

            //Double periods in file name is invalid
            safeName = System.Text.RegularExpressions.Regex.Replace(safeName, @"\.+", ".");
            safeName = System.Text.RegularExpressions.Regex.Replace(safeName, @"[""*:<>?/\\|\t]", "_");

            safeName += System.IO.Path.GetExtension(filename).Trim();

            return safeName;
        }
    }
}
