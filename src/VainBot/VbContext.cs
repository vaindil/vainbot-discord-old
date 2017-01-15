using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VainBot
{
    public class VbContext : DbContext
    {
        public DbSet<UserPoints> UserPoints { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<KeyValue> KeyValues { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./discord.db");
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
        }
    }
}
