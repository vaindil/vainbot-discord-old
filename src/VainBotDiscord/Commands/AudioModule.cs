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
        public async Task PlayReadingRainbow()
        {
            await PlayFile("readingrainbow.mp3");
        }

        [Command("dejavu", RunMode = RunMode.Async)]
        public async Task PlayDejaVu()
        {
            await PlayFile("dejavu.mp3");
        }

        [Command("witchdoctor", RunMode = RunMode.Async)]
        [Alias("magicword", "magicwords")]
        public async Task PlayWitchDoctor()
        {
            await PlayFile("witchdoctor.mp3");
        }

        [Command("airhorn", RunMode = RunMode.Async)]
        [Alias("horn")]
        public async Task PlayAirhorn()
        {
            await PlayFile("airhorn.mp3");
        }

        // [CrendorServerOnly]
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

        async Task PlayFile(string filePath)
        {
            var channel = (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("You're not in a voice channel, you nerd.");
                return;
            }

            var audioClient = await channel.ConnectAsync();
            await SendAudioAsync(audioClient, $"Audio{Path.DirectorySeparatorChar}{filePath}");
            await audioClient.StopAsync();
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
            var discord = audioClient.CreatePCMStream(AudioApplication.Mixed);

            await output.CopyToAsync(discord);
            await discord.FlushAsync();

            if (!ffmpeg.HasExited)
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
