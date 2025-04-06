using CsvHelper.Configuration;
using System.Globalization;

namespace PokemartUSABot.Models
{
    public class ItemRequestProductRecord
    {
        public required string Name { get; set; }
        public required string Price { get; set; }
        public required string Status { get; set; }
        public required string AllocationDueDate { get; set; }
        public required string StreetDate { get; set; }
        public required string LastUpdatedDate { get; set; }
        public required ushort DistroNumber { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Price: {Price}, Status: {Status}, AllocationDue: {AllocationDueDate}, StreetDate: {StreetDate}, AllocationDueDate: {AllocationDueDate}, Distro #{DistroNumber}";
        }
    }

    public sealed class ItemRequestProductRecordMap : ClassMap<ItemRequestProductRecord>
    {
        public ItemRequestProductRecordMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.AllocationDueDate).Name("Allocation Date");
            Map(m => m.StreetDate).Name("Street Date");
            Map(m => m.LastUpdatedDate).Name("Last updated");
            Map(m => m.DistroNumber).Name("Distro #");
        }
    }
}