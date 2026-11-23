

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SubWatch.Api.models;

namespace SubWatch.Api.data;

public class AppDbContext: IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}

    public DbSet<Subscription> Subscriptions {get; set;}
    public DbSet<RefreshToken> RefreshToken {get; set;}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Subscription>()
            .HasOne(s =>s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>()
        .HasOne(rt => rt.User)
        .WithMany()
        .HasForeignKey(rt => rt.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    }
}