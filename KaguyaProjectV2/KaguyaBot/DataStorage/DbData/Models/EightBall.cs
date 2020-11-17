using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "eightball")]
    public class EightBall : IKaguyaQueryable<EightBall>, IKaguyaUnique<EightBall>
    {
        [PrimaryKey]
        [Column(Name = "response")]
        public string Response { get; set; }
    }
}