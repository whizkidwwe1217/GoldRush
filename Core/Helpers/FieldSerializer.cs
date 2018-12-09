using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GoldRush.Core
{
    public class FieldSerializer : DefaultContractResolver
    {
        private readonly string[] _fields;

        public FieldSerializer(string fields)
        {
            var fieldColl = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
            _fields = fieldColl
                .Select(f => f.ToLower().Trim())
                .ToArray();
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = o => _fields.Contains(member.Name.ToLower());

            return property;
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            // This overrides the character-casing returned by the resolver and uses the camel casing.
            return $"{propertyName.Substring(0, 1).ToLowerInvariant()}{propertyName.Substring(1, propertyName.Length-1)}";
        }
    }
}