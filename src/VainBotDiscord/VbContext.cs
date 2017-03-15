using Microsoft.EntityFrameworkCore;
using MySQL.Data.Entity.Extensions;
using System;
using VainBotDiscord.Twitch;
using VainBotDiscord.YouTube;

namespace VainBotDiscord
{
    public class VbContext : DbContext
    {
        public DbSet<UserPoints> UserPoints { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<KeyValue> KeyValues { get; set; }
        public DbSet<StreamToCheck> StreamsToCheck { get; set; }
        public DbSet<StreamRecord> StreamRecords { get; set; }
        public DbSet<StreamGame> StreamGames { get; set; }
        public DbSet<YouTubeToCheck> YouTubesToCheck { get; set; }
        public DbSet<YouTubeRecord> YouTubeRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("VAINBOT_DB_CONNECTIONSTRING");

            if (string.IsNullOrEmpty(dbConnectionString))
                throw new ArgumentNullException(nameof(dbConnectionString), "DB connection string environment variable not found");

            optionsBuilder.UseMySQL(dbConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPoints>(e =>
            {
                e.ToTable("points");
                e.HasKey(up => new { up.ServerId, up.UserId });

                e.Property(up => up.ServerId).HasColumnName("server_id");
                e.Property(up => up.UserId).HasColumnName("user_id");
                e.Property(up => up.Points).IsRequired().HasColumnName("points");
                e.Property(up => up.Allow).IsRequired().HasColumnName("allow");
            });

            modelBuilder.Entity<Admin>(e =>
            {
                e.ToTable("admin");
                e.HasKey(a => new { a.ServerId, a.UserId });

                e.Property(a => a.ServerId).HasColumnName("server_id");
                e.Property(a => a.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<KeyValue>(e =>
            {
                e.ToTable("key_value");
                e.HasKey(kv => kv.Key);

                e.Property(kv => kv.Key).HasColumnName("key");
                e.Property(kv => kv.Value).IsRequired().HasColumnName("value");
            });

            modelBuilder.Entity<StreamToCheck>(e =>
            {
                e.ToTable("stream_to_check");
                e.HasKey(s => s.UserId);

                e.Property(s => s.UserId).HasColumnName("user_id");
                e.Property(s => s.FriendlyUsername).IsRequired().HasMaxLength(200).HasColumnName("friendly_username");
                e.Property(s => s.DiscordChannelId).IsRequired().HasColumnName("discord_channel_id");
                e.Property(s => s.DiscordServerId).IsRequired().HasColumnName("discord_server_id");
                e.Property(s => s.DiscordMessage).IsRequired().HasColumnName("discord_message");
                e.Property(s => s.Frequency).IsRequired().HasColumnName("frequency");
                e.Property(s => s.DeleteDiscordMessage).IsRequired().HasColumnName("delete_discord_message");
                e.Property(s => s.EmbedColor).IsRequired().HasColumnName("embed_color");
            });

            modelBuilder.Entity<StreamRecord>(e =>
            {
                e.ToTable("stream_record");
                e.HasKey(s => s.StreamId);

                e.Property(s => s.StreamId).HasColumnName("stream_id");
                e.Property(s => s.UserId).IsRequired().HasColumnName("user_id");
                e.Property(s => s.DiscordMessageId).IsRequired().HasColumnName("discord_message_id");
                e.Property(s => s.StartTime).HasColumnName("start_time").ValueGeneratedOnAdd();
                e.Property(s => s.CurrentGame).HasColumnName("current_game").IsRequired();

                e.HasMany(s => s.GamesPlayed)
                    .WithOne(g => g.StreamRecord)
                    .HasForeignKey(g => g.StreamId);
            });

            modelBuilder.Entity<StreamGame>(e =>
            {
                e.ToTable("stream_game");
                e.HasKey(g => g.Id);

                e.Property(g => g.StreamId).HasColumnName("stream_id").IsRequired();
                e.Property(g => g.Game).HasColumnName("game").IsRequired().HasMaxLength(200);
                e.Property(g => g.StartTime).HasColumnName("start_time").ValueGeneratedOnAdd();
                e.Property(g => g.StopTime).HasColumnName("stop_time");
            });

            modelBuilder.Entity<YouTubeToCheck>(e =>
            {
                e.ToTable("youtube_to_check");
                e.HasKey(y => y.PlaylistId);

                e.Property(y => y.PlaylistId).HasMaxLength(30).HasColumnName("playlist_id");
                e.Property(y => y.ChannelId).HasMaxLength(200).IsRequired().HasColumnName("channel_id");
                e.Property(y => y.Frequency).IsRequired().HasColumnName("frequency");
                e.Property(y => y.DiscordServerId).IsRequired().HasColumnName("discord_server_id");
                e.Property(y => y.DiscordChannelId).IsRequired().HasColumnName("discord_channel_id");
            });

            modelBuilder.Entity<YouTubeRecord>(e =>
            {
                e.ToTable("youtube_record");
                e.HasKey(y => y.PlaylistId);

                e.Property(y => y.PlaylistId).HasMaxLength(30).HasColumnName("playlist_id");
                e.Property(y => y.VideoId).IsRequired().HasMaxLength(20).HasColumnName("video_id");
                e.Property(y => y.PublishedAt).IsRequired().HasColumnName("published_at");
            });
        }
    }
}
