using System.Threading.Tasks;
using OsuSharp;

namespace Kaguya.External.Osu
{
    public class OsuData : OsuBase
    {
        private readonly string _username;
        public readonly GameMode GameMode;
        public readonly OsuClient Client;

        public OsuData(string username, GameMode gameMode, OsuClient osuClient) : base(osuClient)
        {
            // Client must be set before GetOsuUserAsync();
            Client = osuClient;
            _username = username;
            GameMode = gameMode;
        }
        
        /// <summary>
       /// Tries to get the user through user input. Checks first for username match, then id match.
       /// If no matches are found, the returned value will be null.
       /// </summary>
       /// <returns></returns>
       /// <exception cref="OsuUserNotFoundException">Thrown if the user is not found.</exception>
        public async Task<User> GetOsuUserAsync()
        {
            User user = await Client.GetUserByUsernameAsync(_username, GameMode);

            if (user == null)
            {
                if (long.TryParse(_username, out long id))
                {
                    user = await Client.GetUserByUserIdAsync(id, GameMode);
                }
            }

            return user;
        }
    }
}