using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Device
{
    public static class Devices
    {

        public static List<string> MobileDeviceList
        {
            get
            {
                return new List<string>()
                {
                    "acs-",
"alav",
"alca",
"amoi",
"audi",
"avan",
"benq",
"bird",
"blac",
"blaz",
"brew",
"cell",
"cldc",
"cmd-",
"dang",
"doco",
"eric",
"hipt",
"inno",
"ipaq",
"java",
"jigs",
"kddi",
"keji",
"leno",
"lg-c",
"lg-d",
"lge-",
"maui",
"maxo",
"midp",
"mini",
"mits",
"mmef",
"mmp",
"mobi",
"mot-",
"moto",
"mwbp",
"nec-",
"newt",
"noki",
"palm",
"pana",
"pant",
"pda",
"phil",
"phone",
"play",
"port",
"prox",
"qwap",
"sage",
"sams",
"sany",
"sch-",
"sec-",
"send",
"seri",
"sgh-",
"shar",
"sie-",
"siem",
"smal",
"smar",
"smartphone",
"sony",
"sph-",
"symb",
"t-mo",
"teli",
"tim-",
"tsm-",
"up.browser",
"up.link",
"upg1",
"upsi",
"vk-v",
"voda",
"wap",
"wap-",
"wapa",
"wapi",
"wapp",
"wapr",
"webc",
"windows ce",
"winw",
"xda",
"xda-",
"iphone",
"blackberry",
"google nexus 7"
};
            }
        }
        public static List<string> IpadDeviceList
        {
            get
            {
                return new List<string>()
                {
                     "ipad",
                     "sm-t",
                    "sm-t827r4",
                    "sm-t550",
                    "kfthwi",
                    "sm-t580",
                    "tablet","google nexus 7 2013","google nexus 7"
};
            }
        }
        public static List<string> desktopDevices
        {
            get
            {
                return new List<string>()
                {
            "sony xperia tablet s",
            "motorola xoom","sony xperia tablet z"
                };
            }
        }
    }
}
