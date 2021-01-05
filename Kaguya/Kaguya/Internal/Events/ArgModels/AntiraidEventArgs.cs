using System;
using System.Collections.Generic;
using Kaguya.Internal.Enums;

namespace Kaguya.Internal.Events.ArgModels
{
    public class AntiraidEventArgs : EventArgs
    {
        /// <summary>
        /// A collection of IDs to action
        /// </summary>
        public IList<ulong> UserIds { get; set; }
        
        public string DmMessage { get; set; }
        
        public AntiraidAction Action { get; set; }
        
        /// <summary>
        /// When the appropriate punishment would be lifted.
        /// </summary>
        public DateTime? Expiration { get; set; }
    }
}