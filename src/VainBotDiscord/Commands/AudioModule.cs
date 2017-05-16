using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VainBotDiscord.Utils;

namespace VainBotDiscord.Commands
{
    public class AudioModule : ModuleBase<VbCommandContext>
    {
        [Command("rainbow", RunMode = RunMode.Async)]
        [Alias("readingrainbow")]
        [CrendorServerOnly]
        public async Task PlayReadingRainbow()
        {
            var channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("You're not in a voice channel, you nerd.");
                return;
            }

            var audioClient = await channel.ConnectAsync();
            await SendAudioAsync(audioClient, $"Audio{Path.DirectorySeparatorChar}readingrainbow.mp3");
            await audioClient.StopAsync();
        }

        [CrendorServerOnly]
        [Command("tts", RunMode = RunMode.Async)]
        public async Task TextToSpeech([Remainder]string words)
        {
            var channel = (Context.Message.Author as IGuildUser).VoiceChannel;

            if (channel == null)
            {
                await ReplyAsync("You're not in a voice channel, you nerd.");
                return;
            }

            if (words.Length > 200)
            {
                await ReplyAsync("That's a little too long, sorry.");
                return;
            }

            var filePath = $"TTSTemp{Path.DirectorySeparatorChar}{DateTime.UtcNow.Ticks}.wav";

            CreateTtsFile(filePath, words);

            var audioClient = await channel.ConnectAsync();
            await SendAudioAsync(audioClient, filePath);
            await audioClient.StopAsync();

            File.Delete(filePath);
        }

        [IsAdmin]
        [Command("ttsin", RunMode = RunMode.Async)]
        public async Task TextToSpeechWithChannel(IVoiceChannel channel, [Remainder]string words)
        {
            if (words.Length > 400)
            {
                await ReplyAsync("That's a little too long, sorry.");
                return;
            }

            var filePath = $"TTSTemp{Path.DirectorySeparatorChar}{DateTime.UtcNow.Ticks}.wav";

            CreateTtsFile(filePath, words);

            var audioClient = await channel.ConnectAsync();
            await SendAudioAsync(audioClient, filePath);
            await audioClient.StopAsync();

            File.Delete(filePath);
        }

        void CreateTtsFile(string filePath, string words)
        {
            words = words.Replace("\"", "").Replace("'", "");

            var createWav = new ProcessStartInfo
            {
                FileName = "pico2wave",
                Arguments = $"--wave={filePath} \"{words}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false
            };

            var cwProc = Process.Start(createWav);
            cwProc.Dispose();
        }

        async Task SendAudioAsync(IAudioClient audioClient, string path)
        {
            var ffmpeg = CreateStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = audioClient.CreatePCMStream(AudioApplication.Voice, 1920);

            await output.CopyToAsync(discord);
            await discord.FlushAsync();

            ffmpeg.Kill();
            ffmpeg.WaitForExit();
            ffmpeg.Dispose();
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
