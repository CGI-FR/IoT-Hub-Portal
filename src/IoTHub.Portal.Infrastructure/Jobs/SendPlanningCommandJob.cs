// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Jobs
{
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

    [DisallowConcurrentExecution]
    public class SendPlanningCommandJob : IJob
    {
        private readonly IDeviceService<DeviceDetails> deviceService;
        private readonly ILayerService layerService;
        private readonly IPlanningService planningService;
        private readonly IScheduleService scheduleService;
        private readonly ILoRaWANCommandService loRaWANCommandService;

        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly List<PlanningCommand> planningCommands = new List<PlanningCommand>();
        public PaginatedResult<DeviceListItem> devices { get; set; } = new PaginatedResult<DeviceListItem>();
        public IEnumerable<LayerDto> layers { get; set; } = new List<LayerDto>();
        public IEnumerable<PlanningDto> plannings { get; set; } = new List<PlanningDto>();
        public IEnumerable<ScheduleDto> schedules { get; set; } = new List<ScheduleDto>();
        private readonly ILogger<SendPlanningCommandJob> logger;

        public SendPlanningCommandJob(IDeviceService<DeviceDetails> deviceService,
            ILayerService layerService,
            IPlanningService planningService,
            IScheduleService scheduleService,
            ILoRaWANCommandService loRaWANCommandService,
            ILogger<SendPlanningCommandJob> logger)
        {
            this.logger = logger;

            this.deviceService = deviceService;
            this.layerService = layerService;
            this.planningService = planningService;
            this.scheduleService = scheduleService;
            this.loRaWANCommandService = loRaWANCommandService;

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
                await UpdateAPI();
                UpdateDatabase();

                await SendCommand();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Send planning command has failed");
            }
        }

        public async Task UpdateAPI()
        {
            try
            {
                devices = await this.deviceService.GetDevices(pageSize: 10000);
                layers = await this.layerService.GetLayers();
                plannings = await this.planningService.GetPlannings();
                schedules = await this.scheduleService.GetSchedules();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Update API has failed");
            }
        }

        public void UpdateDatabase()
        {
            foreach (var device in this.devices.Data.Where(d => !string.IsNullOrWhiteSpace(d.LayerId)))
            {
                AddNewDevice(device);
            }
        }

        public void AddNewDevice(DeviceListItem device)
        {
            var layer = layers.FirstOrDefault(layer => layer.Id == device.LayerId);

            if (layer?.Planning is not null and not "None")
            {
                // If the layer linked to a device already has a planning, add the device to the planning list
                foreach (var planning in this.planningCommands.Where(planning => planning.planningId == layer.Planning))
                {
                    planning.listDeviceId.Add(device.DeviceID);
                    return;
                }

                // Else create the planning
                var newPlanning = new PlanningCommand(device.DeviceID, layer.Planning);
                AddCommand(newPlanning);
                this.planningCommands.Add(newPlanning);
            }
        }

        public void AddCommand(PlanningCommand planningCommand)
        {
            var planningData = plannings.FirstOrDefault(planning => planning.Id == planningCommand.planningId);

            // If planning is active
            if (planningData != null && IsPlanningActive(planningData))
            {
                // Connect off days command to the planning
                addPlanningSchedule(planningData, planningCommand);


                foreach (var schedule in schedules.Where(s => s.PlanningId == planningCommand.planningId))
                {
                    // Add schedules to the planning
                    addSchedule(schedule, planningCommand);
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
        public void addPlanningSchedule(PlanningDto planningData, PlanningCommand planning)
        {
            if (planningData != null)
            {
                foreach (var key in planning.commands.Keys.Where(k => (planningData.DayOff & k) == planningData.DayOff))
                {
                    var newPayload = new PayloadCommand(getTimeSpan("0:00"), getTimeSpan("24:00"), planningData.CommandId);
                    planning.commands[key].Add(newPayload);
                }
            }
        }

        public void addSchedule(ScheduleDto schedule, PlanningCommand planning)
        {
            // Convert a string into TimeSpan format
            var start = getTimeSpan(schedule.Start);
            var end = getTimeSpan(schedule.End);

            foreach (var key in planning.commands.Keys)
            {
                if (planning.commands[key].Count == 0)
                {
                    var newPayload = new PayloadCommand(start, end, schedule.CommandId);
                    planning.commands[key].Add(newPayload);
                }
                // The if condition is utilized to skip day off schedules.
                else if (planning.commands[key][0].start != getTimeSpan("00:00") || planning.commands[key][0].end != getTimeSpan("24:00"))
                {
                    var newPayload = new PayloadCommand(start, end, schedule.CommandId);
                    planning.commands[key].Add(newPayload);
                }
            }
        }

        public TimeSpan getTimeSpan(string time)
        {
            var tabTime = time != null ? time.Split(':') : ("0:0").Split(':');

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
    }
}
