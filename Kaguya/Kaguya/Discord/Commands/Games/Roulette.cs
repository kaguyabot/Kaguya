using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Games
{
	[Module(CommandModule.Games)]
	[Group("roulette")]
	[Alias("rt")]
	[Summary("Play a game of roulette!\n\n" +
	         "__**How to Play:**__\n" +
	         "A roulette table consists of numbers ranging from 0-36 with colors green, red, and black. " +
	         "Players can bet coins on a number of specific outcomes for potentially massive rewards!\n\n" +
	         "__**Specify an argument below to apply that to your gameplay**__. " +
	         "If an arg has a `*` attached, it means you must specify numbers after it, separated by a space.\n\n" +
	         "- `straight`* - Bet on any 1 number. | 36x\n" +
	         "- `split`* - Bet on any 2 numbers. | 18x\n" +
	         "- `triple`* - Bet on any 3 numbers. | 12x\n" +
	         "- `quad`* - Bet on any 4 numbers. | 9x\n" +
	         "- `line`* - Bet on any 6 numbers. | 6x\n" +
	         "- `column`* - Bet on a column of numbers. Choose column 1, 2, or 3. 12 numbers per column. | 3x\n" +
	         "- `dozen`* - Bet on any 12 numbers. | 3x\n" +
	         "- `low` - Bet on the outcome being 1-18. | 2x\n" +
	         "- `high` - Bet on the outcome being 19-36. | 2x\n" +
	         "- `odd` - Bet on the outcome being odd. | 2x\n" +
	         "- `even` - Bet on the outcome being even. | 2x\n" +
	         "- `red` - Bet on all red numbers. | 2x\n" +
	         "- `black` - Bet on all black numbers. | 2x")]
	[Remarks("<coins> <arg> [* nums] {...}")]
	public class Roulette : KaguyaBase<Roulette>
	{
		public Roulette(ILogger<Roulette> logger) : base(logger) {}

		[Command]
		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		public async Task RouletteCommandAsync(int coins, string gameArg, params int[] args) {}
	}

	/// <summary>
	///  Class that is responsible for parsing all roulette arguments and determining whether the
	///  provided user input can be used for a valid game.
	/// </summary>
	public class RouletteArgParser
	{
		private readonly Dictionary<RouletteGameType, bool> _additionalArgsRequired;
		private readonly int[] _args;
		private readonly string _gameArg;

		public RouletteArgParser(string gameArg, params int[] args)
		{
			_gameArg = gameArg.ToLower();
			_args = args;
			_additionalArgsRequired = new Dictionary<RouletteGameType, bool>
			{
				{RouletteGameType.StraightUp, true},
				{RouletteGameType.Split, true},
				{RouletteGameType.Triple, true},
				{RouletteGameType.Quad, true},
				{RouletteGameType.Line, true},
				{RouletteGameType.Column, true},
				{RouletteGameType.Dozen, true}
			};
		}

		private RouletteGameType GetGameType()
		{
			return _gameArg switch
			{
				"straight" => RouletteGameType.StraightUp,
				"split" => RouletteGameType.Split,
				"triple" => RouletteGameType.Triple,
				"quad" => RouletteGameType.Quad,
				"line" => RouletteGameType.Line,
				"column" => RouletteGameType.Column,
				"dozen" => RouletteGameType.Dozen,
				"low" => RouletteGameType.Low,
				"high" => RouletteGameType.High,
				"odd" => RouletteGameType.Odd,
				"even" => RouletteGameType.Even,
				"red" => RouletteGameType.Red,
				"black" => RouletteGameType.Black,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public bool IsValid()
		{
			RouletteGameType type;
			try
			{
				type = GetGameType();
			}
			catch (ArgumentOutOfRangeException e)
			{
				return false;
			}

			// If args are required and args are provided, return true.
			// If args are not required and args are not provided, return true.
			// Otherwise return false.
			if (_additionalArgsRequired.ContainsKey(type))
			{
				return _args != null;
			}
			else
			{
				return _args == null;
			}
		}
	}
}