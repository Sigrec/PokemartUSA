using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using Microsoft.Extensions.Logging;
using PokemartUSABot.Models;

namespace PokemartUSABot
{
    internal static class DistroProductSelector
    {
        public static List<string> HEADERS { get; private set; } = [ "Product Name", "Price", "Status", "Allocation Due", "Street Date" ];
        public static string[] BANDAI_IPS { get; private set; } = ["Bandai", "Dragon Ball Super", "Dragon Ball", "Digimon", "One Piece", "Union Arena", "Gundam"];

        public static string GetSheetUri(string ip, string lang, long distro)
        {
            string startColumn = string.Empty;
            string endColumn = string.Empty;
            string sheetUrl = "https://docs.google.com/spreadsheets/d/1Qj9aV8ae0MJ7MlBLIqYVh55B8_Ydprryy9zDwNoRJyU/export?format=csv";
            ulong sheetGid = 0;
            string sheetRange = string.Empty;
            bool IsPokemon = false;

            if (ip.Equals("Pokémon"))
            {
                IsPokemon = true;
                sheetUrl = "https://docs.google.com/spreadsheets/d/1AnnzLYz1ktCLm0-Mt5o-6p4T8AqE0r2gewv-osqrK0A/export?format=csv";

                if (lang.Equals("English", StringComparison.OrdinalIgnoreCase))
                {
                    sheetGid = 0;

                    (startColumn, endColumn) = distro switch
                    {
                        1 => ("F", "J"),
                        2 => ("L", "P"),
                        3 => ("R", "V"),
                        4 => ("W", "Z"),
                        5 => ("AB", "AE"),
                        _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                    };

                    sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
                }
            }
            else if (ip.Equals("Magic The Gathering"))
            {
                sheetGid = 419743007;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    2 => ("F", "J"),
                    3 => ("K", "O"),
                    5 => ("Q", "U"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Flesh & Blood"))
            {
                sheetGid = 1539072415;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    3 => ("G", "K"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Grand Archive"))
            {
                sheetGid = 1217038948;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    3 => ("G", "K"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Lorcana"))
            {
                sheetGid = 613517122;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    3 => ("G", "K"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}16:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Sorcery"))
            {
                sheetGid = 80389347;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    3 => ("G", "K"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Star Wars Unlimited"))
            {
                sheetGid = 879393505;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Weiß Schwarz"))
            {
                sheetGid = 1882644453;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    3 => ("G", "K"),
                    5 => ("M", "Q"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Yu-Gi-Oh"))
            {
                sheetGid = 103569003;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    2 => ("H", "L"),
                    3 => ("N", "R"),
                    4 => ("T", "X"),
                    5 => ("Z", "AD"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (BANDAI_IPS.Contains(ip))
            {
                sheetGid = 24121953;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    3 => ("G", "L"),
                    5 => ("M", "Q"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("holoLive"))
            {
                sheetGid = 941844023;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    3 => ("G", "K"),
                    5 => ("M", "Q"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Riftbound"))
            {
                sheetGid = 859148835;

                (startColumn, endColumn) = distro switch
                {
                    1 => ("A", "E"),
                    _ => throw new DistroConfigurationException($"Distro #{distro} does not have {ip} product.")
                };

                sheetRange = $"{startColumn}15:{endColumn}&tq=SELECT%20*";
            }
            else if (ip.Equals("Item Request"))
            {
                sheetGid = 1689199249;
            }
            else if (ip.Equals("Supplies"))
            {
                sheetGid = 1234938269;
            }
            return IsPokemon ? $"{sheetUrl}&range={sheetRange}" : $"{sheetUrl}&gid={sheetGid}&range={sheetRange}";
        }

        public static async Task<IEnumerable<ProductRecord>> FetchProductsAsync(string uri, string ip, long distro)
        {
            PokemartUSABot.Logger.LogDebug($"Requesting: {uri}");

            Stream? response = await HttpClientHelper.GetSheetDataWithRetryAsync(uri);
            if (response is null)
            {
                throw new DistroConfigurationException($"No product found for '{ip}' at Distro #{distro}");
            }
            else
            {
                using CsvReader csv = new CsvReader(new StreamReader(response), CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<ProductRecordMap>();
                IEnumerable<ProductRecord> result = [.. csv.GetRecords<ProductRecord>()];

                // Remove blanks
                result = result.Where(r => !string.IsNullOrWhiteSpace(r.Name));

                // Filter Bandai sub-IP products
                if (BANDAI_IPS.Contains(ip))
                {
                    result = result.Where(r => r.Name.Contains(ip, StringComparison.OrdinalIgnoreCase));
                }

                if (!result.Any())
                {
                    throw new DistroConfigurationException($"No product found for '{ip}' at Distro #{distro}");
                }

                return result;
            }
        }

        public static string GetResultsAsAsciiTable(IEnumerable<ProductRecord> productList)
        {
            int NameLength = HEADERS[0].Length, priceLength = HEADERS[1].Length, statusLength = HEADERS[2].Length, allocationDueLength = HEADERS[3].Length, streetDateLength = HEADERS[4].Length;

            foreach (ProductRecord result in productList)
            {
                NameLength = Math.Max(NameLength, result.Name.Length);
                priceLength = Math.Max(priceLength, result.Price.Length);
                statusLength = Math.Max(statusLength, result.Status.Length);
            }
            string NameBars = "━".PadRight(NameLength + 2, '━');
            string priceBars = "━".PadRight(priceLength + 2, '━');
            string statusBars = "━".PadRight(statusLength + 2, '━');
            string allocationDueBars = "━".PadRight(allocationDueLength + 2, '━');
            string streetDateBars = "━".PadRight(streetDateLength + 2, '━');

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("┏{0}┳{1}┳{2}┳{3}┳{4}┓", NameBars, priceBars, statusBars, allocationDueBars, streetDateBars).AppendLine();
            stringBuilder.AppendFormat("┃ {0} ┃ {1} ┃ {2} ┃ {3} ┃ {4} ┃", HEADERS[0].PadRight(NameLength), HEADERS[1].PadRight(priceLength), HEADERS[2].PadRight(statusLength), HEADERS[3].PadRight(allocationDueLength), HEADERS[4].PadRight(streetDateLength)).AppendLine();
            stringBuilder.AppendFormat("┣{0}╋{1}╋{2}╋{3}╋{4}┫", NameBars, priceBars, statusBars, allocationDueBars, streetDateBars).AppendLine();
            foreach (ProductRecord products in productList)
            {
                PokemartUSABot.Logger.LogDebug(products.ToString());
                stringBuilder.AppendFormat("┃ {0} ┃ {1} ┃ {2} ┃ {3} ┃ {4} ┃", products.Name.PadRight(NameLength), products.Price.PadRight(priceLength), products.Status.PadRight(statusLength), products.AllocationDue.PadRight(allocationDueLength), products.StreetDate.PadRight(streetDateLength)).AppendLine();
            }
            stringBuilder.AppendFormat("┗{0}┻{1}┻{2}┻{3}┻{4}┛", NameBars, priceBars, statusBars, allocationDueBars, streetDateBars);

            return stringBuilder.ToString();
        }
    }

    public class DistroConfigurationException : Exception
    {
        // Constructor with default message
        public DistroConfigurationException()
            : base("An error occurred while configuring the distro.")
        { }

        // Constructor with custom message
        public DistroConfigurationException(string message)
            : base(message)
        { }

        // Constructor with custom message and inner exception
        public DistroConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
