using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nuuvify.CommonPack.HealthCheck
{
    public class HealthReportEntryCustom
    {
        public string Name { get; set; }
        public string Component { get; set; }

        public Dictionary<string, object> Data { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }

        [JsonIgnore]
        public Exception Exception
        {
            get => null;
            set => ExceptionMessage = value.Message;
        }
        public string ExceptionMessage { get; set; }

        public string Status { get; set; }
        public IEnumerable<string> Tags { get; set; }
        
    }


}