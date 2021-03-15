using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.OwnerOnly
{
	[Restriction(ModuleRestriction.OwnerOnly)]
	[Module(CommandModule.OwnerOnly)]
	[Group("sendsystemmessage")]
	[Alias("sysmsg")]
	public class SendSystemMessage : KaguyaBase<SendSystemMessage>
	{
		private readonly DiscordShardedClient _client;
		public SendSystemMessage(ILogger<SendSystemMessage> logger, DiscordShardedClient client) : base(logger) { _client = client; }

		[Command]
		[Summary("Sends a message to the user from the bot account.")]
		[Remarks("<user id> <message>")]
		public async Task SystemMessageCommand(ulong userId, [Remainder]
			string message)
		{
			var socketUser = _client.GetUser(userId);

			if (socketUser == null)
			{
				await SendBasicErrorEmbedAsync("User could not be retreived through the client.");

				return;
			}

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Orange).WithTitle("System Message")
			                                                       .WithDescription("The owner of the bot has sent you a message.\n\n" +
			                                                                        "Contents:".AsBoldUnderlined() +
			                                                                        $"\n{message}")
			                                                       .WithFooter(new EmbedFooterBuilder
			                                                       {
				                                                       Text = "Please note, replies in this chat channel will not be seen."
			                                                       });

			try
			{
				var dmChannel = await socketUser.GetOrCreateDMChannelAsync();
				await dmChannel.SendMessageAsync(embed: embed.Build());

				await SendBasicSuccessEmbedAsync($"Sent the message to user {socketUser}.");
			}
			catch (Exception e)
			{
				await SendBasicErrorEmbedAsync($"Failed to send user {socketUser} the message. Error:\n\n" + $"{e.Message.AsBold()}");
			}
		}
	}
}