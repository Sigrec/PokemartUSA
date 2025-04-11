namespace PokemartUSABot.Models
{
    public class GoogleSheetsPayload
    {
        public required string Reason { get; set; }
        public required Dictionary<string, string> Data { get; set; }

        public override string ToString()
        {
            string? dataEntries = string.Join(", ", Data.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            return $"Reason: {Reason}, Data: {{ {dataEntries} }}";
        }
    }
}
