// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server.Controllers.V10.LoRaWAN
{
    using System.Collections.Generic;
    using IoTHub.Portal.Server.Filters;
    using IoTHub.Portal.Shared.Models.v10.LoRaWAN;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/lorawan/freqencyplans")]
    [ApiExplorerSettings(GroupName = "LoRa WAN")]
    [LoRaFeatureActiveFilter]
    public class LoRaWANFrequencyPlansController : ControllerBase
    {
        /// <summary>
        /// Get LoRaWAN supported frequency plans.
        /// </summary>
        [HttpGet(Name = "GET LoRaWAN Frequency plans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public ActionResult<IEnumerable<FrequencyPlan>> GetFrequencyPlans()
        {
            return this.Ok(new[] {
                new FrequencyPlan { FrequencyPlanID = "AS_923_925_1",       Name = "Asia 923-925 MHz, Group 1"},
                new FrequencyPlan { FrequencyPlanID = "AS_923_925_2",       Name = "Asia 923-925 MHz, Group 2"},
                new FrequencyPlan { FrequencyPlanID = "AS_923_925_3",       Name = "Asia 923-925 MHz, Group 3"},
                new FrequencyPlan { FrequencyPlanID = "EU_863_870",         Name = "Europe 863-870 MHz"},
                new FrequencyPlan { FrequencyPlanID = "CN_470_510_RP1",     Name = "China 470-510 MHz, RP 1"},
                new FrequencyPlan { FrequencyPlanID = "CN_470_510_RP2",     Name = "China 470-510 MHz, RP 2"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_1",   Name = "United States 902-928 MHz, FSB 1"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_2",   Name = "United States 902-928 MHz, FSB 2"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_3",   Name = "United States 902-928 MHz, FSB 3"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_4",   Name = "United States 902-928 MHz, FSB 4"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_5",   Name = "United States 902-928 MHz, FSB 5"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_6",   Name = "United States 902-928 MHz, FSB 6"},
                new FrequencyPlan { FrequencyPlanID = "US_902_928_FSB_7",   Name = "United States 902-928 MHz, FSB 7"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_1",   Name = "Australia 915-928 MHz, FSB 1"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_2",   Name = "Australia 915-928 MHz, FSB 2"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_3",   Name = "Australia 915-928 MHz, FSB 3"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_4",   Name = "Australia 915-928 MHz, FSB 4"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_5",   Name = "Australia 915-928 MHz, FSB 5"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_6",   Name = "Australia 915-928 MHz, FSB 6"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_7",   Name = "Australia 915-928 MHz, FSB 7"},
                new FrequencyPlan { FrequencyPlanID = "AU_915_928_FSB_8",   Name = "Australia 915-928 MHz, FSB 8"}
            });
        }
    }
}
