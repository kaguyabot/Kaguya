using System;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    // TODO: Test if this owneronly actually blocks execution from non owners.
    [Restriction(ModuleRestriction.OwnerOnly)]
    [Module(CommandModule.OwnerOnly)]
    [Group("blacklist")]
    public class Blacklist : KaguyaBase<Blacklist>
    {
        private readonly ILogger<Blacklist> _logger;
        private readonly BlacklistedEntityRepository _blacklistedEntityRepository;

        public Blacklist(ILogger<Blacklist> logger, BlacklistedEntityRepository blacklistedEntityRepository) : base(logger)
        {
            _logger = logger;
            _blacklistedEntityRepository = blacklistedEntityRepository;
        }

        [Command("-add")]
        [Summary("Blacklists the entity based on ID for an optional duration.\n" +
                 "Valid entity types are `channel`, `user`, and `server`.")]
        [Remarks("<entity id> <entity type> [duration] [reason]")]
        public async Task BlacklistEntityCommand(ulong id, string type, string duration = null, [Remainder]string reason = null)
        {
            BlacklistedEntity entity = await _blacklistedEntityRepository.GetAsync(id);
            if (entity != null)
            {
                await SendBasicErrorEmbedAsync("This entity is already blacklisted.\n\n" + EntityStatus(entity));
            }
            else
            {
                DateTime? expiration = null;
                if (duration != null)
                {
                    var parser = new TimeParser(duration);
                    var parsedDuration = parser.ParseTime();
                    if (parsedDuration != TimeSpan.Zero)
                    {
                        expiration = DateTime.Now.Add(parsedDuration);
                    }
                }
                if (Enum.TryParse(type, true, out BlacklistedEntityType entityType))
                {
                    var newEntity = new BlacklistedEntity
                    {
                        EntityId = id,
                        EntityType = entityType,
                        ExpirationTime = expiration,
                        Reason = reason
                    };
                    
                    await _blacklistedEntityRepository.InsertAsync(newEntity);

                    await SendBasicSuccessEmbedAsync($"Blacklisted entity with ID {newEntity.EntityId.ToString().AsBold()}.");
                }
                else
                {
                    await SendBasicErrorEmbedAsync("Could not parse entity type.");
                }
            }
        }

        [Command("-undo")]
        [Alias("-remove", "-u")]
        [Remarks("<entity id>")]
        public async Task UnblacklistCommand(ulong id)
        {
            var match = await _blacklistedEntityRepository.GetAsync(id);
            if (match == null || match.HasExpired)
            {
                await SendBasicErrorEmbedAsync("This entity is not blacklisted.");

                return;
            }

            match.ExpirationTime = DateTime.Now;
            await _blacklistedEntityRepository.UpdateAsync(match);
            
            await SendBasicSuccessEmbedAsync($"Unblacklisted entity with ID {id.ToString().AsBold()}");
        }

        [Command("-status")]
        [Summary("View the status of a blacklisted entity")]
        [Remarks("<entity id>")]
        public async Task ViewBlacklistCommand(ulong id)
        {
            var entity = await _blacklistedEntityRepository.GetAsync(id);

            if (entity != null)
            {
                await SendBasicSuccessEmbedAsync(EntityStatus(entity));
            }
            else
            {
                await SendBasicErrorEmbedAsync("The given entity is not blacklisted.");
            }
        }

        private string EntityStatus(BlacklistedEntity entity)
        {
            return $"ID: {entity.EntityId.ToString().AsBold()}\n" +
                   $"Type: {entity.EntityType.Humanize(LetterCasing.Title).AsBold()}\n" +
                   $"Reason: {entity.Reason ?? "<No reason>".AsBold()}\n" +
                   $"Expiration: {entity.ExpirationTime?.Humanize(false).AsBold() ?? "Permanent".AsBold()}";
        }
    }
}