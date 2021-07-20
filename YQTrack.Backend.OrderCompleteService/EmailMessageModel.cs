using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQTrack.Backend.Message.Model.Models;

namespace YQTrack.Backend.OrderCompleteService
{
    public class EmailMessageModel : MessageModel
    {
        public Backend.Enums.EnumChannel EmailChannel
        {
            set
            {
                TransmitDataJson = value.ToString();
            }
        }
    }
}
