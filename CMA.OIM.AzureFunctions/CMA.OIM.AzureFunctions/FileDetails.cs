using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMA.OIM.AzureFunctions
{
    class FileDetails
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
