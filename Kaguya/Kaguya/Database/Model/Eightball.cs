using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Internal.Enums;

namespace Kaguya.Database.Model
{
    public class Eightball
    {
        /// <summary>
        /// An eightball response.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Phrase { get; set; }
        /// <summary>
        /// The outlook (positive, negative, or neutral) of this phrase.
        /// </summary>
        public EightballOutlook Outlook { get; set; }
    }
}