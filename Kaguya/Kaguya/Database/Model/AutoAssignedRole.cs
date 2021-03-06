using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Database.Interfaces;

namespace Kaguya.Database.Model
{
    public class AutoAssignedRole : IServerSearchable
    {
        [Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ServerId { get; set; }
        [Key, Column(Order = 1)]
        public ulong RoleId { get; set; }
        /// <summary>
        /// How long to wait before assigning the role automatically. A null value = assign immediately.
        /// </summary>
        public TimeSpan? Delay { get; set; }
    }
}