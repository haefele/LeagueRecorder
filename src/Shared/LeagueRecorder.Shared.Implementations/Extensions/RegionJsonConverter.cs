using System;
using LeagueRecorder.Shared.Abstractions;
using Newtonsoft.Json;

namespace LeagueRecorder.Shared.Implementations.Extensions
{
    public class RegionJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var region = (Region)value;
            writer.WriteValue(region.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var region = (string)reader.Value;
            return Region.FromString(region);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (Region).IsAssignableFrom(objectType);
        }
    }
}