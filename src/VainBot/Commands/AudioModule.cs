﻿using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
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
            await SendAudioAsync(audioClient, $"Audio{Path.DirectorySeparatorChar}readingrainbow.mp3");
            await audioClient.DisconnectAsync();
        }

        [Command("tts", RunMode = RunMode.Async)]
        public async Task TextToSpeech([Remainder]string message)
        {
            var channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("You're not in a voice channel, you nerd.");
                return;
            }

            if (message.Length > 200)
            {
                await ReplyAsync("That's a little too long, sorry.");
                return;
            }

            var filePath = $"TTSTemp{Path.DirectorySeparatorChar}{DateTime.UtcNow.Ticks.ToString()}.wav";

            var createWav = new ProcessStartInfo
            {
                FileName = "pico2wave",
                Arguments = $"--wave={filePath} \"{message}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false
            };

            var cwProc = Process.Start(createWav);
            cwProc.Dispose();

            var audioClient = await channel.ConnectAsync();
            await SendAudioAsync(audioClient, filePath);
            await audioClient.DisconnectAsync();

            File.Delete(filePath);
        }

        async Task SendAudioAsync(IAudioClient audioClient, string path)
        {
            var ffmpeg = CreateStream(path);
            var output = ffmpeg.StandardOutput.BaseStream;
            var discord = audioClient.CreatePCMStream(1920);

            await output.CopyToAsync(discord);
            await discord.FlushAsync();

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
