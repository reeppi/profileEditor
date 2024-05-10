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
    public interface IdxfObj
    {
        List<dxfPoint> snapList();
        List<dxfPoint> startEndList();
        dxfPoint start { get; set; }
        dxfPoint end { get; set;  }
        object clone(bool flip);
        void flip();
        bool join { get; set;  }
        string TypeStr { get;  }
        double angle { get; set; }
        double toolXOffset { get; set;  }
        double toolYOffset { get; set; }
        void setOffsetX(double offsetX);
        void setOffsetY(double offsetY);

        void invertX();
        void draw(StreamGeometryContext ctx, classTranform transform);
    }

    public class classDxf
    {
        DxfDocument doc;
        private List<IdxfObj> dxfObjects;
        public List<dxfPoint> SelectedDxfPoints { get; private set; }
        public int SelectedIndex { get; set; }
        public int SelectedSnapIndex { get; set; }
        public bool Selected { get; set; }
        public static IniFile setINI;

        public int index { get; set; }

        public bool path { get; set; }

        //Työstöratoja varten
          public double toolDia { get; set; }
      //  public bool side { get; set; }
      //  public bool dir { get; set; }

        //MainWindow main;
        classEdge edge;

        // List<line> lines;

        public List<dxfPoint> getAlldxfPoints()
        {
            List<dxfPoint> dxfP = new List<dxfPoint>();
            foreach (IdxfObj dxfObj in DxfObjects)
            {
                foreach (dxfPoint pt in dxfObj.startEndList())
                {
                    dxfP.Add(pt);
                }
            }
             return dxfP;
        }

        public void setBeginPoint(int i)
        {
            dxfObjects = dxfObjects.Circle(i).ToList();
        }

        public void reverseOrder()
        {
            dxfObjects.Reverse();
        }


        public DxfDocument Doc
        {
            get { return doc; }
        }

        public List<IdxfObj> DxfObjects
        {
            get { return dxfObjects; }
        }

        public classEdge Edge
        {
            get { return edge; }
        }

        public classDxf()
        {
            dxfObjects = new List<IdxfObj>();
        }

        public classDxf(double width, double height)
        {
           // main = main_;
            //selPointList = new List<dxfSelPoint>();
            doc = null;
            dxfObjects = new List<IdxfObj>();
            edge = new classEdge(width, 0, height, 0);

            /*
            dxfPoint startP = new dxfPoint(0, 0);
            dxfPoint endP = new dxfPoint(20, 50);
            dxfLine line = new dxfLine(startP, endP);
            dxfObjects.Add(line);*/

        }

    
        public classDxf(string filePath)
        {
            //main = main_;
            if (filePath == "")
            {
                dxfObjects = null;
                edge = new classEdge();
                return;
            }
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Missing file : " + filePath);
                return;
            }
            Selected = false;
            doc = new DxfDocument();
            doc = DxfDocument.Load(filePath);
            //generateLines();

            if (doc != null)
            {
                createDxfObjects(filePath);
                Console.WriteLine("Loaded dxf " + filePath);
            }
            updateDxfSelectedPoints();

        }

        public void movePoints(double xOffset, double yOffset)
        {
            foreach (IdxfObj dxfObj in DxfObjects)
            {
                foreach (dxfPoint DxfPt in dxfObj.snapList())
                {
                    if (DxfPt.Selected)
                    {
                        DxfPt.X += xOffset;
                        DxfPt.Y += yOffset;
                    } 
                }
            }
        }

        public void selectStartAndEndPointByDxfObj(System.Collections.IList ListDxfObj)
        {
            foreach (IdxfObj dxfObj in DxfObjects)
                foreach (dxfPoint DxfPt in dxfObj.snapList())
                    DxfPt.Selected = false;

            foreach (IdxfObj dxfObj in ListDxfObj)
            {
                dxfObj.start.Selected = true;
                dxfObj.end.Selected = true;
            }
        }

        public void selectStartAndEndPointByIndex(List<int> indexes)
        {

            foreach (IdxfObj dxfObj in DxfObjects)
                foreach (dxfPoint DxfPt in dxfObj.snapList())
                    DxfPt.Selected = false;
            foreach ( int i in indexes)
            {
                if (i < dxfObjects.Count && i >= 0)
                {
                    dxfObjects[i].start.Selected = true;
                    dxfObjects[i].end.Selected = true;
                }
            }
        }

        public void selectEndPointByIndex(int i)
        {
            foreach (IdxfObj dxfObj in DxfObjects)
                foreach (dxfPoint DxfPt in dxfObj.snapList())
                    DxfPt.Selected = false;

            if (i < dxfObjects.Count && i >=0 )
                dxfObjects[i].end.Selected = true;
        }

        public static dxfArc createArc(double startAngle, double endAngle, double centerX, double centerY, double radius)
        {
 
            double startAngleRad = classHelper.getRadFromAngle(startAngle);
            double endAngleRad = classHelper.getRadFromAngle(endAngle);
            double startX = Math.Cos(startAngleRad) * radius;
            double startY = Math.Sin(startAngleRad) * radius;
            double endX = Math.Cos(endAngleRad) * radius;
            double endY = Math.Sin(endAngleRad) * radius;

            bool isLargeArc = false;
            double sweep = 0.0;
            if (endAngle < startAngle)
                sweep = (360 + endAngle) - startAngle;
            else
                sweep = Math.Abs(endAngle - startAngle);
            if (sweep >= 180) isLargeArc = true;

            dxfPoint startP = new dxfPoint((centerX + startX), centerY + startY, PointType.START);
            dxfPoint endP = new dxfPoint((centerX + endX), centerY + endY, PointType.END);
            dxfPoint centerP = new dxfPoint(centerX, centerY, PointType.CENTER);
            dxfArc arc1 = new dxfArc(startP, endP, centerP, radius, isLargeArc);
            return arc1;
        }

        public void createDxfObjects(string file)
        {
            double xMinCenter = 0;
            double yMinCenter = 0;

            if (doc.Arcs.Count > 0)
            {
                xMinCenter = doc.Arcs.Min(e => e.Center.X);
                yMinCenter = doc.Arcs.Min(e => e.Center.Y);
            }

            double xMinStart = doc.Lines.Min(e => e.StartPoint.X);
            double xMinEnd = doc.Lines.Min(e => e.EndPoint.X);
            double xOffset = Math.Min(xMinStart, xMinEnd);

            double yMinStart = doc.Lines.Min(e => e.StartPoint.Y);
            double yMinEnd = doc.Lines.Min(e => e.EndPoint.Y);
            double yOffset = Math.Min(yMinStart, yMinEnd);

            double xMove = 0;
            double yMove = 0;

            if (doc.Arcs.Count > 0)
            {
                xMove = Math.Min(xOffset, xMinCenter);
                yMove = Math.Min(yOffset, yMinCenter);
            }
            else
            {
                xMove = xOffset;
                yMove = yOffset;
            }

            double left = 0;
            double top = 0;

            dxfObjects = new List<IdxfObj>();

            foreach (netDxf.Entities.Circle circle in doc.Circles)
            {
                dxfArc arc1=createArc(0, 90, (circle.Center.X - xMove), (circle.Center.Y - yMove),  circle.Radius);
                dxfObjects.Add(arc1);
                Console.WriteLine("CIRCLE lisätty");
            }
            foreach (netDxf.Entities.Arc ark in doc.Arcs)
            {
                dxfArc arc = createArc(ark.StartAngle, ark.EndAngle, (ark.Center.X - xMove), (ark.Center.Y - yMove), ark.Radius);
                dxfObjects.Add(arc);
            }

            foreach (netDxf.Entities.Line lineEntry in doc.Lines)
            {
                Vector2 startPoint = new Vector2(lineEntry.StartPoint.X - xMove, lineEntry.StartPoint.Y - yMove);
                Vector2 endPoint = new Vector2(lineEntry.EndPoint.X - xMove, lineEntry.EndPoint.Y - yMove);
                netDxf.Entities.Line lineEntryC = new netDxf.Entities.Line(startPoint, endPoint);

                if (lineEntryC.StartPoint.X > left) left = lineEntryC.StartPoint.X;
                if (lineEntryC.EndPoint.X > left) left = lineEntryC.EndPoint.X;

                if (lineEntryC.StartPoint.Y > top) top = lineEntryC.StartPoint.Y;
                if (lineEntryC.EndPoint.Y > top) top = lineEntryC.EndPoint.Y;

                dxfPoint startP = new dxfPoint(lineEntryC.StartPoint.X, lineEntryC.StartPoint.Y,PointType.START);
                dxfPoint endP = new dxfPoint(lineEntryC.EndPoint.X, lineEntryC.EndPoint.Y, PointType.END);
                dxfLine line = new dxfLine(startP, endP);
                dxfObjects.Add(line);
            }

        //    dxfArc arcTest = createArc(0, 90, 5, 5, 5);
          //  dxfObjects.Add(arcTest);

            Console.WriteLine("dxfObjectsCount : "+ dxfObjects.Count());

            Console.WriteLine("Left:" + Math.Abs(left));
            Console.WriteLine("top:" + Math.Abs(top));

            double xMoveOffset = 0;
            /* string dxfFilePathName = main.settings.FilesPath + "\\" + main.settings.ProfilesDir + "\\settings.ini";
            setINI = new IniFile(dxfFilePathName);
            double xMoveOffset = 0;
            string profile = Path.GetFileNameWithoutExtension(file);
            string xoffsetStr = setINI.IniReadValue(profile, "xoffset");
            double.TryParse(xoffsetStr, out xMoveOffset);
            foreach (IdxfObj dxfObj in DxfObjects)
                dxfObj.setOffsetX(xMoveOffset);*/

            foreach (IdxfObj dxfObj in DxfObjects)
                dxfObj.setOffsetX(Math.Abs(left));


            edge = new classEdge(Math.Abs(left) - xMoveOffset, xMoveOffset, Math.Abs(top), 0);
        }

        public void updateDxfSelectedPoints()
        {
            SelectedDxfPoints = new List<dxfPoint>();
            foreach (IdxfObj dxfObj in DxfObjects)
            {
                foreach (dxfPoint DxfPt in dxfObj.snapList())
                {
                    if (DxfPt.Selected)
                        SelectedDxfPoints.Add(DxfPt);
                }
            }
            addEdgePointsToSelectedPoints();
        }

        public void addEdgePointsToSelectedPoints()
        {
            dxfPoint edgePointHigh = new dxfPoint(-edge.Left, edge.Top);
            edgePointHigh.notDraw = true;
            SelectedDxfPoints.Add(edgePointHigh);

            dxfPoint lowPoint = new dxfPoint(0, 0);
            lowPoint.notDraw = true;
            SelectedDxfPoints.Add(lowPoint);

            dxfPoint centerPoint = new dxfPoint(-edge.Left / 2, edge.Top / 2);
            SelectedDxfPoints.Add(centerPoint);

        }

        public void clearDxfSelectedPoints()
        {
            SelectedDxfPoints = new List<dxfPoint>();
            foreach (IdxfObj dxfObj in DxfObjects)
            {
                foreach (dxfPoint DxfPt in dxfObj.snapList())
                    DxfPt.Selected = false;
            }

            addEdgePointsToSelectedPoints();
        }

        /*
        private void generateLines()
        {
            lines = new List<line>();
            double x1d = 0, y1d = 0, x2d = 0, y2d = 0;
            foreach (netDxf.Entities.Arc arc in doc.Arcs)
            {
                int count = 10;
                foreach (Vector2 vector in arc.PolygonalVertexes(count))
                {
                    if (count == 10)
                    {
                        x1d = arc.Center.X + vector.X;
                        y1d = arc.Center.Y + vector.Y;
                        count--;
                        continue;
                    }
                    if (count < 0) continue;
                    x2d = arc.Center.X + vector.X;
                    y2d = arc.Center.Y + vector.Y;

                    lines.Add(new line(x1d, y1d, x2d, y2d));

                    y1d = y2d;
                    x1d = x2d;
                    count--;
                }
            }
            foreach (netDxf.Entities.Line line in doc.Lines)
                lines.Add(new line(line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y));
        }*/

    }


 


}
