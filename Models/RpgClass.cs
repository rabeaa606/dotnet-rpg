using System.Text.Json.Serialization;

namespace dotnet_rpg.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RpgClass
    {
        Knight = 1, //فارس
        Mage = 2, //بركه
        Cleric = 3, //رجل دين

    }
}