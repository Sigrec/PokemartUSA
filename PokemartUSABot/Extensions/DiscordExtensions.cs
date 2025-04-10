using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace PokemartUSABot.Extensions
{
    internal static class DiscordExtensions
    {
        public static async Task<DiscordMember?> GetMemberByNameAsync(string name)
        {
            bool isReloaded = false;

            Reload:
            DiscordMember? member = PokemartUSABot.GuildMembers!.FirstOrDefault(m =>
                m.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                (m.Nickname != null && m.Nickname.Equals(name, StringComparison.OrdinalIgnoreCase))
            );

            if (member == null)
            {
                if (!isReloaded)
                {
                    PokemartUSABot.Logger.LogInformation("Could not find Discord user {name}, reloading cache", name);
                    PokemartUSABot.GuildMembers = await GetGuildMembersAsync();
                    isReloaded = true;
                    goto Reload;
                }
                else
                {
                    PokemartUSABot.Logger.LogError("Could not find Discord user {name}, after reloading cache", name);
                    return null;
                }
            }

            PokemartUSABot.Logger.LogDebug("Found discord member {name}", name);
            return member;
        }

        public static async Task<IReadOnlyCollection<DiscordMember>> GetGuildMembersAsync()
        {
            return await PokemartUSABot.Guild!.GetAllMembersAsync();
        }

        public static async Task SendDMByDiscordId(ulong userId, string message)
        {
            try
            {
                DiscordMember member = await PokemartUSABot.Guild!.GetMemberAsync(userId);
                if (member != null)
                {
                    // Send the DM to the user
                    DiscordChannel dmChannel = await member.CreateDmChannelAsync();
                    await dmChannel.SendMessageAsync(message);
                    PokemartUSABot.Logger.LogInformation("Sent DM to user {user}", member.DisplayName);
                }
                else
                {
                    PokemartUSABot.Logger.LogWarning("Member not found.");
                }
            }
            catch (Exception ex)
            {
                PokemartUSABot.Logger.LogError("Error sending DM: {errorMsg}", ex.Message);
            }
        }
    }
}
