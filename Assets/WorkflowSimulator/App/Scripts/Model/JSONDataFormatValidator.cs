using Newtonsoft.Json;
using System.Collections.Generic;

namespace yourvrexperience.WorkDay
{
    public interface IJsonValidatable
    {
        bool IsValid();
    }


    public static class JSONDataFormatValidator
    {
        [System.Serializable]
        public class JsonListWrapper<T>
        {
            public List<T> items;
        }

        public static string CleanJsonResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return string.Empty;

            string cleaned = response.Replace("```json", "")
                                     .Replace("```", "")
                                     .Trim();

            return cleaned;
        }


        public static bool ValidateJsonItem<T>(T item) where T : IJsonValidatable
        {
            return item.IsValid();
        }

        public static bool ValidateJsonItem<T>(string jsonString) where T : IJsonValidatable
        {
            T item;
            try
            {
                item = JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                return false;
            }

            return item.IsValid();
        }

        public static bool ValidateJsonList<T>(string jsonString) where T : IJsonValidatable
        {
            List<T> dataList = null;
            try
            {
                // If your JSON is like: { "items": [ ... ] }
                var wrapper = JsonConvert.DeserializeObject<JsonListWrapper<T>>(jsonString);
                dataList = wrapper?.items;

                // If your JSON is a plain array: [ ... ]
                if (dataList == null && jsonString.TrimStart().StartsWith("["))
                {
                    dataList = JsonConvert.DeserializeObject<List<T>>(jsonString);
                }
            }
            catch
            {
                return false;
            }

            foreach (T item in dataList)
            {
                if (!ValidateJsonItem(item))
                {
                    return false;
                }
            }

            return true;
        }
    }
}