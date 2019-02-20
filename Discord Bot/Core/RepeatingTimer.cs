using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Net;

namespace Discord_Bot.Core
{
    internal static class RepeatingTimer
    {
        private static Timer loopingTimer;
        private static SocketTextChannel channel;

        internal static Task StartTimer()
        {
            channel = Global.Client.GetGuild(407648193392803851).GetTextChannel(407723395556311043);

            loopingTimer = new Timer() //Placeholder, doesn't do anything.
            {
                Interval = 5000,
                AutoReset = true,
                Enabled = false
            };
            loopingTimer.Elapsed += OnTimerTicked;

            return Task.CompletedTask;
        }

        private static async void OnTimerTicked(object sender, ElapsedEventArgs e)
        {
            await channel.SendMessageAsync("Piss in a can");
        }
    }
}
