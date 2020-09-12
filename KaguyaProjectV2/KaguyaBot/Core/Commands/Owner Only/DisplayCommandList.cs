using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class DisplayCommandList : KaguyaBase
    {
        [OwnerCommand]
        [Command("DisplayCommandList")]
        [Alias("dcl")]
        [Summary("Displays a list of all Kaguya commands with their respective categories, as " +
                 "well as the formatting for the list (so it may be pasted elsewhere).")]
        [Remarks("[exclude owner commands? (true/false)]")]
        public async Task Command(bool excludeOwnerCmds = true)
        {
            var cmdInfo = CommandHandler._commands;
            var attributes = new Attribute[]
            {
                new AdminCommandAttribute(), new CurrencyCommandAttribute(),
                new ExpCommandAttribute(), new FunCommandAttribute(),
                new HelpCommandAttribute(), new MusicCommandAttribute(),
                new NsfwCommandAttribute(), new OsuCommandAttribute(),
                new UtilityCommandAttribute(), new PremiumServerCommandAttribute(),
                new PremiumUserCommandAttribute(), new OwnerCommandAttribute()
            };

            if (excludeOwnerCmds)
                attributes[^1] = null;
            var respSb = new StringBuilder();
            
            foreach (var cmd in cmdInfo.Commands.OrderBy(x => x.Name))
            {
                string aliases = cmd.Aliases.Where(alias => alias.ToLower() != cmd.Name.ToLower())
                    .Aggregate("", (current, alias) => current + $"[{alias}]");
                foreach (var attr in attributes)
                {
                    // We skip this attribute because it no longer has a page of its own.
                    if (attr.GetType() == typeof(PremiumUserCommandAttribute) || attr.GetType() == typeof(PremiumServerCommandAttribute))
                        continue;

                    respSb.AppendLine(attr.ToString().Split("Command")[0]);
                    string warn = "";
                    string premium = "";
                    
                    if (cmd.Preconditions.Contains(attributes[9]) || cmd.Preconditions.Contains(attributes[10]))
                        premium = "{$}";
                    
                    if (cmd.Attributes.Contains(attr) || cmd.Preconditions.Contains(attr))
                    {
                        if(cmd.Attributes.Contains(new DangerousCommandAttribute()))
                            warn = "(Dangerous)";
                            
                        if (!respSb.ToString().Contains($"${cmd.Name.ToLower()} {aliases}{warn}"))
                        {
                            if (!string.IsNullOrWhiteSpace(aliases))
                            {
                                aliases = aliases.Insert(0, " ");
                            }
                            
                            respSb.AppendLine($"`${cmd.Name.ToLower()}{aliases} {premium}`");
                        }
                    }
                }
            }

            var dt = DateTime.Now;
            int cmdCount = respSb.ToString().Split("\n").Count(x => x.Contains('$'));
            respSb.Insert(0, $"Date: {dt.Date.Humanize(false)} | Command Count: {cmdCount}\n");
            // Messages sent via Discord can't be greater than 2,000 characters.
            if (respSb.Length >= 1950)
            {
                using (var stream = new MemoryStream())
                {
                    var writer = new StreamWriter(stream);
                    await writer.WriteAsync(respSb.ToString());
                    
                    await writer.FlushAsync();
                    stream.Seek(0, SeekOrigin.Begin);
                    
                    await Context.Channel.SendFileAsync(stream, $"Commands_List_Count{cmdCount}{dt.Year}-{dt.Month}-{dt.Day}.txt");
                }

                return;
            }
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} {respSb}");
        }
    }
}