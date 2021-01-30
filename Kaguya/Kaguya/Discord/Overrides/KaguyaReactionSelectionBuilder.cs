using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Extensions;
using Interactivity.Selection;
using Kaguya.Discord.Overrides.Extensions;

namespace Kaguya.Discord.Overrides
{
    public sealed class KaguyaReactionSelectionBuilder<TValue> : BaseReactionSelectionBuilder<TValue>
    {
        /// <summary>
        /// Gets or sets the items to select from.
        /// </summary>
        public Dictionary<IEmote, TValue> Selectables { get; set; } = new Dictionary<IEmote, TValue>();

        /// <summary>
        /// Gets or sets the function to convert the values into display text.
        /// </summary>
        public Func<TValue, string> StringConverter { get; set; } = x => x.ToString();

        /// <summary>
        /// Gets or sets the title of the selection.
        /// </summary>
        public string Title { get; set; } = "Select one of these:";

        /// <summary>
        /// Gets whether the selection allows for cancellation.
        /// </summary>
        public bool AllowCancel { get; set; } = true;

        /// <summary>
        /// Gets the emote used for cancelling.
        /// Only works if <see cref="AllowCancel"/> is set to <see cref="true"/>.
        /// </summary>
        public IEmote CancelEmote { get; set; } = new Emoji("❌");

        /// <summary>
        /// Gets the <see cref="Page"/> which is sent into the channel.
        /// </summary>
        public PageBuilder SelectionPage { get; set; } = new PageBuilder().WithColor(Color.Blue);

        /// <summary>
        /// Gets the <see cref="Page"/> which will be shown on cancellation.
        /// </summary>
        public PageBuilder CancelledPage { get; set; } = null;

        /// <summary>
        /// Gets the <see cref="Page"/> which will be shown on a timeout.
        /// </summary>
        public PageBuilder TimeoutedPage { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the selectionembed will be added by a default value visualizer.
        /// </summary>
        public bool EnableDefaultSelectionDescription { get; set; } = true;

        public override BaseReactionSelection<TValue> Build()
        {
            if (Selectables == null)
            {
                throw new ArgumentException(nameof(Selectables));
            }
            if (Selectables.Count == 0)
            {
                throw new InvalidOperationException("You need at least one selectable");
            }
            if (AllowCancel && Selectables.Keys.Contains(CancelEmote))
            {
                throw new InvalidOperationException("Found duplicate emotes! (Cancel Emote)");
            }
            if (Selectables.Distinct().Count() != Selectables.Count)
            {
                throw new InvalidOperationException("Found duplicate emotes!");
            }
            if (AllowCancel && CancelEmote == null)
            {
                throw new ArgumentNullException(nameof(CancelEmote));
            }

            if (EnableDefaultSelectionDescription)
            {
                var builder = new StringBuilder();

                foreach (var selectable in Selectables)
                {
                    string option = StringConverter.Invoke(selectable.Value);
                    builder.AppendLine($"{selectable.Key} - {option}");
                }

                SelectionPage.AddField(Title, builder.ToString());
            }

            return new KaguyaReactionSelection<TValue>(
                Selectables.AsReadOnlyDictionary(),
                Users?.AsReadOnlyCollection() ?? throw new ArgumentException(nameof(Users)),
                SelectionPage?.Build() ?? throw new ArgumentNullException(nameof(SelectionPage)),
                CancelledPage?.Build(),
                TimeoutedPage?.Build(),
                AllowCancel,
                CancelEmote,
                Deletion
                );
        }

        /// <summary>
        /// Sets the values to select from.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithSelectables<TKey>(IDictionary<TKey, TValue> selectables) where TKey : IEmote
        {
            Selectables = selectables.ToDictionary(x => x.Key as IEmote, x => x.Value);
            return this;
        }

        /// <summary>
        /// Sets the users who can interact with the selection.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithUsers(IEnumerable<SocketUser> users)
        {
            Users = users.ToList();
            return this;
        }

        /// <summary>
        /// Sets the users who can interact with the selection.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithUsers(params SocketUser[] users)
        {
            Users = users.ToList();
            return this;
        }

        /// <summary>
        /// Sets what the selection should delete.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithDeletion(DeletionOptions deletion)
        {
            Deletion = deletion;
            return this;
        }

        /// <summary>
        /// Sets the selection embed of the selection.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithSelectionEmbed(PageBuilder selectionPage)
        {
            SelectionPage = selectionPage;
            return this;
        }

        /// <summary>
        /// Sets the embed which the selection embed gets modified to after the selection has been cancelled.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithCancelledEmbed(PageBuilder cancelledPage)
        {
            CancelledPage = cancelledPage;
            return this;
        }

        /// <summary>
        /// Sets the embed which the selection embed gets modified to after the selection has timed out.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithTimeoutedEmbed(PageBuilder timeoutedPage)
        {
            TimeoutedPage = timeoutedPage;
            return this;
        }

        /// <summary>
        /// Sets the function to convert the values into possibilites.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithStringConverter(Func<TValue, string> stringConverter)
        {
            StringConverter = stringConverter;
            return this;
        }

        /// <summary>
        /// Sets the title of the selection.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Gets whether the selection allows for cancellation.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithAllowCancel(bool allowCancel)
        {
            AllowCancel = allowCancel;
            return this;
        }

        /// <summary>
        /// Gets the emote used for cancelling.
        /// Only works if <see cref="AllowCancel"/> is set to <see cref="true"/>.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithCancelEmote(IEmote cancelEmote)
        {
            CancelEmote = cancelEmote;
            return this;
        }

        /// <summary>
        /// Sets whether the selectionembed will be added by a default value visualizer.
        /// </summary>
        public KaguyaReactionSelectionBuilder<TValue> WithEnableDefaultSelectionDescription(bool enableDefaultSelectionDescription)
        {
            EnableDefaultSelectionDescription = enableDefaultSelectionDescription;
            return this;
        }
    }
}