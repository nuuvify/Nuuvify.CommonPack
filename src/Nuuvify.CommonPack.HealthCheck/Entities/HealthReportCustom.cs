using System;
using System.Collections.Generic;

namespace Nuuvify.CommonPack.HealthCheck
{

    public class HealthReportCustom
    {

        public string Uri { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string TotalDuration { get; set; }
        public DateTimeOffset OnStateFrom { get; set; }
        public DateTimeOffset LastExecuted { get; set; }
        public List<HealthReportEntryCustom> Entries { get; set; }
        public string DiscoveryService { get; set; }


        public Dictionary<string, object> DataEntries()
        {
            var entries = new Dictionary<string, object>();

            foreach (var item in Entries)
            {
                if (item != null && item?.Data?.Count > 0)
                {
                    foreach (var data in item?.Data)
                    {
                        entries.TryAdd(data.Key, data.Value);
                    }
                }
                else
                {
                    entries.TryAdd(item?.Name, $"{item?.Status} {item?.Description} {item?.ExceptionMessage}");
                }

            }

            return entries;
        }

    }

}