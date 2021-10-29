using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMA.OIM.AzureFunctions
{
    class Response
    {
        public bool success { get; set; }
        public dynamic data { get; set; }
        public string error { get; set; }

        public Response()
        {
            success = true;
            error = null;
            data = null;
        }

        public string GetJSON()
        {
            return JsonConvert.SerializeObject(
                this,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            );
        }
    }
}
