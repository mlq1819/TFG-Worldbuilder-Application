using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TFG_Worldbuilder_Application
{
    class Global
    {
        private static bool _hascanvas = false;
        private static bool HasCanvas {
            get
            {
                return _hascanvas;
            }
        }
        private static Canvas _rendercanvas = null;
        public static Canvas RenderCanvas
        {
            get
            {
                if (HasCanvas)
                    return _rendercanvas;
                return null;
            }
            set
            {
                if(value != null)
                {
                    _rendercanvas = value;
                    _hascanvas = true;
                    DefaultCenter = new AbsolutePoint(RenderedCenter.X, RenderedCenter.Y);
                } else
                {
                    _rendercanvas = null;
                    _hascanvas = false;
                }
            }
        }
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
                if (DefaultZoom > 1.0f)
                    return DefaultZoom * 10;
                return DefaultZoom * 5;
            }
        }
        public static double MinZoom
        {
            get
            {
                if (DefaultZoom > 1.0f)
                    return DefaultZoom / 2;
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
                if (!HasCanvas || RenderCanvas == null)
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
        private static AbsolutePoint _center = new AbsolutePoint(0, 0);//Must always be in absolute coordinates
        public static AbsolutePoint DefaultCenter = new AbsolutePoint(0, 0);
        /// <summary>
        /// The point around which zooming will take place; should be recorded in absolute coordinates.
        /// Any objects at this point will be rendered at RenderedCenter
        /// </summary>
        public static AbsolutePoint Center
        {
            get
            {
                return _center;
            }
            set
            {
                _center = value;
            }
        }

        public static SubtypeArchive Subtypes = new SubtypeArchive();

        public static MapPage mappage = null;
    }
}
