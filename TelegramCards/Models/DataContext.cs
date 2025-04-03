using Microsoft.EntityFrameworkCore;
using TelegramCards.Models.Entitys;

namespace TelegramCards.Models;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options) 
{
    public DbSet<User> Users { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardBase> CardBases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(e => e.Cards)
            .WithOne(e => e.Owner)
            .HasForeignKey(e => e.OwnerId)
            .IsRequired();

        modelBuilder.Entity<CardBase>()
            .HasMany(e => e.Cards)
            .WithOne(e => e.BaseCard)
            .HasForeignKey(e => new{e.RarityLevel, e.CardIndex})
            .HasPrincipalKey(e => new{e.RarityLevel, e.CardIndex})
            .IsRequired();

        modelBuilder.Entity<CardBase>()
            .HasKey(c => new { c.RarityLevel, c.CardIndex });
    }
}