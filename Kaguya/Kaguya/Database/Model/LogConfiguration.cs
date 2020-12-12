using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Kaguya.Database.Model
{
    public enum KaguyaLogType
    {
        Ban,
        Unban,
        Warn,
        Unwarn,
        Shadowban,
        Unshadowban,
        Userjoin,
        Userleave,
        Voiceupdate,
        Messagedeleted,
        Messageupdated
    }
    public class LogConfiguration
    {
        public static IList<PropertyInfo> LogProperties { get; private set; }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ServerId { get; init; }
        public ulong Bans { get; set; }
        public ulong UnBans { get; set; }
        public ulong Warns { get; set; }
        public ulong Unwarns { get; set; }
        public ulong Shadowbans { get; set; }
        public ulong Unshadowbans { get; set; }
        public ulong UserJoins { get; set; }
        public ulong UserLeaves { get; set; }
        public ulong VoiceUpdates { get; set; }
        public ulong MessageDeleted { get; set; }
        public ulong MessageUpdated { get; set; }
        
        public static IList<PropertyInfo> GetLogProperties()
        {
            IList<PropertyInfo> properties = typeof(LogConfiguration).GetProperties()
                                                                     .Where(x => !x.Name.Equals("ServerId", StringComparison.OrdinalIgnoreCase) &&
                                                                                 x.PropertyType == typeof(ulong))
                                                                     .ToList();

            return properties;
        }

        /// <summary>
        /// Populates this class's <see cref="LogProperties"/> value.
        /// </summary>
        public static void LoadProperties()
        {
            LogProperties = GetLogProperties();
        }
    }
}