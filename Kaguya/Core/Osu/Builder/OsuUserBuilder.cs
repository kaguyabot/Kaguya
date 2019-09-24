using Kaguya.Core.Osu.Models;
using System;
using System.Linq;
using System.Text;

namespace Kaguya.Core.Osu.Builder
{
    public class OsuUserBuilder : OsuBaseBuilder<OsuUserModel>
    {
        public string UserId; // u
        public int Mode; // m

        public OsuUserBuilder(string user, int mode = 0)
        {
            UserId = user;
            Mode = mode;

            Execute();
        }

        public OsuUserModel Execute()
        {
            var userArray = ExecuteJson(OsuRequest.User);
            var user = ProcessJson(userArray);
            return user;
        }

        public OsuUserModel ProcessJson(OsuUserModel[] userArray)
        {
            foreach (var item in userArray)
            {
                item.difference = DateTime.Now - item.join_date;
            }

            var userList = userArray.ToList();

            return userList.FirstOrDefault();
        }

        public override string Build(StringBuilder urlBuilder)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                urlBuilder.Append("&u=").Append(UserId);
            }

            if (!string.IsNullOrEmpty(Mode.ToString()))
            {
                urlBuilder.Append("&m=").Append(Mode);
            }
            return urlBuilder.ToString();
        }
    }
}
