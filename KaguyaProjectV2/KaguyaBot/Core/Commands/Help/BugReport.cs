using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class BugReport : KaguyaBase
    {
        [HelpCommand]
        [Command("BugReport")]
        [Summary("Responds with a link to the bug report template. Requires a GitHub account.")]
        [Remarks("")]
        public async Task Command()
        {
            var formLink = "https://github.com/stageosu/Kaguya/issues/new?assignees=&labels=Bug&template=bug-report.md&title=";
            await SendBasicSuccessEmbedAsync($"[[Click Here]]({formLink}) to file a new bug-report issue " +
                                                             $"on the Kaguya Github repository. This is the fastest possible " +
                                                             $"way the bug will be fixed.\n\n" +
                                                             $"Requires a GitHub account. Don't have one? " +
                                                             $"[[Create a GitHub account]](https://github.com/join)");
        }
    }
}