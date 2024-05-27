// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure
{
    using Domain.Entities;
    using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class PortalDbContext : DbContext, IDataProtectionKeyContext
    {
        public DbSet<DeviceModelProperty> DeviceModelProperties { get; set; }
        public DbSet<DeviceTag> DeviceTags { get; set; }
        public DbSet<DeviceModelCommand> DeviceModelCommands { get; set; }
        public DbSet<DeviceModel> DeviceModels { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<EdgeDevice> EdgeDevices { get; set; }
        public DbSet<LorawanDevice> LorawanDevices { get; set; }
        public DbSet<DeviceTagValue> DeviceTagValues { get; set; }
        public DbSet<EdgeDeviceModel> EdgeDeviceModels { get; set; }
        public DbSet<EdgeDeviceModelCommand> EdgeDeviceModelCommands { get; set; }
        public DbSet<Concentrator> Concentrators { get; set; }
        public DbSet<LoRaDeviceTelemetry> LoRaDeviceTelemetry { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<AccessControl> AccessControls { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Action> Actions { get; set; }

        public PortalDbContext(DbContextOptions<PortalDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _ = modelBuilder.Entity<LoRaDeviceTelemetry>()
                .Property(b => b.Telemetry)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<LoRaTelemetry>(v)!);

            _ = modelBuilder.Entity<Device>()
                .ToTable($"{nameof(Device)}s")
                .HasOne(x => x.DeviceModel)
                .WithMany()
                .HasForeignKey(x => x.DeviceModelId);

            _ = modelBuilder.Entity<LorawanDevice>()
                .ToTable($"{nameof(LorawanDevice)}s")
                .HasOne(x => x.DeviceModel)
                .WithMany()
                .HasForeignKey(x => x.DeviceModelId);

            _ = modelBuilder.Entity<EdgeDevice>()
                .HasOne(x => x.DeviceModel)
                .WithMany()
                .HasForeignKey(x => x.DeviceModelId);

            _ = modelBuilder.Entity<UserMemberShip>()
                .HasKey(m => new { m.UserId, m.GroupId });

            _ = modelBuilder.Entity<UserMemberShip>()
                .HasOne(m => m.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(m => m.UserId);

            _ = modelBuilder.Entity<UserMemberShip>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId);

            _ = modelBuilder.Entity<Group>()
                .HasMany(a => a.AccessControls);

            _ = modelBuilder.Entity<User>()
               .HasMany(a => a.AccessControls);

            _ = modelBuilder.Entity<Role>()
                .HasMany(a => a.Actions);

            _ = modelBuilder.Entity<AccessControl>()
                .HasOne(r => r.Role);

            _ = modelBuilder.Entity<EdgeDevice>()
                .HasMany(c => c.Tags)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<Device>()
                .HasMany(c => c.Tags)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
