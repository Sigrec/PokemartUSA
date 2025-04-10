using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemartUSABot.Extensions;

namespace PokemartUSABot.Controllers
{
    [Route("webhook/orderaction")]
    [ApiController]
    public class OrderActionController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GoogleSheetsPayload payload)
        {
            PokemartUSABot.Logger.LogDebug("Received google sheets webhook: {payload}", payload.ToString());

            // You can forward the data to Discord or handle it however you like
            DiscordMember? member = await DiscordExtensions.GetMemberByNameAsync(payload.Data["Name"]);
            if (member != null)
            {
                DiscordDmChannel dmChannel = await member.CreateDmChannelAsync();

                string doubleBoxed = payload.Data["Double Boxed?"];
                string notes = payload.Data["Notes"];
                DiscordEmbedBuilder OrderCancelledEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Order {payload.Data["Row Number"]} Cancelled",
                    Description = $@"
                        **Order Date:** {payload.Data["Order Date"]}
                        **Distro:** {payload.Data["Distro Number"]}
                        **Distro Availability:** {payload.Data["Distro Availability"]}
                        **Product:** {payload.Data["Product Requested"]}
                        **Price Each:** ${payload.Data["Price Each"]}
                        **Qty Requested:** {payload.Data["Qty Req"]}
                        **Ship Method:** {payload.Data["Ship Method"]}
                        **Double Boxed:** {(string.IsNullOrWhiteSpace(doubleBoxed) ? "Yes": "No")}
                        **Total Cost:** ${payload.Data["Total Cost"]}
                        {(string.IsNullOrWhiteSpace(notes) ? string.Empty : $"**Notes:** {payload.Data["Notes"]}")}",
                    Color = PokemartUSABot.COLOR,
                    Timestamp = DateTimeOffset.UtcNow
                }.WithFooter(PokemartUSABot.NAME, PokemartUSABot.Client.CurrentUser.AvatarUrl);
                DiscordMessageBuilder resultMessage = new DiscordMessageBuilder().AddEmbed(OrderCancelledEmbed);

                await dmChannel.SendMessageAsync(resultMessage);
                return Ok();
            }

            return BadRequest("Member not found in the guild.");
        }
    }

    public class GoogleSheetsPayload
    {
        public required string Reason { get; set; }
        public required Dictionary<string, string> Data { get; set; }
    }
}
