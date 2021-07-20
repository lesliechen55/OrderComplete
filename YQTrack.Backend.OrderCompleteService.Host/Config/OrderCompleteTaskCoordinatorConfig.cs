using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQTrack.Backend.OrderCompleteService.Host.Config
{
    public class OrderCompleteTaskCoordinatorConfig
    {
        [Category("Schedule-Seller-OrderCompleteService")]
        public YQTrack.Schedule.Config ScheduleConfig { set; get; }
    }
}
