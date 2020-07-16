using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG_Worldbuilder_Application
{
    class Global
    {
        public static FileManager ActiveFile;
        public static double Zoom = 0;
        public static AbsolutePoint CanvasSize = new AbsolutePoint(0, 0); //Must always be in absolute coordinates
        //public static AbsolutePoint OriginalCenter = new AbsolutePoint(0, 0);
        public static AbsolutePoint OriginalCenter
        {
            get
            {
                return CanvasSize / 2;
            }
        }
        public static AbsolutePoint Center = new AbsolutePoint(0, 0); //Must always be in absolute coordinates
    }
}
