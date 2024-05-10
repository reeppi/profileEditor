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


namespace profileEditor
{

    public class dxfLine : IdxfObj
    {
        dxfPoint startPoint;
        dxfPoint endPoint;
        string typeStr;
        public bool join { get; set; }
        public double angle { get; set; }

        public double toolXOffset { get; set; }
        public double toolYOffset { get; set; }
        public double factor { get; set; }
        public double k { get; set; }

        public object clone(bool flip)
        {
            dxfLine lineClone = new dxfLine();
            if (!flip)
            {
                lineClone.startPoint = new dxfPoint(startPoint.X, startPoint.Y,PointType.START);
                lineClone.startPoint.begin = startPoint.begin;
                lineClone.endPoint = new dxfPoint(endPoint.X, endPoint.Y, PointType.END);
                lineClone.endPoint.begin = endPoint.begin;
            }
            else
            {
                lineClone.startPoint = new dxfPoint(endPoint.X, endPoint.Y, PointType.START);
                lineClone.startPoint.begin = endPoint.begin;
                lineClone.endPoint = new dxfPoint(startPoint.X, startPoint.Y, PointType.END);
                lineClone.endPoint.begin = startPoint.begin;
            }
            lineClone.typeStr = typeStr;
            return (object)lineClone;
        }

        public void flip()
        {
            dxfPoint startPointTmp = startPoint;
            startPoint = endPoint;
            endPoint = startPointTmp;
        }

        public dxfPoint start
        {
            get { return startPoint; }
            set { startPoint = value;  }
        }

        public dxfPoint end
        {
            get { return endPoint; }
            set { endPoint = value; }
        }

        public string TypeStr
        {
            get { return typeStr; }
        }

        public dxfLine()
        {


        }

        public dxfLine(dxfPoint startPoint_, dxfPoint endPoint_)
        {
            startPoint = startPoint_;
            endPoint = endPoint_;
            typeStr = "LINE";
        }

        public void setOffsetX(double offsetX)
        {
            startPoint.X += offsetX;
            endPoint.X += offsetX;
        }

        public void setOffsetY(double offsetY)
        {
            startPoint.Y += offsetY;
            endPoint.Y += offsetY;
        }



        public void invertX()
        {
            startPoint.X = -startPoint.X;
            endPoint.X = -endPoint.X;
        }

        public List<dxfPoint> snapList()
        {
            return new List<dxfPoint> { startPoint, endPoint };
        }

        public List<dxfPoint> startEndList()
        {
            return new List<dxfPoint> { startPoint, endPoint };

        }

        public void draw(StreamGeometryContext ctx, classTranform transform)
        {
            double X1 = transform.PosX(startPoint.X);
            double Y1 = transform.PosY(startPoint.Y);
            double X2 = transform.PosX(endPoint.X);
            double Y2 = transform.PosY(endPoint.Y);
            ctx.BeginFigure(new Point(X1, Y1), false, false);
            ctx.LineTo(new Point(X2, Y2), true, false);
        }

    }

}
