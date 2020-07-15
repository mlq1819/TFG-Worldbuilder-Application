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
        public static Point2D OriginalCenter = new Point2D(0, 0);
        public static Point2D Shift = new Point2D(0, 0);
        public static Point2D Center = new Point2D(0, 0);
    }
}
