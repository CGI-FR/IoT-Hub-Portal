// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SensorModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string AppEUI { get; set; }

        public string Image { get; set; }

        public List<SensorCommand> Commands { get; set; }

        public SensorModel()
        {
            this.Commands = new List<SensorCommand>();
        }
    }
}
