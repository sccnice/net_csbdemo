using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.demo.csb
{
   public static class SyetemExtend
    {
        public  static long ToUnixTimeMilliseconds(this DateTime nowTime) {

            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));

           // DateTime nowTime = DateTime.Now;

            long unixTime = (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
    }
}
