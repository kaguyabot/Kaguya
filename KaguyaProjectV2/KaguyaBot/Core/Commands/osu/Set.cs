using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Builders;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class OsuSet : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [OsuCommand]
        [Command("osuSet")]
        [Summary("Allows a user to store their osu! username or ID in their Kaguya account. " +
                 "This way, users may use the other osu! commands without having to specify a " +
                 "player, assuming they want information about themselves.")]
        [Remarks("<username or ID>\nSomeUser\nSome user with spaces")]
        public async Task OsuSetCommand([Remainder]string username)
        {
            var playerObject = new OsuUserBuilder(username).Execute();
            if (playerObject == null)
            {
                embed.WithDescription($"{Context.User.Mention} **ERROR: This username does not match a valid osu! username!**");
                embed.SetColor(EmbedColor.RED);
                await ReplyAsync(embed: embed.Build());
                return;
            }

            //Getting user profile database object and updating it.
            var userAccount = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            userAccount.OsuId = playerObject.UserId;
            await DatabaseQueries.UpdateAsync(userAccount);

            embed.WithTitle("osu! Username Set");
            embed.WithDescription($"{Context.User.Mention} **Your new username has been set! Changed to `{playerObject.Username}`.**");

            await ReplyAsync(embed: embed.Build());
        }
    }
}
