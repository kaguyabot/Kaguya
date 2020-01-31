﻿using System.Linq;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

// ReSharper disable AccessToDisposedClosure

namespace KaguyaProjectV2.KaguyaBot.Core.Images.UserProfile.Models
{
    public class ProfileTemplateUserData
    {
        private SocketGuildUser User { get; }
        public ProfileTemplateUserData(User user, Server server)
        {
            User = ConfigProperties.Client.GetGuild(server.ServerId).GetUser(user.UserId);
            ServerXp = server.ServerExp.First(x => x.UserId == user.UserId).Exp;
            GlobalXp = user.Experience;
            Username = User.Username;
            Discriminator = User.Discriminator;
            ProfileUrl = User.GetAvatarUrl();
            ServerXpRank = user.GetServerXpRank(server).Item1;
            GlobalXpRank = user.GetGlobalXpRank().Result.Item1;
            TotalServerXpUsers = user.GetServerXpRank(server).Item2;
            TotalGlobalXpUsers = user.GetGlobalXpRank().Result.Item2;
        }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public int ServerXp { get; set; }
        public int GlobalXp { get; set; }
        public string ProfileUrl { get; set; }
        public int ServerXpRank { get; set; }
        public int GlobalXpRank { get; set; }
        public int TotalServerXpUsers { get; set; }
        public int TotalGlobalXpUsers { get; set; }
    }
}
