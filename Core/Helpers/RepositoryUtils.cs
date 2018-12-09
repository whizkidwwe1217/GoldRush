using System;
using System.IO;
using System.Linq;
using System.Text;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoldRush.Core
{
    public static class RepositoryUtils
    {
        public static object SerializeSelect<TEntity>(this object data, string fields)
        {
            return Serialize<TEntity>(data, fields);
        }

        public static object Serialize<TEntity>(object data, string fields)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                var serializer = new JsonSerializer
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                serializer.Serialize(writer, data);
            }

            var field = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
            string[] fieldsArray = field.Select(f => ToCamelCase(f.Trim())).ToArray();
            var json = JObject.Parse(sb.ToString());
            var newJson = new JObject();

            foreach (var f in fieldsArray)
            {
                var value = json.SelectToken(f);
                var fieldName = ToLowerCamelCase(f, '.');
                newJson.Add(fieldName, value);
            }
            return newJson.ToObject<TEntity>();
        }

        public static string ToLowerCamelCase(string path, char separator)
        {
            var names = path.Split(separator);
            StringBuilder sb = new StringBuilder();
            foreach (var name in names)
            {
                sb.Append(ToLowerCamelCase(name));
                sb.Append(separator);
            }
            return sb.Remove(sb.Length-1, 1).ToString();
        }

        public static string ToLowerCamelCase(string input)
        {
            string[] words = input.Split(' ');
            StringBuilder sb = new StringBuilder();
            foreach (string s in words)
            {
                string firstLetter = s.Substring(0, 1);
                string rest = s.Substring(1, s.Length - 1);
                sb.Append(firstLetter.ToLower() + rest);
                sb.Append(' ');

            }
            return sb.ToString().Substring(0, sb.ToString().Length - 1);
        }

        public static string ToCamelCase(string input)
        {
            string[] words = input.Split(' ');
            StringBuilder sb = new StringBuilder();
            foreach (string s in words)
            {
                string firstLetter = s.Substring(0, 1);
                string rest = s.Substring(1, s.Length - 1);
                sb.Append(firstLetter.ToUpper() + rest);
                sb.Append(' ');

            }
            return sb.ToString().Substring(0, sb.ToString().Length - 1);
        }

        public static Exception CreateInnerException<TEntity, TKey>(this BaseRepository<TKey, TEntity> repository, string message, Exception exception)
            where TEntity : class, IBaseEntity<TKey>, ICanAudit, new()
        {
            return new Exception($"{message}. Error details: {exception.Message} {(exception.InnerException != null ? " " + exception.InnerException.Message : "")}");
        }
    }
}