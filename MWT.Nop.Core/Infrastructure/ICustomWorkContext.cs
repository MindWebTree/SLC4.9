using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Infrastructure
{
    public interface ICustomWorkContext 
    {

        bool IsMobileDevice();

        bool IsMobileDeviceExcludingIpad();
        int GetDevice(string usrAgent);


    }


}
