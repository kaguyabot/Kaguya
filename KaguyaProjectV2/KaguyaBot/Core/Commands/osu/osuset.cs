using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Osu.Builders;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    [Group("osuset")]
    public class OsuSet : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [Command()]
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
            var userAccount = await UserQueries.GetOrCreateUser(Context.User.Id);
            userAccount.OsuId = playerObject.user_id;
            await UserQueries.UpdateUser(userAccount);

            embed.WithTitle("osu! Username Set");
            embed.WithDescription($"{Context.User.Mention} **Your new username has been set! Changed to `{playerObject.username}`.**");

            await ReplyAsync(embed: embed.Build());
        }
    }
}
