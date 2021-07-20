using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YQTrack.Backend.OrderComplete.Model;

namespace YQTrack.Backend.OrderCompleteService.Host
{
    public interface ICompleteTask
    {
       void Execute(List<OrderCompleteItem> userDbNos = null);
    }
}
