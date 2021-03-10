using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using NekosSharp;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Fun
{
	public enum EmotionType
	{
		Hug,
		Kiss,
		Cuddle,
		Feed,
		Pat,
		Poke,
		Slap,
		Tickle
	}

	[Module(CommandModule.Fun)]
	[Group("emotion")]
	[Alias("e")]
	[Summary("Displays an emotion towards a user.")]
	[Remarks("[user]")]
	public class Emotion : KaguyaBase<Emotion>
	{
		private readonly NekoClient _nekoClient;
		public Emotion(ILogger<Emotion> logger, NekoClient nekoClient) : base(logger) { _nekoClient = nekoClient; }

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-hug")]
		public async Task HugCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Hug, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-hug")]
		public async Task HugCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Hug, user?.Mention));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-kiss")]
		public async Task KissCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Kiss, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-kiss")]
		public async Task KissCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Kiss, user?.Mention));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-cuddle")]
		public async Task CuddleCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Cuddle, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-cuddle")]
		public async Task CuddleCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Cuddle, user?.Mention));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-feed")]
		public async Task FeedCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Feed, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-feed")]
		public async Task FeedCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Feed, user?.Mention));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-pat")]
		public async Task PatCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Pat, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-pat")]
		public async Task PatCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Pat, user?.Mention));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-poke")]
		public async Task PokeCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Poke, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-poke")]
		public async Task PokeCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Poke, user?.Mention));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-slap")]
		public async Task SlapCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Slap, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-slap")]
		public async Task SlapCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Slap, user?.Mention));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-tickle")]
		public async Task TickleCommand(string user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Tickle, user));
		}

		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Command("-tickle")]
		public async Task TickleCommand(SocketGuildUser user = null)
		{
			await SendEmbedAsync(await GetEmotionEmbed(EmotionType.Tickle, user?.Mention));
		}

		private async Task<Request> GetRequest(EmotionType emotion)
		{
			var action = _nekoClient.Action_v3;

			return emotion switch
			{
				EmotionType.Hug => await action.HugGif(),
				EmotionType.Kiss => await action.KissGif(),
				EmotionType.Cuddle => await action.CuddleGif(),
				EmotionType.Feed => await action.FeedGif(),
				EmotionType.Pat => await action.PatGif(),
				EmotionType.Poke => await action.PokeGif(),
				EmotionType.Slap => await action.SlapGif(),
				EmotionType.Tickle => await action.TickleGif(),
				_ => await action.SlapGif()
			};
		}

		private string GetEmotionPastTense(EmotionType emotion)
		{
			return emotion switch
			{
				EmotionType.Hug => "hugged",
				EmotionType.Kiss => "kissed",
				EmotionType.Cuddle => "cuddled",
				EmotionType.Feed => "fed",
				EmotionType.Pat => "patted",
				EmotionType.Poke => "poked",
				EmotionType.Slap => "slapped",
				EmotionType.Tickle => "tickled",
				_ => "NOT_IMPLEMENTED"
			};
		}

		private async Task<Embed> GetEmotionEmbed(EmotionType emotion, string user = null)
		{
			if (user == Context.User.Mention)
			{
				user = "themselves";
			}

			string emotionString = user == null
				? $"needs to be {GetEmotionPastTense(emotion)}."
				: $"just {GetEmotionPastTense(emotion)} {user}!";

			return new KaguyaEmbedBuilder(KaguyaColors.Blue)
			       .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
			       .WithDescription($"{Context.User.Mention} {emotionString}")
			       .WithImageUrl((await GetRequest(emotion)).ImageUrl)
			       .Build();
		}
	}
}