using CsvHelper.Configuration;
using System.Globalization;

namespace PokemartUSABot.Models
{
    public class ProductRecord
    {
        public required string Name { get; set; }
        public required string Price { get; set; }
        public required string Status { get; set; }
        public required string AllocationDueDate { get; set; }
        public required string StreetDate { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Price: {Price}, Status: {Status}, AllocationDue: {AllocationDueDate}, StreetDate: {StreetDate}";
        }
    }

    public sealed class ProductRecordMap : ClassMap<ProductRecord>
    {
        public ProductRecordMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.AllocationDueDate).Name("Pre-order Deadline");
            Map(m => m.StreetDate).Name("Street Date");
        }
    }
}