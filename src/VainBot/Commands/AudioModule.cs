using Discord;
using Discord.Commands;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VainBotDiscord.Commands
{
    public class AudioModule : ModuleBase
    {
        [Command("rainbow", RunMode = RunMode.Async)]
        [Alias("readingrainbow")]
        public async Task PlayReadingRainbow()
        {
            var channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("You're not in a voice channel, you nerd.");
                return;
            }

            var audioClient = await channel.ConnectAsync();
            var ffmpeg = CreateStream($"Audio{Path.DirectorySeparatorChar}readingrainbow.mp3");
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = audioClient.CreatePCMStream(1920);

            await output.CopyToAsync(discord);
            await discord.FlushAsync();

            await audioClient.DisconnectAsync();
        }
        
        Process CreateStream(string path)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            return Process.Start(ffmpeg);
        }
    }
}
