﻿using System;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

// ReSharper disable VariableHidesOuterVariable

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent
{
    public static class WarnEvent
    {
        public static event EventHandler<WarnHandlerEventArgs> OnWarn;

        public static void Trigger(Server server, WarnedUser warnedUser)
        {
            WarnEventTrigger(new WarnHandlerEventArgs(server, warnedUser));
        }

        static void WarnEventTrigger(WarnHandlerEventArgs e)
        {
            OnWarn?.Invoke(null, e);
        }
    }

    public class WarnHandlerEventArgs : EventArgs
    {
        public WarnHandlerEventArgs(Server server, WarnedUser warnedUser)
        {
            this.server = server;
            this.warnedUser = warnedUser;
        }
        public Server server { get; private set; }
        public WarnedUser warnedUser { get; private set; }
    }
}