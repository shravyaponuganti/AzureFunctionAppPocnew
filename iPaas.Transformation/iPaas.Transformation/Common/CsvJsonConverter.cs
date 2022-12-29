using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Common
{
    public class CsvJsonConverter
    {
        public static async Task<string> GetJsonAsync(Stream stream, string targetJsonFormat)
        {
            if (string.IsNullOrEmpty(targetJsonFormat) || stream is null)
            {
                return string.Empty;
            }
            var records = await GetDynamicListAsync(stream, targetJsonFormat);

            if (!records.Any())
            {
                return string.Empty;
            }

            if (records.Count > 1)
            {
                return JsonConvert.SerializeObject(records);
            }
            else
            {
                return JsonConvert.SerializeObject(records.FirstOrDefault());
            }
        }

        private static async Task<List<dynamic>> GetDynamicListAsync(Stream stream, string targetJsonFormat)
        {
            if (stream is null || string.IsNullOrEmpty(targetJsonFormat))
            {
                return null;
            }

            List<dynamic> records = new List<dynamic>();
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var mappings = GetTemplateMappings(targetJsonFormat);
                if (!mappings.Any())
                {
                    return null;
                }
                while (await csv.ReadAsync())
                {
                    var record = CreateDynamicObject(mappings, (IDictionary<string, object>)csv.GetRecord<dynamic>());
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
            }
            return records;
        }

        private static Dictionary<string, object> GetMappings(Dictionary<string, object> datap)
        {
            Dictionary<string, object> list = new Dictionary<string, object>();
            foreach (var item in datap)
            {
                if (item.Value is string)
                {
                    list.Add(item.Key, item.Value);
                }
                else if (item.Value.GetType() == typeof(JArray))
                {
                    list.Add(item.Key, ConvertArraytoDictionary(item.Value));
                }
                else if (item.Value.GetType() == typeof(JObject))
                {
                    list.Add(item.Key, ConvertObjecttoDictionary(item.Value));
                }
            }
            return list;
        }

        public static List<IDictionary<string, object>> ConvertArraytoDictionary(dynamic jarray)
        {
            List<IDictionary<string, object>> myDictionary = new List<IDictionary<string, object>>();

            foreach (JObject content in jarray.Children<JObject>())
            {
                myDictionary.Add(ConvertObjecttoDictionary(content));
            }
            return myDictionary;
        }

        public static IDictionary<string, object> ConvertObjecttoDictionary(dynamic content)
        {
            IDictionary<string, object> myDictionary = new Dictionary<string, object>();

            foreach (JProperty prop in content.Properties())
            {
                if (!myDictionary.ContainsKey(prop.Name))
                {
                    myDictionary.Add(prop.Name, prop.Value);
                }
            }
            return myDictionary;
        }

        private static ExpandoObject CreateDynamicObject(IDictionary<string, object> jsonFormat, IDictionary<string, object> csvRow)
        {
            var newobj = new ExpandoObject();
            foreach (var item in jsonFormat)
            {
                var csvData = csvRow.Keys.Contains(item.Key) ? csvRow[item.Key] : String.Empty;
                if (item.Value is KeyValuePair)
                {
                    newobj.TryAdd(item.Value?.ToString(), csvData);
                }
                else if (item.Value is JValue)
                {
                    newobj.TryAdd(item.Value?.ToString(), csvData);
                }
                else if (item.Value is string)
                {
                    newobj.TryAdd(item.Value?.ToString(), csvData);
                }
                else if (item.Value is IDictionary<string, object>)
                {
                    newobj.TryAdd(item.Key, CreateDynamicObject((IDictionary<string, object>)item.Value, csvRow));
                }
                else if (item.Value is List<IDictionary<string, object>>)
                {
                    List<ExpandoObject> list = new List<ExpandoObject>();
                    foreach (var cObj in (List<IDictionary<string, object>>)item.Value)
                    {
                        list.Add(CreateDynamicObject(cObj, csvRow));
                    }
                    newobj.TryAdd(item.Key?.ToString(), list);
                }
            }
            return newobj;
        }

        public static string GetJson(Stream stream, string targetJsonFormat)
        {
            if (string.IsNullOrEmpty(targetJsonFormat) || stream is null)
            {
                return string.Empty;
            }

            var records = GetDynamicList(stream, targetJsonFormat);
            if (!records.Any())
            {
                return string.Empty;
            }

            if (records.Count > 1)
            {
                return JsonConvert.SerializeObject(records);
            }
            else
            {
                return JsonConvert.SerializeObject(records.FirstOrDefault());
            }
        }

        private static List<dynamic> GetDynamicList(Stream stream, string targetJsonFormat)
        {
            List<dynamic> records = new List<dynamic>();
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var mappings = GetTemplateMappings(targetJsonFormat);
                if (!mappings.Any())
                {
                    return null;
                }
                while (csv.Read())
                {
                    var record = CreateDynamicObject(mappings, (IDictionary<string, object>)csv.GetRecord<dynamic>());
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
            }
            return records;
        }

        private static Dictionary<string, object> GetTemplateMappings(string jsonTemplate)
        {
            var tmp_mappings = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonTemplate);
            var mappings = GetMappings(tmp_mappings);
            return mappings;
        }

        private static IList<string> flattenJSON(JObject obj, IList<string> path)
        {
            var output = new List<string>();
            foreach (var prop in obj.Properties())
            {
                if (prop.Value.Type == JTokenType.Object)
                {
                    output.AddRange(flattenJSON(prop.Value as JObject, new List<string>(path) { prop.Name }));
                }
                else
                {
                    var s = string.Join(".", new List<string>(path) { prop.Name }) + " = " + prop.Value.ToString();
                    output.Add(s);
                }
            }
            return output;
        }

        public static async Task<List<string>> GetJsonListAsync(Stream stream, string targetJsonFormat)
        {
            if (string.IsNullOrEmpty(targetJsonFormat) || stream is null)
            {
                return new List<string>();
            }
            var records = await GetDynamicListAsync(stream, targetJsonFormat);
            return GetDynamicJsonObjects(records);
        }

        public static List<string> GetJsonList(Stream stream, string targetJsonFormat)
        {
            if (string.IsNullOrEmpty(targetJsonFormat) || stream is null)
            {
                return new List<string>();
            }
            var records = GetDynamicList(stream, targetJsonFormat);
            return GetDynamicJsonObjects(records);
        }

        private static List<string> GetDynamicJsonObjects(List<dynamic> records)
        {
            var listofJsonobjects = new List<string>();
            if (records is null)
            {
                return listofJsonobjects;
            }

            foreach (var record in records)
            {
                if (record != null)
                {
                    listofJsonobjects.Add(JsonConvert.SerializeObject(record));
                }
            }
            return listofJsonobjects;
        }
    }
}