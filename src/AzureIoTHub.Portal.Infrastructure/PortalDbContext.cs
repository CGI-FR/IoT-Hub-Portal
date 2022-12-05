// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure
{
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class PortalDbContext : DbContext
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
                    v => JsonConvert.DeserializeObject<LoRaTelemetry>(v));

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
        }
    }
}
