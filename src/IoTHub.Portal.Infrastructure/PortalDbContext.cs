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
        public DbSet<Member> Members { get; set; }
        public DbSet<AccessControl> AccessControls { get; set; }
        public DbSet<UserAccessControl> UserAccessControls { get; set; }
        public DbSet<GroupAccessControl> GroupAccessControls { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleAction> RoleActions { get; set; }
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

            _ = modelBuilder.Entity<Member>()
                .HasKey(m => new { m.UserId, m.GroupId });

            _ = modelBuilder.Entity<Member>()
                .HasOne(m => m.User)
                .WithMany(u => u.Members)
                .HasForeignKey(m => m.UserId);

            _ = modelBuilder.Entity<Member>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId);
            _ = modelBuilder.Entity<UserAccessControl>()
                .HasKey(ua => new { ua.UserId, ua.AccessControlId });

            _ = modelBuilder.Entity<UserAccessControl>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAccessControls)
                .HasForeignKey(ua => ua.UserId);

            _ = modelBuilder.Entity<UserAccessControl>()
                .HasOne(ua => ua.AccessControl)
                .WithMany()
                .HasForeignKey(ua => ua.AccessControlId);

            _ = modelBuilder.Entity<GroupAccessControl>()
                .HasKey(ga => new { ga.GroupId, ga.AccessControlId });

            _ = modelBuilder.Entity<GroupAccessControl>()
                .HasOne(ga => ga.Group)
                .WithMany(g => g.GroupAccessControls)
                .HasForeignKey(ga => ga.GroupId);

            _ = modelBuilder.Entity<GroupAccessControl>()
                .HasOne(ga => ga.AccessControl)
                .WithMany()
                .HasForeignKey(ga => ga.AccessControlId);

            _ = modelBuilder.Entity<AccessControl>()
                .HasOne(ac => ac.Role)
                .WithMany(r => r.AccessControls)
                .HasForeignKey(ac => ac.RoleId);
            _ = modelBuilder.Entity<Member>()
                .HasOne(m => m.User)
                .WithMany(u => u.Members)
                .HasForeignKey(m => m.UserId);

            _ = modelBuilder.Entity<Member>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId);

            _ = modelBuilder.Entity<UserAccessControl>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAccessControls)
                .HasForeignKey(ua => ua.UserId);

            _ = modelBuilder.Entity<UserAccessControl>()
                .HasOne(ua => ua.AccessControl)
                .WithMany(ac => ac.UserAccessControls)
                .HasForeignKey(ua => ua.AccessControlId);

            _ = modelBuilder.Entity<GroupAccessControl>()
                .HasOne(ga => ga.Group)
                .WithMany(g => g.GroupAccessControls)
                .HasForeignKey(ga => ga.GroupId);

            _ = modelBuilder.Entity<GroupAccessControl>()
                .HasOne(ga => ga.AccessControl)
                .WithMany(ac => ac.GroupAccessControls)
                .HasForeignKey(ga => ga.AccessControlId);

            _ = modelBuilder.Entity<RoleAction>()
                .HasOne(ra => ra.Role)
                .WithMany(r => r.RoleActions)
                .HasForeignKey(ra => ra.RoleId);

            _ = modelBuilder.Entity<RoleAction>()
                .HasOne(ra => ra.Action)
                .WithMany(a => a.RoleActions)
                .HasForeignKey(ra => ra.ActionId);
        }
    }
}
