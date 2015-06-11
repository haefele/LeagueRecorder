using Newtonsoft.Json;

namespace LeagueRecorder.Shared.Implementations.Extensions
{
    public static class LeagueJsonSerializerSettings
    {
        public static JsonSerializerSettings Get()
        {
            return new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new RegionJsonConverter(), new TeamJsonConverter() }
            };
        }
    }
}