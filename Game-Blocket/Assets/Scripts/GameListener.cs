using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    /// <summary>
    /// Listener
    /// </summary>
    public class GameListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            throw new NotImplementedException();
        }
    }
}
