using System.ComponentModel.DataAnnotations;
using Kaguya.Database.Interfaces;

namespace Kaguya.Database.Model
{
    public class WarnConfiguration : IServerSearchable
    {
        [Key]
        public ulong ServerId { get; set; }
        /// <summary>
        /// The number of warnings needed to mute a user.
        /// </summary>
        public int MuteNum { get; set; }
        /// <summary>
        /// The number of warnings needed to kick a user.
        /// </summary>
        public int KickNum { get; set; }
        /// <summary>
        /// The number of warnings needed to shadowban a user.
        /// </summary>
        public int ShadowbanNum { get; set; }
        /// <summary>
        /// The number of warnings needed to ban a user.
        /// </summary>
        public int BanNum { get; set; }
        
        /*
         * todo: figure out how to allow server admins to configure warnings such that the
         * following is possible:
         *
         * specify x warnings for a temp mute of 30m.
         * specify x warnings for a temp mute of 6h,
         * etc... for mutes, bans, and shadowbans.
         */
    }
}