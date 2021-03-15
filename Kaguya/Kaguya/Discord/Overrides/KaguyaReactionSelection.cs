using Discord;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Selection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Discord.Overrides
{
	/// <summary>
	///  Represents the default implementation of <see cref="BaseReactionSelection{TValue}" /> which comes with a lot of
	///  options suitable for most users.
	///  This class is immutable!
	/// </summary>
	/// <typeparam name="TValue">The type of the values to select from</typeparam>
	public sealed class KaguyaReactionSelection<TValue> : BaseReactionSelection<TValue>
	{
		public KaguyaReactionSelection(IReadOnlyDictionary<IEmote, TValue> selectables, IReadOnlyCollection<SocketUser> users,
			Page selectionPage, Page cancelledPage, Page timeoutedPage, bool allowCancel,
			IEmote cancelEmote, DeletionOptions deletion) : base(users, deletion)
		{
			this.Selectables = selectables;
			this.SelectionPage = selectionPage;
			this.CancelledPage = cancelledPage;
			this.TimeoutedPage = timeoutedPage;
			this.AllowCancel = allowCancel;
			this.CancelEmote = cancelEmote;
		}

		/// <summary>
		///  Gets the items to select from.
		/// </summary>
		public IReadOnlyDictionary<IEmote, TValue> Selectables { get; }
		/// <summary>
		///  Gets the <see cref="Page" /> which is sent into the channel.
		/// </summary>
		public Page SelectionPage { get; }
		/// <summary>
		///  Gets the <see cref="Page" /> which will be shown on cancellation.
		/// </summary>
		public Page CancelledPage { get; }
		/// <summary>
		///  Gets the <see cref="Page" /> which will be shown on a timeout.
		/// </summary>
		public Page TimeoutedPage { get; }
		/// <summary>
		///  Gets whether the selection allows for cancellation.
		/// </summary>
		public bool AllowCancel { get; }
		/// <summary>
		///  Gets the emote used for cancelling.
		///  Only works if <see cref="AllowCancel" /> is set to true.
		/// </summary>
		public IEmote CancelEmote { get; }

		protected override bool ShouldProcess(SocketReaction reaction)
		{
			return (this.AllowCancel && reaction.Emote.Equals(this.CancelEmote)) || this.Selectables.ContainsKey(reaction.Emote);
		}

		protected override Optional<TValue> ParseValue(SocketReaction reaction)
		{
			return this.AllowCancel && reaction.Emote.Equals(this.CancelEmote)
				? Optional<TValue>.Unspecified
				: this.Selectables[reaction.Emote];
		}

		protected override Task<IUserMessage> SendMessageAsync(IMessageChannel channel)
		{
			return channel.SendMessageAsync(this.SelectionPage.Text, embed: this.SelectionPage.Embed);
		}

		// This is this way so that the lib calling this does not modify our message.
		protected override Task ModifyMessageAsync(IUserMessage message) { return Task.CompletedTask; }

		protected override async Task InitializeMessageAsync(IUserMessage message)
		{
			await message.AddReactionsAsync(this.Selectables.Keys.ToArray());
			if (this.AllowCancel)
			{
				await message.AddReactionAsync(this.CancelEmote);
			}
		}

		protected override async Task CloseMessageAsync(IUserMessage message, InteractivityResult<TValue> result)
		{
			await message.RemoveAllReactionsAsync();

			if (result.IsCancelled && this.CancelledPage != null)
			{
				await message.ModifyAsync(x =>
				{
					x.Content = this.CancelledPage.Text;
					x.Embed = this.CancelledPage.Embed;
				});
			}

			if (result.IsTimeouted && this.TimeoutedPage != null)
			{
				await message.ModifyAsync(x =>
				{
					x.Content = this.TimeoutedPage.Text;
					x.Embed = this.TimeoutedPage.Embed;
				});
			}
		}
	}
}