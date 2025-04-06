using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using PokemartUSABot.Models;
using System.Text;

namespace PokemartUSABot
{
    [SlashCommandGroup(PokemartUSABot.NAME, "PokemartUSA Bot Commands")]
    public class PokemartUSABotCommands : ApplicationCommandModule
    {
        // TODO - User unable (only mod) to delete final messages, find a better way of outputting messages?
        // TODO - Add link to changelog/updates to help command or create new command?
        // TODO - Issue where "Is Running" stays when there is a error
        //[SlashCommand("start", "Start PokemartUSABot")]
        //[SlashCooldown(1, 90, SlashCooldownBucketType.User)]
        //public async Task PokemartUSABotCommand(
        //    InteractionContext ctx,
        //    [Option("Product", "Enter Product Name")] string product,
        //    [Choice("Pokémon", "Pokémon")][Choice("Magic The Gathering", "Magic The Gathering")][Choice("Flesh & Blood", "Flesh & Blood")][Choice("Grand Archive", "Grand Archive")][Choice("holoLive", "holoLive")][Choice("Lorcana", "Lorcana")][Choice("Riftbound", "Riftbound")][Choice("Sorcery", "Sorcery")][Choice("Star Wars Unlimited", "Star Wars Unlimited")][Choice("Weiß Schwarz", "Weiß Schwarz")][Choice("Bandai", "Bandai")][Choice("Supplies", "Supplies")] string ip,
        //    [Choice("Southern Hobby (Distro #1)", 1)][Choice("Magazine Exchange (Distro #2)", 2)][Choice("PHD Games (Distro #3)", 3)][Choice("Madal (Distro #4)", 4)][Choice("GTS Distribution (Distro #5)", 5)] ushort distro,
        //    [Option("DM", "Direct Message Results?")] bool dm, 
        //    [Option("Mobile", "Print results in a mobile friendly format")] bool mobile = false)
        //{
        //    // Get input for the scrape from user
        //    ctx.SlashCommandsExtension.SlashCommandErrored += OnErrorOccured;

        //}

        [SlashCommand("product", "Get current list of products for a given IP and distro")]
        [SlashCooldown(1, 5, SlashCooldownBucketType.User)]
        public async Task ListPokemartUSADistroProductCommand(
            InteractionContext ctx,
            [Option("IP", "Select IP")]
            [Choice("Bandai", "Bandai")]
            [Choice("Dragon Ball", "Dragon Ball")]
            [Choice("Digimon", "Digimon")]
            [Choice("Flesh & Blood", "Flesh & Blood")]
            [Choice("Grand Archive", "Grand Archive")]
            [Choice("Gundam", "Gundam")]
            [Choice("holoLive", "holoLive")]
            [Choice("Item Request", "Item Request")]
            [Choice("Lorcana", "Lorcana")]
            [Choice("Magic The Gathering", "Magic The Gathering")]
            [Choice("One Piece", "One Piece")]
            [Choice("Pokémon", "Pokémon")]
            [Choice("Riftbound", "Riftbound")]
            [Choice("Sorcery", "Sorcery")]
            [Choice("Star Wars Unlimited", "Star Wars Unlimited")]
            [Choice("Union Arena", "Union Arena")]
            [Choice("Weiß Schwarz", "Weiß Schwarz")] string ip,

            [Option("distro", "Select Distributor")]
            [Choice("Southern Hobby (Distro #1)", 1)]
            [Choice("Magazine Exchange (Distro #2)", 2)]
            [Choice("PHD Games (Distro #3)", 3)]
            [Choice("Madal (Distro #4)", 4)]
            [Choice("GTS Distribution (Distro #5)", 5)] long distro)
        {
            await ctx.DeferAsync();
            string results;
            IEnumerable<object> resultList = await DistroProductSelector.FetchProductsAsync(DistroProductSelector.GetSheetUri(ip, "English", distro), ip, distro);
            if (!ip.Equals("Item Request"))
            {
                IEnumerable<ProductRecord> productList = (IEnumerable<ProductRecord>) resultList;
                results = DistroProductSelector.GetResultsAsAsciiTable(productList);
            }
            else
            {
                IEnumerable<ItemRequestProductRecord> productList = (IEnumerable<ItemRequestProductRecord>)resultList;
                results = DistroProductSelector.GetResultsAsAsciiTable(productList);
            }

            DiscordMessageBuilder resultMessage = new DiscordMessageBuilder()
                .WithContent($">>> **Distro #{distro} {ip} Product**")
                .AddFile("Results.txt", new MemoryStream(Encoding.UTF8.GetBytes(results)));

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder(resultMessage));
        }

        [SlashCommand("distro", "List current distros supported in the program and their links")]
        [SlashCooldown(1, 30, SlashCooldownBucketType.User)]
        public async Task ListPokemartUSADistrosCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder(
                    new DiscordMessageBuilder()
                        .AddEmbed(PokemartUSABot.DistroEmbed)));
        }

        [SlashCommand("sheets", "List current spreadsheets and the links to them for the program")]
        [SlashCooldown(1, 30, SlashCooldownBucketType.User)]
        public async Task ListPokemartUSASpreadsheetsCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder(
                    new DiscordMessageBuilder()
                        .AddEmbed(PokemartUSABot.SheetsEmbed)));
        }

        [SlashCommand("help", "Information About PokemartUSABot")]
        [SlashCooldown(1, 30, SlashCooldownBucketType.User)]
        public async Task PokemartUSABotHelpCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder(
                    new DiscordMessageBuilder()
                        .AddEmbed(PokemartUSABot.HelpEmbed)));
        }

        //    private static async Task OnErrorOccured(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        //    {
        //        if (e.Exception is SlashExecutionChecksFailedException)
        //        {
        //            TimeSpan rawTime = ((SlashCooldownAttribute)(e.Exception as SlashExecutionChecksFailedException)!.FailedChecks[0]).GetRemainingCooldown(e.Context);
        //            string timeLeft = $"{rawTime.Seconds}s";

        //            await e.Context.EditResponseAsync(
        //                new DiscordWebhookBuilder(
        //                    new DiscordMessageBuilder()
        //                    .AddEmbed(PokemartUSABot.CooldownEmbed.WithDescription($"### :hourglass_flowing_sand: PokemartUSABot Command on Cooldown, Wait {(rawTime.Minutes != 0 ? timeLeft.Insert(0, $"{rawTime.Minutes}m ") : timeLeft)}"))));
        //        }
        //        else
        //        {
        //            // PokemartUSABot.Client.Logger.LogError(e.Exception, "PokemartUSABot Slash Command Error -> \"{}\"", e.Exception.Message);
        //            await e.Context.EditResponseAsync(
        //                new DiscordWebhookBuilder(
        //                    new DiscordMessageBuilder()
        //                        .AddEmbed(PokemartUSABot.CrashEmbed)));
        //        }
        //        throw e.Exception;
        //    }
        //}
    }
}