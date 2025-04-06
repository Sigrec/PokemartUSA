using System.Text;
using System.Text.Json.Serialization;

namespace PokemartUSABot.Config
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(PokemartUSABotConfig))]
    internal sealed partial class PokemartUSABotConfigContext : JsonSerializerContext { }

    internal sealed class PokemartUSABotConfig
    {
        public required string Token { get; set; }

        internal string EncodeToken()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Token));
        }

        internal string DecodeToken()
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(Token));
            }
            catch (FormatException)
            {
                //Console.WriteLine("Invalid Base64 Token.");
                return string.Empty; // Return empty string on failure
            }
        }
    }
}
