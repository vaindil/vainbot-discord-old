using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using VainBotDiscord.Twitch;

namespace VainBotDiscord
{
    public class VbContext : DbContext
    {
        public DbSet<UserPoints> UserPoints { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<KeyValue> KeyValues { get; set; }
        public DbSet<StreamToCheck> StreamsToCheck { get; set; }
        public DbSet<StreamRecord> StreamRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Environment.GetEnvironmentVariable("VAINBOT_DB_PATH");

            if (string.IsNullOrEmpty(dbPath))
                throw new ArgumentNullException(nameof(dbPath), "DB path environment variable not found");

            if (!File.Exists(dbPath))
                throw new ArgumentNullException(nameof(dbPath), "DB file does not exist at path " + dbPath);

            optionsBuilder.UseSqlite("Filename=" + dbPath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPoints>(e =>
            {
                e.ForSqliteToTable("points");
                e.HasKey(up => new { up.ServerId, up.UserId });

                e.Property(up => up.ServerId).ForSqliteHasColumnName("serverid").ForSqliteHasColumnType("INTEGER");
                e.Property(up => up.UserId).ForSqliteHasColumnName("userid").ForSqliteHasColumnType("INTEGER");
                e.Property(up => up.Points).ForSqliteHasColumnName("points").ForSqliteHasColumnType("REAL");
                e.Property(up => up.Allow).ForSqliteHasColumnName("allow").ForSqliteHasColumnType("INTEGER");
            });

            modelBuilder.Entity<Admin>(e =>
            {
                e.ForSqliteToTable("admin");
                e.HasKey(a => new { a.ServerId, a.UserId });

                e.Property(a => a.ServerId).ForSqliteHasColumnName("serverid").ForSqliteHasColumnType("INTEGER");
                e.Property(a => a.UserId).ForSqliteHasColumnName("userid").ForSqliteHasColumnType("INTEGER");
            });

            modelBuilder.Entity<KeyValue>(e =>
            {
                e.ForSqliteToTable("kv");
                e.HasKey(kv => kv.Key);

                e.Property(kv => kv.Key).ForSqliteHasColumnName("key").ForSqliteHasColumnType("TEXT");
                e.Property(kv => kv.Value).ForSqliteHasColumnName("value").ForSqliteHasColumnType("TEXT");
            });

            modelBuilder.Entity<StreamToCheck>(e =>
            {
                e.ForSqliteToTable("stream_to_check");
                e.HasKey(s => s.UserId);

                e.Property(s => s.UserId).ForSqliteHasColumnName("user_id").ForSqliteHasColumnType("INTEGER");
                e.Property(s => s.DiscordChannelId).ForSqliteHasColumnName("discord_channel_id").ForSqliteHasColumnType("INTEGER");
                e.Property(s => s.DiscordServerId).ForSqliteHasColumnName("discord_server_id").ForSqliteHasColumnType("INTEGER");
                e.Property(s => s.DiscordMessage).ForSqliteHasColumnName("discord_message").ForSqliteHasColumnType("TEXT");
                e.Property(s => s.Frequency).ForSqliteHasColumnName("frequency").ForSqliteHasColumnType("INTEGER");
                e.Property(s => s.DeleteDiscordMessage).ForSqliteHasColumnName("delete_discord_message").ForSqliteHasColumnType("INTEGER");
                e.Property(s => s.EmbedColor).ForSqliteHasColumnName("embed_color").ForSqliteHasColumnType("INTEGER");
            });

            modelBuilder.Entity<StreamRecord>(e =>
            {
                e.ForSqliteToTable("stream_record");
                e.HasKey(s => s.StreamId);

                e.Property(s => s.StreamId).ForSqliteHasColumnName("stream_id").ForSqliteHasColumnType("INTEGER");
                e.Property(s => s.UserId).ForSqliteHasColumnName("user_id").ForSqliteHasColumnType("INTEGER");
                e.Property(s => s.DiscordMessageId).ForSqliteHasColumnName("discord_message_id").ForSqliteHasColumnType("INTEGER");
            });
        }
    }
}
