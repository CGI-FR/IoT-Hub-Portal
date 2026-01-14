// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
    public class PlanningCommand
    {
        public string PlanningId { get; }
        public Collection<string> ListDeviceId { get; } = new Collection<string>();
        public Dictionary<DaysEnumFlag.DaysOfWeek, List<PayloadCommand>> Commands { get; } = new Dictionary<DaysEnumFlag.DaysOfWeek, List<PayloadCommand>>();

        public PlanningCommand(string listDeviceId, string planningId)
        {
            PlanningId = planningId;
            ListDeviceId.Add(listDeviceId);

            foreach (DaysEnumFlag.DaysOfWeek day in Enum.GetValues(typeof(DaysEnumFlag.DaysOfWeek)))
            {
                Commands.Add(day, new List<PayloadCommand>());
            }
        }
    }

    public class PayloadCommand
    {
        public string PayloadId { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public PayloadCommand(TimeSpan start, TimeSpan end, string payloadId)
        {
            PayloadId = payloadId;
            Start = start;
            End = end;
        }
    }

    [DisallowConcurrentExecution]
    public class SendPlanningCommandJob : IJob
    {
        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly ILayerService layerService;
        private readonly IPlanningService planningService;
        private readonly IScheduleService scheduleService;
        private readonly ILoRaWanCommandService loRaWanCommandService;

        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly List<PlanningCommand> planningCommands = new List<PlanningCommand>();
        public PaginatedResult<DeviceListItem> Devices { get; set; } = new PaginatedResult<DeviceListItem>();
        public IEnumerable<LayerDto> Layers { get; set; } = new List<LayerDto>();
        public IEnumerable<PlanningDto> Plannings { get; set; } = new List<PlanningDto>();
        public IEnumerable<ScheduleDto> Schedules { get; set; } = new List<ScheduleDto>();
        private readonly ILogger<SendPlanningCommandJob> logger;

        public SendPlanningCommandJob(IDeviceService<DeviceDetails> deviceService,
            ILayerService layerService,
            IPlanningService planningService,
            IScheduleService scheduleService,
            ILoRaWanCommandService loRaWanCommandService,
            ILogger<SendPlanningCommandJob> logger)
        {
            this.logger = logger;

            this.deviceService = deviceService;
            this.layerService = layerService;
            this.planningService = planningService;
            this.scheduleService = scheduleService;
            this.loRaWanCommandService = loRaWanCommandService;

            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of send planning commands job");

                await DoWork(this.cancellationTokenSource.Token);

                this.logger.LogInformation("End of send planning commands job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Send planning commands job has failed");
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                this.planningCommands.Clear();
                await UpdateApi();
                UpdateDatabase();

                await SendCommand();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Send planning command has failed");
            }
        }

        public async Task UpdateApi()
        {
            try
            {
                Devices = await this.deviceService.GetDevices(pageSize: 10000);
                Layers = await this.layerService.GetLayers();
                Plannings = await this.planningService.GetPlannings();
                Schedules = await this.scheduleService.GetSchedules();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Update API has failed");
            }
        }

        public void UpdateDatabase()
        {
            if (Devices.Data == null) return;

            foreach (var device in Devices.Data)
            {
                if (!string.IsNullOrWhiteSpace(device.LayerId)) AddNewDevice(device);
            }
        }

        public void AddNewDevice(DeviceListItem device)
        {
            var layer = Layers.FirstOrDefault(layer => layer.Id == device.LayerId);

            if (layer?.Planning is not null and not "None")
            {
                // If the layer linked to a device already has a planning, add the device to the planning list
                foreach (var planning in this.planningCommands.Where(planning => planning.PlanningId == layer.Planning))
                {
                    planning.ListDeviceId.Add(device.DeviceId);
                    return;
                }

                // Else create the planning
                var newPlanning = new PlanningCommand(device.DeviceId, layer.Planning);
                AddCommand(newPlanning);
                this.planningCommands.Add(newPlanning);
            }
        }

        public void AddCommand(PlanningCommand planningCommand)
        {
            var planningData = Plannings.FirstOrDefault(planning => planning.Id == planningCommand.PlanningId);

            // If planning is active
            if (planningData != null && IsPlanningActive(planningData))
            {
                // Connect off days command to the planning
                AddPlanningSchedule(planningData, planningCommand);


                foreach (var schedule in Schedules)
                {
                    // Add schedules to the planning
                    if (schedule.PlanningId == planningCommand.PlanningId) AddSchedule(schedule, planningCommand);
                }
            }
        }

        private bool IsPlanningActive(PlanningDto planning)
        {
            var startDay = DateTime.ParseExact(planning.Start, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDay = DateTime.ParseExact(planning.End, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            return DateTime.Now >= startDay && DateTime.Now <= endDay;
        }

        // Include Planning Commands used for off days in the command dictionary.
        // "Sa" represents Saturday and serves as a dictionary key.
        // planning.commands[Sa] contains a list of PayloadCommand Values.
        public void AddPlanningSchedule(PlanningDto planningData, PlanningCommand planning)
        {
            foreach (var key in planning.Commands.Keys)
            {
                if ((planningData.DayOff & key) == planningData.DayOff)
                {
                    var newPayload = new PayloadCommand(GetTimeSpan("0:00"), GetTimeSpan("24:00"), planningData.CommandId);
                    planning.Commands[key].Add(newPayload);
                }
            }
        }

        public void AddSchedule(ScheduleDto schedule, PlanningCommand planning)
        {
            // Convert a string into TimeSpan format
            var start = GetTimeSpan(schedule.Start);
            var end = GetTimeSpan(schedule.End);

            foreach (var key in planning.Commands.Keys)
            {
                if (planning.Commands[key].Count == 0)
                {
                    var newPayload = new PayloadCommand(start, end, schedule.CommandId);
                    planning.Commands[key].Add(newPayload);
                }
                // The if condition is utilized to skip day off schedules.
                else if (planning.Commands[key][0].Start != GetTimeSpan("00:00") || planning.Commands[key][0].End != GetTimeSpan("24:00"))
                {
                    var newPayload = new PayloadCommand(start, end, schedule.CommandId);
                    planning.Commands[key].Add(newPayload);
                }
            }
        }

        public static TimeSpan GetTimeSpan(string? time)
        {
            var tabTime = time?.Split(':') ?? ("0:0").Split(':');

            var hour = int.Parse(tabTime[0], CultureInfo.InvariantCulture);
            var minute = int.Parse(tabTime[1], CultureInfo.InvariantCulture);

            return new TimeSpan(hour, minute, 0);
        }

        public async Task SendCommand()
        {
            var timeZoneId = "Europe/Paris";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);

            var currentDay = currentTime.DayOfWeek;
            var currentHour = currentTime.TimeOfDay ;

            // Search for the appropriate command at the correct time from each plan.
            foreach (var planning in this.planningCommands)
            {
                foreach (var schedule in planning.Commands[DayConverter.Convert(currentDay)])
                {
                    if (schedule.Start < currentHour && schedule.End > currentHour)
                    {
                        await SendDevicesCommand(planning.ListDeviceId, schedule.PayloadId);
                    }
                }
            }
        }

        public async Task SendDevicesCommand(Collection<string> devices, string command)
        {
            foreach (var device in devices) await this.loRaWanCommandService.ExecuteLoRaWanCommand(device, command);
        }
    }
}
