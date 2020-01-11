using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

// Todo: Test

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent
{
    public static class FishHandler // Fish level-up handler.
    {
        public static async void OnFish(object fish, FishHandlerEventArgs args)
        {
            var context = args.Context;
            var server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);
            var curFishExp = args.User.FishExp;
            var newFishExp = curFishExp + args.Fish.Exp;

            if (HasLeveledUp(curFishExp, newFishExp))
            {
                var channel = (SocketTextChannel)context.Channel;
                var level = (int)GetFishLevel(newFishExp);

                if(server.LogFishLevels != 0)
                {
                    try
                    {
                        channel = ConfigProperties.Client.GetGuild(context.Guild.Id).GetTextChannel(server.LogFishLevels);
                    }
                    catch (Exception)
                    {
                        channel = (SocketTextChannel)context.Channel;
                    }
                }

                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Kaguya Fishing: Level Up!",
                    Description = $"{context.User.Mention}'s fishing level is now `level {level}`! " +
                                  $"You now have access to the following perks:\n\n" +
                                  $"`{GetRewardString(level)}`"
                };
            }
        }

        public static bool HasLeveledUp(int oldExp, int newExp)
        {
            return Math.Floor(GetFishLevel(oldExp)) < Math.Floor(GetFishLevel(newExp));
        }

        public static double GetFishLevel(int exp)
        {
            return Math.Sqrt((exp / 8) + -8);
        }

        private static string GetRewardString(int fishLevel)
        {
            const string rare = "increased chance to catch a rare fish";
            const string tax = "decreased tax penalty when selling a fish";
            const string bait = "increased bait cost";

            // todo: Apply reward string to levelup embed.

            return fishLevel switch
            {
                10 => $"`3%` {rare}\n" +
                      $"`5%` {tax}\n" +
                      $"`25%` {bait}",
                20 => $"`7%` {rare}\n" +
                      $"`10%` {tax}\n" +
                      $"`60%` {bait}",
                30 => $"`12%` {rare}\n" +
                      $"`15%` {tax}\n" +
                      $"`150%` {bait}\n",
                40 => $"`17%` {rare}\n" +
                      $"`20%` {tax}\n" +
                      $"`250%` {bait}\n",
                50 => $"`21%` {rare}\n" +
                      $"`20%` {tax}\n" +
                      $"`400%` {bait}\n",
                _ => ""
            };
        }
    }
}
