using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace Kaguya.Core.UserAccounts
{
    public static class UserAccounts
    {
        private static string accountsFile = "Resources/accounts.json";

        static UserAccounts()
        {
            if(DataStorage2.SaveExists(accountsFile))
            {
                Global.UserAccounts = DataStorage2.LoadUserAccounts(accountsFile).ToList();
            }
            else
            {
                Global.UserAccounts = new List<UserAccount>();
                SaveAccounts();
            }
        }

        public static List<UserAccount> GetAllAccounts()
        {
            return Global.UserAccounts;
        }

        public static void SaveAccounts()
        {
            DataStorage2.SaveUserAccounts(Global.UserAccounts, accountsFile);
        }

        public static UserAccount GetAccount(SocketUser user)
        {
            return GetOrCreateAccount(user.Id);
        }

        public static UserAccount GetAccount(ulong id)
        {
            return GetOrCreateAccount(id);
        }

        public static UserAccount GetAuthor()
        {
            return GetOrCreateAccount(146092837723832320);
        }

        private static UserAccount GetOrCreateAccount(ulong id)
        {
            var result = from a in Global.UserAccounts
                         where a.ID == id
                         select a;

            var account = result.FirstOrDefault();
            if(account == null) account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount(id);

            Global.UserAccounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
    }
}
