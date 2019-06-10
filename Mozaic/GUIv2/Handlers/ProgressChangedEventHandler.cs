using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIv2.Handlers
{
    public class ProgressChangedEventHandler : EventArgs
    {
        public int Progress { get; private set; }
        public ProgressChangedEventHandler(int progress)
        {
            Progress = progress;
        }
    }
}
