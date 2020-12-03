﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    public class DictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);

            foreach (var key in dictionary.Keys.ToList())
            {
                if (dictionary[key] is JsonElement je)
                {
                    dictionary[key] = Unwrap(je);
                }
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, options);

        private static object Unwrap(JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.ToString(),
                JsonValueKind.Number => je.TryGetInt64(out var l) ? l : je.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => je.EnumerateArray().Select(Unwrap).ToList(),
                JsonValueKind.Object => je.EnumerateObject().ToDictionary(x => x.Name, x => Unwrap(x.Value)),
                _ => null
            };
        }
    }
}
