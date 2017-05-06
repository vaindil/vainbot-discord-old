using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VainBotDiscord.Events
{
    public static class LolCounterEvent
    {
        public static async Task LolCounterAsync(SocketMessage inMsg)
        {
            var msg = inMsg as SocketUserMessage;

            // 110878826136907776
            if (msg.Author.Id != 110878826136907776)
                return;

            var content = msg.Content.ToLower();
            if (content != "lol" && !content.Contains(" lol ") && !content.EndsWith(" lol") && !content.StartsWith("lol "))
                return;

            int count;

            using (var db = new VbContext())
            {
                var countKv = await db.KeyValues.FirstOrDefaultAsync(k => k.Key == DbKey.LolCounter.ToString());
                if (countKv == null)
                {
                    countKv = new KeyValue
                    {
                        Key = DbKey.LolCounter.ToString(),
                        Value = "0"
                    };

                    db.KeyValues.Add(countKv);
                    await db.SaveChangesAsync();
                }

                count = int.Parse(countKv.Value);
                count += 1;
                countKv.Value = count.ToString();

                await db.SaveChangesAsync();
            }

            await msg.AddReactionAsync(Emote.Parse("LUL:232582021493424128"));

            if (count % 5 == 0)
                await msg.Channel.SendMessageAsync(msg.Author.Username + " lol counter: " + count);
        }
    }
}
