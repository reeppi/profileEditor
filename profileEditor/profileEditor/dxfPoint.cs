using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Data;
using netDxf;
using System.ComponentModel;

namespace profileEditor
{
    public enum PointType
    {
        NONE,
        START,
        END,
        CENTER,
    }

  

    public class dxfPoint 
    {
        public bool Selected { get; set; }
        public bool notDraw { get; set; }
        public bool begin { get; set;  }
        public bool join { get; set;  }
        public bool open { get; set; }
        public bool mark { get; set; }
        public int trackIndex { get; set; }
        public PointType pT { get;  set; }
       
        Point p;

        public dxfPoint(double x, double y, PointType pT_)
        {
            p = new Point(x, y);
            Selected = false;
            pT = pT_;
        }

        public dxfPoint(double x, double y)
        {
            p = new Point(x, y);
            Selected = false;
            pT = PointType.NONE;
        }

        public double X
        {
            get { return p.X; }
            set { p.X = value; }
        }

        public double Y
        {
            get { return p.Y; }
            set { p.Y = value; }
        }

        public Point P
        {
            get { return p; }
        }
    }
}
