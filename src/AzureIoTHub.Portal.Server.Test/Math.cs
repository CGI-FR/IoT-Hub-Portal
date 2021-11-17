using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Test
{
    internal class Math
    {

        public int Add(int? a, int b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }


            return a.Value + b;
        }
    }
}
