using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TFG_Worldbuilder_Application
{
    class Global
    {
        public static Canvas RenderCanvas = null;
        public static FileManager ActiveFile;
        public static double Zoom = 1;
        private static double _defaultzoom = 1.0f;
        public static double DefaultZoom {
            get
            {
                return _defaultzoom;
            }
            set
            {
                _defaultzoom = value;
                Zoom = Math.Max(Math.Min(Zoom, MaxZoom), MinZoom);
            }
        }
        public static double MaxZoom
        {
            get
            {
                return DefaultZoom * 5;
            }
        }
        public static double MinZoom
        {
            get
            {
                return DefaultZoom / 5;
            }
        }
        /// <summary>
        /// RenderedPoint that is defined by the Size of the WorldCanvas
        /// </summary>
        public static RenderedPoint CanvasSize
        {
            get
            {
                if (RenderCanvas == null)
                    return new RenderedPoint(0, 0);
                return new RenderedPoint((long)(RenderCanvas.ActualWidth), (long)(RenderCanvas.ActualHeight));
            }
        }
        /// <summary>
        /// The centerpoint of the rendered canvas, primarily used to aid in correctly transforming and reverting AbsolutePoints and RenderedPoints
        /// </summary>
        public static RenderedPoint RenderedCenter
        {
            get
            {
                return CanvasSize / 2;
            }
        }//Again, always in Rendered coordinates
        /// <summary>
        /// The point around which zooming will take place; should be recorded in absolute coordinates.
        /// Any objects at this point will be rendered at RenderedCenter
        /// </summary>
        public static AbsolutePoint Center = new AbsolutePoint(0, 0); //Must always be in absolute coordinates
    }
}
