﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PokemartUSABot.Config;
using PokemartUSABot.Extensions;
using PokemartUSABot.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokemartUSABot
{
    [JsonSerializable(typeof(GoogleSheetsPayload))]
    [JsonSourceGenerationOptions(UseStringEnumConverter = true)]
    internal partial class BotJsonSerializerContext : JsonSerializerContext
    {
    }

    internal class PokemartUSABot
    {
        internal static DiscordEmbedBuilder? CrashEmbed;
        internal static DiscordEmbedBuilder? CooldownEmbed;
        internal static DiscordEmbedBuilder? HelpEmbed;
        internal static DiscordEmbedBuilder? WarningEmbed;
        internal static DiscordEmbedBuilder? DistroEmbed;
        internal static DiscordEmbedBuilder? SheetsEmbed;

        public static DiscordClient Client { get; set; }
        private static PokemartUSABotConfig? PokemartUSABotConfig;
        internal const string NAME = "PokemartUSA";
        internal static readonly DiscordColor COLOR = new DiscordColor("#FEC634");
        internal static ILogger<BaseDiscordClient> Logger { get; private set; }
        internal static DiscordGuild? Guild { get; private set; }
        internal static IReadOnlyCollection<DiscordMember>? GuildMembers { get; set; }

        static async Task Main(string[] args)
        {
            // Configure and start the bot
            await using FileStream stream = File.OpenRead(@"Config/config.json");
            PokemartUSABotConfig = await JsonSerializer.DeserializeAsync(stream, PokemartUSABotConfigContext.Default.PokemartUSABotConfig);

            await Task.WhenAll(StartPokemartUSABotClient(), SetupApp(args));
        }

        private static async Task StartPokemartUSABotClient()
        {
            // Setup PokemartUSABot bot
            DiscordConfiguration clientConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.DirectMessages | DiscordIntents.MessageContents | DiscordIntents.Guilds | DiscordIntents.DirectMessageReactions | DiscordIntents.GuildEmojis,
                Token = PokemartUSABotConfig!.DecodeToken(),
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                AutoReconnect = true
            };
            Client = new DiscordClient(clientConfig);
            Logger = Client!.Logger;

            Client.ComponentInteractionCreated += async (s, e) =>
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            };

            // Set timeout for user input
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            Client.Ready += async (sender, e) =>
            {
#if DEBUG
                Guild = await GetGuildById(1211706746586796104);
#else
                Guild = await GetGuildById();
#endif
                GuildMembers = await DiscordExtensions.GetGuildMembersAsync();
            };

            SlashCommandsExtension Command = Client.UseSlashCommands();
            Command.RegisterCommands<PokemartUSABotCommands>();
            Command.SlashCommandErrored += OnErrorOccured;


            await Client.ConnectAsync();
            CreateEmbeds();
            await Task.Delay(-1);
        }

        public static async Task SetupApp(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Configure services
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.TypeInfoResolver = BotJsonSerializerContext.Default;
                options.JsonSerializerOptions.WriteIndented = true;
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            WebApplication app = builder.Build();

            // Configure middleware
            app.UseRouting();
            app.MapControllers();

            await app.RunAsync();
        }

        private static async Task<DiscordGuild?> GetGuildById(ulong guildId)
        {
            // Iterate over all the guilds the bot is part of
            DiscordGuild? guild = await Client.GetGuildAsync(guildId);

            if (guild != null)
            {
                Logger.LogInformation("The bot is connected to the server: {guildName} (Guild ID: {guildId})", guild.Name, guild.Id);
                return guild;
            }

            Logger.LogError("The bot is not connected to server with id '{id}'.", guildId);
            return guild;
        }

        public static DiscordEmbedBuilder CreateOrderDm(string title, GoogleSheetsPayload payload, string? reason = null)
        {
            string doubleBoxed = payload.Data["Double Boxed?"];
            string notes = payload.Data["Notes"];
            DiscordEmbedBuilder orderEmbed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = $"{(reason != null ? reason : string.Empty)}",
                Color = PokemartUSABot.COLOR,
                Timestamp = DateTimeOffset.UtcNow
            }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl)
            .AddField("Details", $@"
                        **Order Date:** {payload.Data["Order Date"]}
                        **Distro:** {payload.Data["Distro Number"]}
                        **Distro Availability:** {payload.Data["Distro Availability"]}
                        **Product:** {payload.Data["Product Requested"]}
                        **Price Each:** ${payload.Data["Price Each"]}
                        **Qty Requested:** {payload.Data["Qty Req"]}
                        **Ship Method:** {payload.Data["Ship Method"]}
                        **Double Boxed:** {(string.IsNullOrWhiteSpace(doubleBoxed) ? "Yes" : "No")}
                        **Total Cost:** ${payload.Data["Total Cost"]}
                        {(string.IsNullOrWhiteSpace(notes) ? string.Empty : $"**Notes:** {payload.Data["Notes"]}")}");

            return orderEmbed;
        }

        private static void CreateEmbeds()
        {
            CrashEmbed = new DiscordEmbedBuilder
            {
                Title = $":bangbang: PokemartUSABot Crashed! Try Again",
                Color = PokemartUSABot.COLOR,
                Timestamp = DateTimeOffset.UtcNow
            }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl);

            CooldownEmbed = new DiscordEmbedBuilder
            {
                Color = PokemartUSABot.COLOR,
                Timestamp = DateTimeOffset.UtcNow
            }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl);

            HelpEmbed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = PokemartUSABot.NAME,
                    Url = @"https://github.com/Sigrec/PokemartUSABot",
                    IconUrl = PokemartUSABot.Client.CurrentUser.AvatarUrl
                },
                Color = PokemartUSABot.COLOR,
                Timestamp = DateTimeOffset.UtcNow,
                Description = ""
            }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl)
             .WithThumbnail(PokemartUSABot.Client.CurrentUser.AvatarUrl)
             .AddField("FAQ", "[Google Doc](<https://docs.google.com/document/d/1K3hmfo1EzLazjQz2-_zFdsqjz-NQnz7POPyAORxO_Wo/edit?tab=t.0#heading=h.45c7ytkyi1ft>)")
             .AddField("Reselling Guides",
             "[Reselling Guide Part 1](<https://docs.google.com/document/d/1dn2_5Th4TG-WAh9EL_D_BYBtd9-vIcox2Ih6bNUuFdw/edit>)\n" +
             "[Reselling Guide Part 2](<https://docs.google.com/document/d/1Oxw2iCtMSjOLkMG2wEMdH-Vu-OvrcS3rdhWnwa8x2mg/edit>)")
             .AddField("Commands",
             $"**/{NAME.ToLower()} product** - Outputs the list of products for a given IP at a specific distro\n" +
             $"**/{NAME.ToLower()} distro** - Outputs the list of distros currently supported by the wholesale program\n" +
             $"**/{NAME.ToLower()} sheets** - Outputs the list of google spreadsheets used in the wholesale program");

            WarningEmbed = new DiscordEmbedBuilder
            {
                Title = ":warning: Warning",
                Color = PokemartUSABot.COLOR,
                Timestamp = DateTimeOffset.UtcNow
            }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl);

            DistroEmbed = new DiscordEmbedBuilder
            {
                Title = "Distro",
                Description = 
                "**#1 - [Southern Hobby](<https://www.southernhobby.com/>)**\n" +
                "**#2 - [Magazine Exchange](<https://magazine-exchange.com/>)**\n" +
                "**#3 - [PHD Games](<https://portal.phdgames.com/products?p=preordersdue&page=1&size=20>)**\n" +
                "**#4 - [Madal](<https://madal.com/>)**\n" +
                "**#5 - [GTS Distribution](<https://www.gtsdistribution.com/>)**",
                Color = PokemartUSABot.COLOR,
                Timestamp = DateTimeOffset.UtcNow
            }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl);

            SheetsEmbed = new DiscordEmbedBuilder
            {
                Title = "Spreadsheets",
                Description =
                "**[Master Tracking Sheet](<https://docs.google.com/spreadsheets/d/1fWKRk_1i69rFE2ytxEmiAlHqYrPXVmhXSbG3fgGsl_I/edit?gid=1962881622#gid=1962881622>)**\n" +
                "**[Pokémon Allocation Sheet](<https://docs.google.com/spreadsheets/d/1AnnzLYz1ktCLm0-Mt5o-6p4T8AqE0r2gewv-osqrK0A/edit?gid=0#gid=0>)**\n" +
                "**[Alt Product Allocation Sheet](<https://docs.google.com/spreadsheets/d/1Qj9aV8ae0MJ7MlBLIqYVh55B8_Ydprryy9zDwNoRJyU/edit?gid=419743007#gid=419743007>)**",
                Color = PokemartUSABot.COLOR,
                Timestamp = DateTimeOffset.UtcNow
            }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl);
        }

        private static async Task OnErrorOccured(SlashCommandsExtension s, SlashCommandErrorEventArgs e)
        {
            if (e.Exception is SlashExecutionChecksFailedException)
            {
                TimeSpan rawTime = ((SlashCooldownAttribute)(e.Exception as SlashExecutionChecksFailedException)!.FailedChecks[0]).GetRemainingCooldown(e.Context);
                string timeLeft = $"{rawTime.Seconds}s";

                await e.Context.EditResponseAsync(
                    new DiscordWebhookBuilder(
                        new DiscordMessageBuilder()
                        .AddEmbed(CooldownEmbed!.WithDescription($"### :hourglass_flowing_sand: PokemartUSABot Command on Cooldown, Wait {(rawTime.Minutes != 0 ? timeLeft.Insert(0, $"{rawTime.Minutes}m ") : timeLeft)}"))));
            }
            else if (e.Exception is DistroConfigurationException)
            {
                Logger.LogError(e.Exception, "PokemartUSABot Slash Command Error -> \"{}\"", e.Exception.Message);
                await e.Context.EditResponseAsync(
                    new DiscordWebhookBuilder(
                        new DiscordMessageBuilder()
                            .AddEmbed(WarningEmbed!.WithDescription($"**{e.Exception.Message}**"))));
            }
            else
            {
                Logger.LogError(e.Exception, "PokemartUSABot Slash Command Error -> \"{}\"", e.Exception.Message);
                await e.Context.EditResponseAsync(
                    new DiscordWebhookBuilder(
                        new DiscordMessageBuilder()
                            .AddEmbed(CrashEmbed!)));
            }
        }
    }
}
