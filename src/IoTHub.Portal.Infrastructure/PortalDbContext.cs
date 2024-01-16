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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PortalDbContext(DbContextOptions<PortalDbContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

            _ = modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            _ = modelBuilder.Entity<AccessControl>()
                .HasOne(ac => ac.Role)
                .WithMany()
                .HasForeignKey(ac => ac.RoleName)
                .OnDelete(DeleteBehavior.Restrict);

            _ = modelBuilder.Entity<AccessControl>()
                .HasOne(ac => ac.User)
                .WithMany(u => u.AccessControls)
                .HasForeignKey(ac => ac.UserId)
                .IsRequired(false);

            _ = modelBuilder.Entity<AccessControl>()
                .HasOne(ac => ac.Group)
                .WithMany(g => g.AccessControls)
                .HasForeignKey(ac => ac.GroupId)
                .IsRequired(false);
        }
    }
}
