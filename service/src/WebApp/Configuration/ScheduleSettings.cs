using Medusa.Enap.Common.Infrastructure.Scheduling;

namespace Medusa.Enap.DemoService.WebApp.Configuration;

public class ScheduleSettings : ScheduleSettingsBase
{
    public required string DemoCron { get; set; }
}
