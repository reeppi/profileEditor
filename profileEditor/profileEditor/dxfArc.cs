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

    public class dxfArc : IdxfObj
    {
        dxfPoint startPoint;
        dxfPoint endPoint;
        public dxfPoint centerPoint { get; private set; }
        public double radius { get; set; }
        public bool isLargeArc { get; set; }
        string typeStr;
        public SweepDirection sweep { get; set; }
        public double angle { get; set; }
        public double toolXOffset { get; set; }
        public double toolYOffset { get; set; }

        public bool join { get; set; }


        public object clone(bool flip)
        {
            dxfArc arcClone = new dxfArc();
            if (!flip)
            {
                arcClone.startPoint = new dxfPoint(startPoint.X, startPoint.Y,PointType.START);
                arcClone.startPoint.begin = startPoint.begin;
                arcClone.endPoint = new dxfPoint(endPoint.X, endPoint.Y, PointType.END);
                arcClone.endPoint.begin = endPoint.begin;
                arcClone.sweep = sweep; 
            }
            else
            {
                arcClone.startPoint = new dxfPoint(endPoint.X, endPoint.Y, PointType.START);
                arcClone.startPoint.begin = endPoint.begin;
                arcClone.endPoint = new dxfPoint(startPoint.X, startPoint.Y, PointType.END);
                arcClone.endPoint.begin = startPoint.begin;
                arcClone.sweep = SweepDirection.Clockwise;
            }
            arcClone.centerPoint = new dxfPoint(centerPoint.X, centerPoint.Y, PointType.CENTER);
            arcClone.radius = radius;
            arcClone.isLargeArc = isLargeArc;
            arcClone.typeStr = "ARC";
       

            return (object)arcClone;
        }


        public void flip()
        {
            
            double tmpX, tmpY;
            tmpX = startPoint.X;
            tmpY = startPoint.Y;
            startPoint.X = endPoint.X;
            startPoint.Y = endPoint.Y;
            endPoint.X = tmpX;
            endPoint.Y = tmpY;
            startPoint.pT = PointType.START;
            endPoint.pT = PointType.END;

            if (sweep == SweepDirection.Counterclockwise)
                sweep = SweepDirection.Clockwise;
             else
               sweep = SweepDirection.Counterclockwise;
 
        }


        public dxfPoint start
        {
            get { return startPoint; }
            set { startPoint = value; }
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

        public List<dxfPoint> snapList()
        {
            return new List<dxfPoint> { startPoint, endPoint, centerPoint };
        }

        public List<dxfPoint> startEndList()
        {
            return new List<dxfPoint> { startPoint, endPoint };
        }

        public void setNewRadius(double newRadius)
        {
            double factor = newRadius /radius;
            double deltaStartX = (startPoint.X - centerPoint.X) * factor;
            double deltaStartY = (startPoint.Y - centerPoint.Y) * factor;
            double deltaEndX = (endPoint.X - centerPoint.X) * factor;
            double deltaEndY = (endPoint.Y - centerPoint.Y) * factor;
            startPoint.X =  centerPoint.X + deltaStartX;
            startPoint.Y = centerPoint.Y + deltaStartY;
            endPoint.X = centerPoint.X + deltaEndX;
            endPoint.Y = centerPoint.Y + deltaEndY;
            radius = newRadius;
        }

        public void setOffsetX(double offsetX)
        {
            startPoint.X += offsetX;
            endPoint.X += offsetX;
            centerPoint.X += offsetX;
        }

        public void setOffsetY(double offsetY)
        {
            startPoint.Y += offsetY;
            endPoint.Y += offsetY;
            centerPoint.Y += offsetY;
        }


        public void invertX()
        {
            startPoint.X = -startPoint.X;
            endPoint.X = -endPoint.X;
            centerPoint.X = -centerPoint.X;
        }

        public dxfArc()
        {

        }

        public dxfArc(dxfPoint startPoint_, dxfPoint endPoint_, dxfPoint centerPoint_, double radius_, bool isLargeArc_)
        {
            startPoint = startPoint_;
            endPoint = endPoint_;
            centerPoint = centerPoint_;
            radius = radius_;
            isLargeArc = isLargeArc_;
            typeStr = "ARC";
            sweep = SweepDirection.Counterclockwise;
        }

        public void draw(StreamGeometryContext ctx, classTranform transform)
        {
            Point startP = transform.PointXY(startPoint.X, startPoint.Y);
            Point endP = transform.PointXY(endPoint.X, endPoint.Y);

            if (radius * transform.Scale >= 0)
            {
                Size ArcSize = new Size(radius * transform.Scale, radius * transform.Scale);
                ctx.BeginFigure(startP, false, false);
                ctx.ArcTo(endP, ArcSize, 360, isLargeArc, sweep, true, false);
            }
        }
    }

}
    
