using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemartUSABot.Extensions;
using PokemartUSABot.Models;

namespace PokemartUSABot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UncancelOrderController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GoogleSheetsPayload payload)
        {
            PokemartUSABot.Logger.LogDebug("Received google sheets uncancel order webhook: {payload}", payload.ToString());

            // You can forward the data to Discord or handle it however you like
            DiscordMember? member = await DiscordExtensions.GetMemberByNameAsync(payload.Data["Name"].Trim());
            if (member != null)
            {
                DiscordDmChannel dmChannel = await member.CreateDmChannelAsync();
                DiscordMessageBuilder resultMessage = new DiscordMessageBuilder().AddEmbed(PokemartUSABot.CreateOrderDm($"Order '{payload.Data["Row Number"]}' Uncanceled", payload));

                await dmChannel.SendMessageAsync(resultMessage);
                return Ok();
            }

            return BadRequest("Member not found in the guild.");
        }
    }
}
