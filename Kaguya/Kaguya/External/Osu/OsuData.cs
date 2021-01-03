using System.Threading.Tasks;
using OsuSharp;

namespace Kaguya.External.Osu
{
    public class OsuData : OsuBase
    {
        public readonly string OsuUsername;
        public readonly GameMode GameMode;
        public readonly OsuClient Client;

        public OsuData(string osuUsername, string gameModeString, OsuClient osuClient) : base(osuClient)
        {
            Client = osuClient;
            
            OsuUsername = osuUsername;
            GameMode = GetGamemode(gameModeString);
        }

        public OsuData(string osuUsername, GameMode gameMode, OsuClient osuClient) : base(osuClient)
        {
            Client = osuClient;

            OsuUsername = osuUsername;
            GameMode = gameMode;
        }

        public OsuData(long osuId, GameMode gameMode, OsuClient osuClient) : this(osuId.ToString(), gameMode, osuClient) { }
        public OsuData(long osuId, string gameModeString, OsuClient osuClient) : this(osuId.ToString(), gameModeString, osuClient) { }

        /// <summary>
       /// Tries to get the user through user input. Checks first for username match, then id match.
       /// If no matches are found, the returned value will be null.
       /// </summary>
       /// <returns></returns>
        public async Task<User> GetOsuUserAsync()
        {
            User user = await Client.GetUserByUsernameAsync(OsuUsername, GameMode);

            if (user == null)
            {
                if (long.TryParse(OsuUsername, out long id))
                {
                    user = await Client.GetUserByUserIdAsync(id, GameMode);
                }
            }

            return user;
        }
        
        /// <summary>
        /// std = Standard
        /// mania = Mania
        /// taiko = Taiko
        /// ctb = Catch
        /// </summary>
        /// <param name="gmString"></param>
        /// <returns></returns>
        public GameMode GetGamemode(string gmString)
        {
            return gmString.ToLower() switch
            {
                "std" => GameMode.Standard,
                "mania" => GameMode.Mania,
                "taiko" => GameMode.Taiko,
                "ctb" => GameMode.Catch,
                _ => throw new OsuException("Invalid gamemode provided.\n**Valid Game Modes:** `std`, `mania`, `taiko`, `ctb`")
            };
        }
    }
}