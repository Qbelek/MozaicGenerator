using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIv2.Handlers
{
    public class BitmapLoadedEventHandler : EventArgs
    {
        public Bitmap BMP { get; private set; }

        public BitmapLoadedEventHandler(Bitmap bmp)
        {
            BMP = bmp;
        }
    }
}
