using System;
using System.Data;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("math")]
    [Summary("Performs a simple mathematical calculation. You can add, subtract, " +
             "multiply, divide, and use the modulus operator.")]
    
    public class Math : KaguyaBase<Math>
    {
        private readonly ILogger<Math> _logger;

        public Math(ILogger<Math> logger) : base(logger) { _logger = logger; }

        [Command]
        [InheritMetadata(CommandMetadata.Summary)]
        [Example("1 + 1")]
        [Example("10 * 9 / 5 + 6")]
        [Example("((10 * 9) / 5) + 6")]
        public async Task MathCommandAsync([Remainder]string input)
        {
            try
            {
                double result = Convert.ToDouble(new DataTable().Compute(input, null));
                if (result % 1 == 0)
                {
                    await SendBasicSuccessEmbedAsync($"Result: {result.ToString().AsBold()}");
                }
                else
                {
                    await SendBasicSuccessEmbedAsync($"Result: {result.ToString("N2").AsBold()}");
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "Math parse failure");
                await SendBasicErrorEmbedAsync("Failed to parse your math operation. Please try again.");
            }
        }
    }
}