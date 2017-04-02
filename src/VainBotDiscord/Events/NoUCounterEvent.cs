using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VainBotDiscord.Events
{
    public static class NoUCounterEvent
    {
        public static async Task NoUCounterAsync(SocketMessage inMsg)
        {
            var msg = inMsg as SocketUserMessage;

            // 110878826136907776
            if (msg.Author.Id != 117028123303870473)
                return;

            var content = msg.Content.ToLower();
            if (content != "no u"
                && content != "nou"
                && !content.Contains(" no u ")
                && !content.Contains(" nou ")
                && !content.EndsWith(" no u")
                && !content.EndsWith(" nou")
                && !content.StartsWith("no u ")
                && !content.StartsWith("nou "))
                return;

            int count;

            using (var db = new VbContext())
            {
                var countKv = await db.KeyValues.FirstOrDefaultAsync(k => k.Key == DbKey.NoUCounter.ToString());

                count = int.Parse(countKv.Value);
                count += 1;
                countKv.Value = count.ToString();

                await db.SaveChangesAsync();
            }

            await msg.AddReactionAsync("pepe:266080004897636352");

            if (count % 5 == 0 || count == 1)
                await msg.Channel.SendMessageAsync(msg.Author.Username + " no u counter: " + count);
        }
    }
}
