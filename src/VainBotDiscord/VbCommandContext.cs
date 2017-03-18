using Discord;
using Discord.Commands;

namespace VainBotDiscord
{
    public class VbCommandContext : CommandContext
    {
        public VbCommandContext(IDiscordClient client, IUserMessage msg) : base(client, msg)
        {
        }

        public ServerMainUser MainUser { get; set; }

        public bool HasMainUser
        {
            get
            {
                return MainUser != null;
            }
        }
    }
}
