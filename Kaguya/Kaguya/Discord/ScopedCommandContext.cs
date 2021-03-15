using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Kaguya.Discord
{
	public class ScopedCommandContext : ShardedCommandContext
	{
		public ScopedCommandContext(IServiceScope scope, DiscordShardedClient client, SocketUserMessage msg) : base(client, msg)
		{
			this.Scope = scope;
		}

		public IServiceScope Scope { get; }
	}
}