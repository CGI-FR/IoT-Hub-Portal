// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Services
{
    using System.Threading;
    using Microsoft.AspNetCore.Components;
    using IoTHub.Portal.Client.Shared;
    using IoTHub.Portal.Client.Exceptions;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Models.v10;
    using System.Collections.Generic;
    using IoTHub.Portal.Shared.Models.v1._0;
    using IoTHub.Portal.Shared.Models.v10;
    using System.Collections.ObjectModel;
    using System;
    using System.Globalization;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Linq;
    using IoTHub.Portal.Shared.Constants;

    public class PlanningCommand
    {
        public string planningId { get; set; } = default!;
        public Collection<string> listDeviceId { get; } = new Collection<string>();
        public Dictionary<DaysEnumFlag.DaysOfWeek, List<PayloadCommand>> commands { get; } = new Dictionary<DaysEnumFlag.DaysOfWeek, List<PayloadCommand>>();


        public PlanningCommand(string listDeviceId, string planningId)
        {
            this.planningId = planningId;
            this.listDeviceId.Add(listDeviceId);

            foreach (DaysEnumFlag.DaysOfWeek day in Enum.GetValues(typeof(DaysEnumFlag.DaysOfWeek)))
            {
                commands.Add(day, new List<PayloadCommand>());
            }
        }
    }

    public class PayloadCommand
    {
        public string payloadId { get; set; } = default!;
        public TimeSpan start { get; set; } = default!;
        public TimeSpan end { get; set; } = default!;

        public PayloadCommand(TimeSpan start, TimeSpan end, string payloadId)
        {
            this.payloadId = payloadId;
            this.start = start;
            this.end = end;
        }
    }

    public class SendPlanningCommandService : ISendPlanningCommandService, IHostedService, IDisposable
    {
        [CascadingParameter]
        private Error Error { get; set; } = default!;

        private readonly CancellationTokenSource cancellationTokenSource;
        private bool isUpdating;

        private readonly List<PlanningCommand> planningCommands = new List<PlanningCommand>();

        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly ILayerService layerService;
        private readonly IPlanningService planningService;
        private readonly IScheduleService scheduleService;
        private readonly ILoRaWANCommandService loRaWANCommandService;

        public PaginatedResult<DeviceListItem> devices { get; set; } = new PaginatedResult<DeviceListItem>();
        public IEnumerable<LayerDto> layers { get; set; } = new List<LayerDto>();
        public IEnumerable<PlanningDto> plannings { get; set; } = new List<PlanningDto>();
        public IEnumerable<ScheduleDto> schedules { get; set; } = new List<ScheduleDto>();

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<SendPlanningCommandService> logger;

        /// <summary>
        /// The service scope.
        /// </summary>
        private readonly IServiceScope serviceScope;

        /// <summary>
        /// The timer period.
        /// </summary>
        private readonly TimeSpan timerPeriod;

        /// <summary>
        /// The timer.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The executing task.
        /// </summary>
        private Task executingTask;

        public SendPlanningCommandService(
            ILogger<SendPlanningCommandService> logger,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;

            this.serviceScope = serviceProvider.CreateScope();

            this.deviceService = this.serviceScope.ServiceProvider.GetRequiredService<IDeviceService<DeviceDetails>>();
            this.layerService = this.serviceScope.ServiceProvider.GetRequiredService<ILayerService>();
            this.planningService = this.serviceScope.ServiceProvider.GetRequiredService<IPlanningService>();
            this.scheduleService = this.serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();
            this.loRaWANCommandService = this.serviceScope.ServiceProvider.GetRequiredService<ILoRaWANCommandService>();

            this.cancellationTokenSource = new CancellationTokenSource();

            var timeSpanSeconds = 600;
            this.timerPeriod = TimeSpan.FromSeconds(timeSpanSeconds);
            this.isUpdating = true;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>
        /// Async task.
        /// </returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create the timer
            this.timer = new Timer(this.OnTimerCallback, null, TimeSpan.Zero, this.timerPeriod);
        }

        /// <summary>
        /// Does the work asynchronous.
        /// </summary>
        /// <param name="stoppingToken">The stopping token.</param>
        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                if (this.isUpdating)
                {
                    this.planningCommands.Clear();
                    await UpdateAPI();
                    UpdateDatabase();
                    this.isUpdating = false;
                }
                await SendCommand();
            }
            catch (ProblemDetailsException exception)
            {
                Error?.ProcessProblemDetails(exception);
            }

            _ = this.timer.Change(this.timerPeriod, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>
        /// Async task.
        /// </returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _ = (this.timer?.Change(Timeout.Infinite, 0));

            try
            {
                await this.cancellationTokenSource.CancelAsync();
            }
            catch (ProblemDetailsException exception)
            {
                Error?.ProcessProblemDetails(exception);
            }
        }

        /// <summary>
        /// Called when [timer callback].
        /// </summary>
        /// <param name="state">The state.</param>
        private void OnTimerCallback(object state)
        {
            GC.KeepAlive(this.timer);
            _ = (this.timer?.Change(Timeout.Infinite, 0));
            this.executingTask = this.DoWorkAsync(this.cancellationTokenSource.Token);
        }

        public async Task UpdateAPI()
        {
            try
            {
                devices = await this.deviceService.GetDevices();
                layers = await this.layerService.GetLayers();
                plannings = await this.planningService.GetPlannings();
                schedules = await this.scheduleService.GetSchedules();
            }
            catch (ProblemDetailsException exception)
            {
                Error?.ProcessProblemDetails(exception);
            }
        }

        public void UpdateDatabase()
        {
            foreach (var device in this.devices.Data)
            {
                if (device.LayerId != null) AddNewDevice(device);
            }
        }

        public void AddNewDevice(DeviceListItem device)
        {
            LayerDto layer = layers.FirstOrDefault(layer => layer.Id == device.LayerId);

            // If the layer linked to a device already has a planning, add the device to the planning list
            foreach (PlanningCommand planning in this.planningCommands.Where(planning => planning.planningId == layer.Planning))
            {
                planning.listDeviceId.Add(device.DeviceID);
                return;
            }

            // Else create the planning
            PlanningCommand newPlanning = new PlanningCommand(device.DeviceID, layer.Planning);
            AddCommand(newPlanning);
            this.planningCommands.Add(newPlanning);
        }

        public void AddCommand(PlanningCommand planningCommand)
        {
            PlanningDto planningData = plannings.FirstOrDefault(planning => planning.Id == planningCommand.planningId);

            // Connect off days command to the planning
            addPlanningSchedule(planningData, planningCommand);


            foreach (ScheduleDto schedule in schedules)
            {
                // Add schedules to the planning
                if (schedule.PlanningId == planningCommand.planningId) addSchedule(schedule, planningCommand);
            }

        }

        // Include Planning Commands used for off days in the command dictionary.
        // "Sa" represents Saturday and serves as a dictionary key.
        // planning.commands[Sa] contains a list of PayloadCommand Values.
        public void addPlanningSchedule(PlanningDto planningData, PlanningCommand planning)
        {
            foreach (DaysEnumFlag.DaysOfWeek key in planning.commands.Keys)
            {
                if ((planningData.DayOff & key) == planningData.DayOff)
                {
                    PayloadCommand newPayload = new PayloadCommand(getTimeSpan("0:00"), getTimeSpan("24:00"), planningData.CommandId);
                    planning.commands[key].Add(newPayload);
                }
            }
        }

        public void addSchedule(ScheduleDto schedule, PlanningCommand planning)
        {
            // Convert a string into TimeSpan format
            TimeSpan start = getTimeSpan(schedule.Start);
            TimeSpan end = getTimeSpan(schedule.End);

            foreach (DaysEnumFlag.DaysOfWeek key in planning.commands.Keys)
            {
                if (planning.commands[key].Count == 0)
                {
                    PayloadCommand newPayload = new PayloadCommand(start, end, schedule.CommandId);
                    planning.commands[key].Add(newPayload);
                }
                // The if condition is utilized to skip day off schedules.
                else if (planning.commands[key][0].start != getTimeSpan("00:00") || planning.commands[key][0].end != getTimeSpan("24:00"))
                {
                    PayloadCommand newPayload = new PayloadCommand(start, end, schedule.CommandId);
                    planning.commands[key].Add(newPayload);
                }
            }
        }

        public TimeSpan getTimeSpan(string time)
        {
            var tabTime = time != null ? time.Split(':') : ("0:0").Split(':');

            int hour = int.Parse(tabTime[0], CultureInfo.InvariantCulture);
            int minute = int.Parse(tabTime[1], CultureInfo.InvariantCulture);

            return new TimeSpan(hour, minute, 0);
        }

        public async Task SendCommand()
        {
            string timeZoneId = "Europe/Paris";
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);

            DayOfWeek currentDay = currentTime.DayOfWeek;
            TimeSpan currentHour = currentTime.TimeOfDay ;

            // Search for the appropriate command at the correct time from each plan.
            foreach (var planning in this.planningCommands)
            {
                foreach (var schedule in planning.commands[DayConverter.Convert(currentDay)])
                {
                    if (schedule.start < currentHour && schedule.end > currentHour)
                    {
                        await SendDevicesCommand(planning.listDeviceId, schedule.payloadId);
                    }
                }
            }
        }

        public async Task SendDevicesCommand(Collection<string> devices, string command)
        {
            foreach (var device in devices) await loRaWANCommandService.ExecuteLoRaWANCommand(device, command);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.timer?.Dispose();
            this.timer = null;

            this.cancellationTokenSource?.Dispose();
        }
    }
}
