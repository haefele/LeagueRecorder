using System;
using LeagueRecorder.Shared.Abstractions;
using Newtonsoft.Json;

namespace LeagueRecorder.Shared.Implementations.Extensions
{
    public class TeamJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var team = (Team)value;
            writer.WriteValue(team.TeamId);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var teamId = (long)reader.Value;
            return Team.FromTeamId(teamId);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (Team).IsAssignableFrom(objectType);
        }
    }
}