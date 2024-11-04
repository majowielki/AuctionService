﻿using AuctionService.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.API.Data
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Auction> Auctions { get; set; }
        //public DbSet<Item> Items { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Auction>()
        //        .HasOne(a => a.Item)
        //        .WithOne(i => i.Auction)
        //        .HasForeignKey<Item>(i => i.AuctionId);
        //}
    }
}
