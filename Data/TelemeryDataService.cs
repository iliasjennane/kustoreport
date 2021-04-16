using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
namespace kustoreport.Data
{
    public class TelemeryDataService
    {
       
        const string Cluster = "https://iliasiotdataexplorer.eastus.kusto.windows.net";
        const string Database = "iotdata";
        public Task<List<TelemetryData>> GetStormDataAsync(DateTime startDate, DateTime endDate)
        {
           
            List<TelemetryData> telemetryData = new List<TelemetryData>();

             // The query provider is the main interface to use when querying Kusto.
            // It is recommended that the provider be created once for a specific target database,
            // and then be reused many times (potentially across threads) until it is disposed-of.
            
            var kcsb = new KustoConnectionStringBuilder(Cluster, Database)
                .WithAadUserPromptAuthentication();

            using (var queryProvider = KustoClientFactory.CreateCslQueryProvider(kcsb))
            {
                // The query -- Note that for demonstration purposes, we send a query that asks for two different
                // result sets (HowManyRecords and SampleRecords).
                var query = @$"declare query_parameters(startTime:datetime = datetime({startDate}), endTime:datetime = datetime({endDate})); 
                            VersionedProcessedTelemetry
                            | where timestamp between (startTime  ..  endTime) and site == '52dd1dab-135b-44aa-824e-8db89f36d1d9'
                            | summarize arg_max(revision, *) by  timestamp
                            | count | as HowManyRecords;
                            VersionedProcessedTelemetry
                            | where timestamp between (startTime  ..  endTime) and site == '52dd1dab-135b-44aa-824e-8db89f36d1d9'
                            | summarize arg_max(revision, *) by  timestamp
                            | order by timestamp";

                // It is strongly recommended that each request has its own unique
                // request identifier. This is mandatory for some scenarios (such as cancelling queries)
                // and will make troubleshooting easier in others.
                var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
                using (var reader = queryProvider.ExecuteQuery(query, clientRequestProperties))
                {
                    // Read HowManyRecords
                    while (reader.Read())
                    {
                        var howManyRecords = reader.GetInt64(0);
                    }

                    // Move on to the next result set, SampleRecords
                    reader.NextResult();
                    while (reader.Read())
                    {
                        var telemetryDataItem = new TelemetryData();
                        // Important note: For demonstration purposes we show how to read the data
                        // using the "bare bones" IDataReader interface. In a production environment
                        // one would normally use some ORM library to automatically map the data from
                        // IDataReader into a strongly-typed record type (e.g. Dapper.Net, AutoMapper, etc.)
                        telemetryDataItem.site = reader.GetString(2);
                        telemetryDataItem.timestamp = reader.GetDateTime(0);
                        telemetryDataItem.revision = reader.GetInt32(1);
                        telemetryDataItem.temperature = reader.GetDouble(3);
                        telemetryDataItem.humidity = reader.GetDouble(4);
                        telemetryDataItem.methane = reader.GetDouble(5);
                        telemetryData.Add(telemetryDataItem);
                    }
                }
                return Task.FromResult(telemetryData);
            }
            
        }

        
    }
}