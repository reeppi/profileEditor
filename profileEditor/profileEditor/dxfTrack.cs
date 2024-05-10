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
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace profileEditor
{
    public class classDxfTrack : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] String caller = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller)); }


        public classDxf dxf { get; set; }
        public classDxf dxfPath { get; set; }

        public classDxfTrack()
        {
            dxf = new classDxf();
            dxfPath = new classDxf();
        }


        bool Side;
        public bool side
        {
            get { return Side; }
            set { Side = value; }
        }

        bool Dir;
        public bool dir {
            get { return Dir; }
            set { Dir = value; }
        }


        public void generatePath()
        {
            double toolDia = dxfPath.toolDia;
            Console.WriteLine("GENERATEPATH() toolDia : " + toolDia);


            //Etsitään pisteet joilla ei ole jatkoa. Merkataan kaikki avoimiksi jonka jälkeen käydään pisteet lävitse.
            foreach (dxfPoint pt in dxfPath.getAlldxfPoints())
                pt.open = true;
            foreach (dxfPoint pt in dxfPath.getAlldxfPoints())
            {
                foreach (dxfPoint ptF in dxfPath.getAlldxfPoints())
                {
                    if (pt.IsConnect(ptF) && (pt != ptF))
                    {
                        pt.open = false;
                        break;
                    }
                }
            }
     
            for (int i = 0; i < dxfPath.DxfObjects.Count(); i++)
            {
                IdxfObj dxfObj = dxfPath.DxfObjects[i];
                double yDelta = dxfObj.end.Y - dxfObj.start.Y;
                double xDelta = dxfObj.end.X - dxfObj.start.X;
                double rad = 0;
                rad = netDxf.Vector2.Angle(new netDxf.Vector2(xDelta, yDelta));
                dxfObj.angle = classHelper.getAngleFromRad(rad);

                if (dxfObj.GetType() == typeof(dxfLine))
                {
                    if (side)
                    {
                        dxfObj.toolXOffset = Math.Sin(rad) * (toolDia / 2);
                        dxfObj.toolYOffset = -Math.Cos(rad) * (toolDia / 2);
                    }
                    else
                    {
                        dxfObj.toolXOffset = -Math.Sin(rad) * (toolDia / 2);
                        dxfObj.toolYOffset = Math.Cos(rad) * (toolDia / 2);
                    }
                    double startXC = dxfObj.start.X + dxfObj.toolXOffset;
                    double startYC = dxfObj.start.Y + dxfObj.toolYOffset;
                    double endXC = dxfObj.end.X + dxfObj.toolXOffset;
                    double endYC = dxfObj.end.Y + dxfObj.toolYOffset;

                    dxfObj.start.X = startXC;
                    dxfObj.start.Y = startYC;

                    dxfObj.end.X = endXC;
                    dxfObj.end.Y = endYC;

                    Vector vect = new Vector(dxfObj.end.X - dxfObj.start.X, dxfObj.end.Y - dxfObj.start.Y);

                    if ( vect.Length < 0.02 )
                    {
                        dxfPath.DxfObjects.RemoveAt(i);
                        i--;
                    }

                }

                if (dxfObj.GetType() == typeof(dxfArc))
                {
                    dxfArc curArc = ((dxfArc)dxfObj);
                    if (side == false)
                    {
                        if (curArc.sweep == SweepDirection.Counterclockwise)
                            curArc.setNewRadius(curArc.radius - toolDia / 2);
                        else
                            curArc.setNewRadius(curArc.radius + toolDia / 2);
                    }
                    else
                    {
                        if (curArc.sweep == SweepDirection.Counterclockwise)
                            curArc.setNewRadius(curArc.radius + toolDia / 2);
                        else
                            curArc.setNewRadius(curArc.radius - toolDia / 2);
                    }

                   
                    /*
                    if (curArc.isLargeArc)
                    {
                        double radStart = netDxf.Vector2.Angle(new netDxf.Vector2(curArc.start.X - curArc.centerPoint.X, curArc.start.Y - curArc.centerPoint.Y));
                        double radEnd = netDxf.Vector2.Angle(new netDxf.Vector2(curArc.end.X - curArc.centerPoint.X, curArc.end.Y - curArc.centerPoint.Y));
                        double startAngle = classHelper.getAngleFromRad(radStart);
                        double endAngle = classHelper.getAngleFromRad(radEnd);

                        double sweep = 0;
                        if (endAngle < startAngle)
                            sweep = (360 + endAngle) - startAngle;
                        else
                            sweep = Math.Abs(endAngle - startAngle);

                        double splitAngle = 0;
                        //  Console.WriteLine("start angle : " + startAngle);
                        //  Console.WriteLine("end angle   : " + endAngle);
                        //  Console.WriteLine("sweeep      : " + sweep/2);

                        if (curArc.sweep == SweepDirection.Counterclockwise)
                        {
                            double sp = (startAngle + sweep / 2);
                            if (sp >= 360)
                                splitAngle = sp - 360;
                            else
                                splitAngle = sp;
                        } else
                        {
                            double sp = (startAngle - sweep / 2);
                            if (sp <= 0 )
                                splitAngle = 360-sp - startAngle;
                            else
                                splitAngle = sp;
                        }

                        dxfArc arc1 = classDxf.createArc(startAngle, splitAngle, curArc.centerPoint.X, curArc.centerPoint.Y, curArc.radius);
                        arc1.isLargeArc = false;
                        arc1.sweep = curArc.sweep;
                        dxfArc arc2 = classDxf.createArc(splitAngle, endAngle, curArc.centerPoint.X, curArc.centerPoint.Y, curArc.radius);
                        arc2.isLargeArc = false;
                        arc2.sweep = curArc.sweep;
                        dxfPath.DxfObjects.RemoveAt(i);
                        dxfPath.DxfObjects.Insert(i, arc1);
                        i++;
                        dxfPath.DxfObjects.Insert(i, arc2);
                    }
                    */
                    
                    if (curArc.radius <= 0)
                    {
                        dxfPath.DxfObjects.RemoveAt(i);
                        i--;
                    }
                }
            }


            for (int i = 0; i < dxfPath.DxfObjects.Count(); i++)
            {
                IdxfObj curDxfObj = dxfPath.DxfObjects[i];
                IdxfObj nextDxfObj = null;

                if (i == (dxfPath.DxfObjects.Count() - 1))
                    nextDxfObj = dxfPath.DxfObjects[0];
                else
                    nextDxfObj = dxfPath.DxfObjects[i + 1];

                if ((curDxfObj.GetType() == typeof(dxfLine)) && (nextDxfObj.GetType() == typeof(dxfLine)))
                {
                    if (curDxfObj.end.IsConnect(nextDxfObj.start)) continue;
                    Point sect = new Point();
                    bool linesIntersect = false;
                    bool segmentsIntersect = false;
                    Point closeP1 = new Point();
                    Point closeP2 = new Point();
                    classHelper.FindIntersection(curDxfObj.start.P, curDxfObj.end.P, nextDxfObj.start.P, nextDxfObj.end.P, out linesIntersect, out segmentsIntersect, out sect, out closeP1, out closeP2);

                    Double deltaEndX = sect.X - curDxfObj.end.X;
                    Double deltaEndY = sect.Y - curDxfObj.end.Y;
                    Double deltaStartX = sect.X - nextDxfObj.start.X;
                    Double deltaStartY = sect.Y - nextDxfObj.start.Y;

                    double endAbs = Math.Sqrt(deltaEndX * deltaEndX + deltaEndY * deltaEndY);
                    double startAbs = Math.Sqrt(deltaStartX * deltaStartX + deltaStartY * deltaStartY);

                    Vector endVector = new Vector(sect.X - curDxfObj.end.X, sect.Y - curDxfObj.end.Y);
                    Vector startVector = new Vector(sect.X - nextDxfObj.start.X, sect.Y - nextDxfObj.start.Y);

                    endVector = endVector.clampVector(toolDia / 2);
                    startVector = startVector.clampVector(toolDia / 2);

                    if (!curDxfObj.end.open)
                    {
                        curDxfObj.end.X += endVector.X;
                        curDxfObj.end.Y += endVector.Y;
                    }
                    if (!nextDxfObj.start.open)
                    {
                        nextDxfObj.start.X += startVector.X;
                        nextDxfObj.start.Y += startVector.Y;
                    }

                    classHelper.FindIntersection(curDxfObj.start.P, curDxfObj.end.P, nextDxfObj.start.P, nextDxfObj.end.P, out linesIntersect, out segmentsIntersect, out sect, out closeP1, out closeP2);
                    endVector = new Vector(sect.X - curDxfObj.end.X, sect.Y - curDxfObj.end.Y);
                    startVector = new Vector(sect.X - nextDxfObj.start.X, sect.Y - nextDxfObj.start.Y);
                    
                    if (segmentsIntersect)
                    {
                        curDxfObj.end.X += endVector.X;
                        curDxfObj.end.Y += endVector.Y;
                        nextDxfObj.start.X += startVector.X;
                        nextDxfObj.start.Y += startVector.Y;
                    }


                    if (!curDxfObj.IsConnect(nextDxfObj) && (!curDxfObj.end.open))
                    {
                        
                        IdxfObj connectDxf = (IdxfObj)curDxfObj.clone(false);
                        connectDxf.start.X = curDxfObj.end.X;
                        connectDxf.start.Y = curDxfObj.end.Y;
                        connectDxf.end.X = nextDxfObj.start.X;
                        connectDxf.end.Y = nextDxfObj.start.Y;
                        dxfPath.DxfObjects.Insert(i + 1, connectDxf);
                        i++;
                    }
                }

                dxfLine line = null;
                dxfArc arc = null;

                bool lineArcConnection = false;
                if (curDxfObj.GetType() == typeof(dxfLine) && nextDxfObj.GetType() == typeof(dxfArc))
                {
                    line = (dxfLine)curDxfObj;
                    arc = (dxfArc)nextDxfObj;
                    lineArcConnection = true;
                }
                if (curDxfObj.GetType() == typeof(dxfArc) && nextDxfObj.GetType() == typeof(dxfLine))
                {
                    line = (dxfLine)nextDxfObj;
                    arc = (dxfArc)curDxfObj;
                    lineArcConnection = true;
                }

                // PERUS
                
                if ( lineArcConnection )
                {
                    Point sect = new Point();
                    if ( classHelper.FindCollisionLineArc(line, arc, out sect))
                    {
                        curDxfObj.end.X = sect.X;
                        curDxfObj.end.Y = sect.Y;
                        nextDxfObj.start.X = sect.X;
                        nextDxfObj.start.Y = sect.Y;
                    } 
                }
                
                /*
                if (lineArcConnection)
                {
                    Point sectPoint1 = new Point();
                    Point sectPoint2 = new Point();
                    int count = classHelper.FindLineCircleIntersections(arc.centerPoint.X, arc.centerPoint.Y, arc.radius, line.start.P, line.end.P, out sectPoint1, out sectPoint2);
                    if (count == 2)
                    {
                        if (sectPoint1.distance(curDxfObj.end.P) < sectPoint2.distance(curDxfObj.end.P))
                        {
                            curDxfObj.end.X = sectPoint1.X;
                            curDxfObj.end.Y = sectPoint1.Y;
                            nextDxfObj.start.X = sectPoint1.X;
                            nextDxfObj.start.Y = sectPoint1.Y;
                        }
                        else
                        {
                            curDxfObj.end.X = sectPoint2.X;
                            curDxfObj.end.Y = sectPoint2.Y;
                            nextDxfObj.start.X = sectPoint2.X;
                            nextDxfObj.start.Y = sectPoint2.Y;
                        }
                    }
                    if (count == 1)
                    {
                        curDxfObj.end.X = sectPoint1.X;
                        curDxfObj.end.Y = sectPoint1.Y;
                        nextDxfObj.start.X = sectPoint1.X;
                        nextDxfObj.start.Y = sectPoint1.Y;
                    }
                }*/
                
            }


            //MEGA ETSINTÄ
             megaSearch();
            megaSearch();
            removeAndConnect(toolDia);

    


            }


        private void removeAndConnect(double toolDia)
        {
            //Poistetaan viivat jotka rikkovat muotoja!!!
            for (int i = 0; i < dxfPath.DxfObjects.Count(); i++)
            {
                IdxfObj curDxfPathObj = dxfPath.DxfObjects[i];

                IdxfObj nextDxfObj = null;
                if (i < (dxfPath.DxfObjects.Count() - 1))
                    nextDxfObj = dxfPath.DxfObjects[i + 1];
                IdxfObj prevDxfObj = null;
                if (i > 0)
                    prevDxfObj = dxfPath.DxfObjects[i - 1];

                if (!(curDxfPathObj.GetType() == typeof(dxfLine))) continue;

                for (int ix = 0; ix < dxf.DxfObjects.Count(); ix++)
                {
                    IdxfObj findDxfObj = dxf.DxfObjects[ix];
                    Point foundPoint;
                    if (!(findDxfObj.GetType() == typeof(dxfLine))) continue;

   
                    double dy = curDxfPathObj.end.Y - curDxfPathObj.start.Y;
                    double dx = curDxfPathObj.end.X - curDxfPathObj.start.X;

                    /*
                    netDxf.Vector2 vectFindObj = new netDxf.Vector2(findDxfObj.end.X - findDxfObj.start.X, findDxfObj.end.Y - findDxfObj.start.Y);
                    netDxf.Vector2 vectCurDxfPathObj = new netDxf.Vector2(curDxfPathObj.end.X - curDxfPathObj.start.X, curDxfPathObj.end.Y - curDxfPathObj.start.Y);
                    double angleBetween = netDxf.Vector2.AngleBetween(vectFindObj, vectCurDxfPathObj);*/

                    if (prevDxfObj != null && prevDxfObj.GetType() == typeof(dxfArc))
                    {
                        double distStart = classHelper.FindDistanceToSegment(curDxfPathObj.start.P, findDxfObj.start.P, findDxfObj.end.P, out foundPoint);
                        if (distStart < (toolDia / 2 - 0.001))
                        {
                            //double db = classHelper.FindDistanceToSegment(curDxfPathObj.end.P, findDxfObj.start.P, findDxfObj.end.P, out foundPoint);
                            // bool linesIntersect = false, segmentsInterect = false;
                            // Point sect = new Point();
                            // Point p1 = new Point();
                            // Point p2 = new Point();
                            // classHelper.FindIntersection(curDxfPathObj.start.P, curDxfPathObj.end.P, nextDxfObj.start.P, nextDxfObj.end.P, out linesIntersect, out segmentsInterect,out sect, out p1, out p2);

                            /*
                            double dist = (toolDia/2 - distStart);
                            double remY = dist * Math.Cos(angleBetween);
                            double remX = -dist * Math.Sin(angleBetween);
                            Console.WriteLine("I: "+i +  "IX: "+ix+" -> remX : " + remX + " remY : " + remY + " angleBetween : "+ classHelper.getAngleFromRad(angleBetween));

                            curDxfPathObj.start.X += remX;
                            curDxfPathObj.start.Y += remY;*/

                            dxfPath.DxfObjects.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                    if (nextDxfObj != null && nextDxfObj.GetType() == typeof(dxfArc))
                    {
                        double distEnd = classHelper.FindDistanceToSegment(curDxfPathObj.end.P, findDxfObj.start.P, findDxfObj.end.P, out foundPoint);
                        if (distEnd < (toolDia / 2 - 0.001))
                        {
                            //Console.WriteLine(ix + " -> tuhotaan " + i + " : distEnd : " + distEnd);
                            /*  double dist = (toolDia / 2 - distEnd);
                              double remY = dist * Math.Cos(angleBetween);
                              double remX = -dist * Math.Sin(angleBetween);
                              Console.WriteLine("I: " + i + "IX: " + ix + " -> remX : " + remX + " remY : " + remY + " angleBetween : " + classHelper.getAngleFromRad(angleBetween));
                              curDxfPathObj.end.X += remX;
                              curDxfPathObj.end.Y += remY;*/

                            dxfPath.DxfObjects.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                }
            }

            //Poistetaan 0 pituiset vektorit
            
            
            for (int i = 0; i < dxfPath.DxfObjects.Count(); i++)
            {
                IdxfObj curDxfObj = dxfPath.DxfObjects[i];

                if ( curDxfObj.end.X.Equals(curDxfObj.start.X)  && curDxfObj.end.Y.Equals(curDxfObj.start.Y) )
                {
                    Console.WriteLine("NOLLA PITUINEN " + i);
                    dxfPath.DxfObjects.RemoveAt(i);
                    i--;
                }
            }

            
            for (int i = 0; i < dxfPath.DxfObjects.Count() - 1; i++)
            {
                IdxfObj nextDxfObj = null;
                IdxfObj curDxfObj = dxfPath.DxfObjects[i];
                if (i < (dxfPath.DxfObjects.Count() - 1))
                    nextDxfObj = dxfPath.DxfObjects[i + 1];

                dxfArc arc = null;
                dxfArc arc2 = null;
                dxfLine line = null;
                bool lineArc = false;
                bool arcArc = false;

                if (nextDxfObj.GetType() == typeof(dxfArc) && curDxfObj.GetType() == typeof(dxfLine))
                {
                    lineArc = true;
                    arc = (dxfArc)nextDxfObj;
                    line = (dxfLine)curDxfObj;
                }

                if (nextDxfObj.GetType() == typeof(dxfLine) && curDxfObj.GetType() == typeof(dxfArc))
                {
                    lineArc = true;
                    arc = (dxfArc)curDxfObj;
                    line = (dxfLine)nextDxfObj;
                }

                if (nextDxfObj.GetType() == typeof(dxfArc) && curDxfObj.GetType() == typeof(dxfArc))
                {
                    arcArc = true;
                    arc = (dxfArc)curDxfObj;
                    arc2 = (dxfArc)nextDxfObj;
                }

                if (lineArc)
                {
                    Point sectPoint1 = new Point();
                    Point sectPoint2 = new Point();
                    int count = classHelper.FindLineCircleIntersections(arc.centerPoint.X, arc.centerPoint.Y, arc.radius, line.start.P, line.end.P, out sectPoint1, out sectPoint2);
                    if (count == 2)
                    {
                        if (sectPoint1.distance(curDxfObj.end.P) < sectPoint2.distance(curDxfObj.end.P))
                        {
                            curDxfObj.end.X = sectPoint1.X;
                            curDxfObj.end.Y = sectPoint1.Y;
                            nextDxfObj.start.X = sectPoint1.X;
                            nextDxfObj.start.Y = sectPoint1.Y;
                        }
                        else
                        {
                            curDxfObj.end.X = sectPoint2.X;
                            curDxfObj.end.Y = sectPoint2.Y;
                            nextDxfObj.start.X = sectPoint2.X;
                            nextDxfObj.start.Y = sectPoint2.Y;
                        }
                    }
                    if (count == 1)
                    {
                        curDxfObj.end.X = sectPoint1.X;
                        curDxfObj.end.Y = sectPoint1.Y;
                        nextDxfObj.start.X = sectPoint1.X;
                        nextDxfObj.start.Y = sectPoint1.Y;
                    }
                }
                if (arcArc && !arc.IsConnect(arc2))
                {
                    Point sectPoint1 = new Point();
                    Point sectPoint2 = new Point();
                    int count = classHelper.FindCircleCircleIntersections(arc.centerPoint.X, arc.centerPoint.Y, arc.radius, arc2.centerPoint.X, arc2.centerPoint.Y, arc.radius, out sectPoint1, out sectPoint2);
                    if (count == 2)
                    {
                        if (sectPoint1.distance(curDxfObj.end.P) < sectPoint2.distance(curDxfObj.end.P))
                        {
                            curDxfObj.end.X = sectPoint1.X;
                            curDxfObj.end.Y = sectPoint1.Y;
                            nextDxfObj.start.X = sectPoint1.X;
                            nextDxfObj.start.Y = sectPoint1.Y;
                        }
                        else
                        {
                            curDxfObj.end.X = sectPoint2.X;
                            curDxfObj.end.Y = sectPoint2.Y;
                            nextDxfObj.start.X = sectPoint2.X;
                            nextDxfObj.start.Y = sectPoint2.Y;
                        }
                    }
                }
              
            }



        }


        private void megaSearch()
        {
            for (int i = 0; i < dxfPath.DxfObjects.Count(); i++)
            {
                IdxfObj curDxfObj = dxfPath.DxfObjects[i];
                int removeCount = 0;
                bool lineArc = false;
                bool ArcArc = false;
                for (int ix = i + 1; ix < dxfPath.DxfObjects.Count(); ix++)
                {
                    
                    Point sect = new Point();
                    bool linesIntersect = false;
                    bool segmentsIntersect = false;

                    lineArc = false;
                    ArcArc = false;
                    dxfArc arc = null;
                    dxfArc arc2 = null;
                    dxfLine line = null;

                    Point closeP1 = new Point();
                    Point closeP2 = new Point();

                    IdxfObj cmpDxfObj = dxfPath.DxfObjects[ix];

                    if (cmpDxfObj == curDxfObj) continue;

                    if (curDxfObj.GetType() == typeof(dxfLine) && cmpDxfObj.GetType() == typeof(dxfLine))
                    {
                        if (!curDxfObj.end.IsConnect(cmpDxfObj.start) && !curDxfObj.start.IsConnect(cmpDxfObj.end) )
                        {
                            classHelper.FindIntersection(curDxfObj.start.P, curDxfObj.end.P, cmpDxfObj.start.P, cmpDxfObj.end.P, out linesIntersect, out segmentsIntersect, out sect, out closeP1, out closeP2);

                            if (segmentsIntersect)
                            {
                                curDxfObj.end.X = sect.X;
                                curDxfObj.end.Y = sect.Y;
                                cmpDxfObj.start.X = sect.X;
                                cmpDxfObj.start.Y = sect.Y;
                                dxfPath.DxfObjects.RemoveRange(i + 1, removeCount);
                                Console.WriteLine("RemoveIndex : " + (i + 1) + "  RemoveCount:" + removeCount);
                                break;
                            }
                        }
                    }
                    if (curDxfObj.GetType() == typeof(dxfLine) && cmpDxfObj.GetType() == typeof(dxfArc))
                    {
                        line = (dxfLine)curDxfObj;
                        arc = (dxfArc)cmpDxfObj;
                        lineArc = true;
                    }
                    if (curDxfObj.GetType() == typeof(dxfArc) && cmpDxfObj.GetType() == typeof(dxfLine))
                    {
                        line = (dxfLine)cmpDxfObj;
                        arc = (dxfArc)curDxfObj;
                        lineArc = true;
                    }

                    if (curDxfObj.GetType() == typeof(dxfArc) && cmpDxfObj.GetType() == typeof(dxfArc))
                    {
                        arc2 = (dxfArc)cmpDxfObj;
                        arc = (dxfArc)curDxfObj;
                        ArcArc = true;
                    }
                    if (!curDxfObj.end.IsConnect(cmpDxfObj.start) && lineArc)
                    {
                        sect = new Point();
                        bool col = classHelper.FindCollisionLineArc(line, arc, out sect);
                        if (col)
                        {
                            curDxfObj.end.X = sect.X;
                            curDxfObj.end.Y = sect.Y;
                            cmpDxfObj.start.X = sect.X;
                            cmpDxfObj.start.Y = sect.Y;
                            dxfPath.DxfObjects.RemoveRange(i + 1, removeCount);
                            break;
                        }
                    }
                    if (!curDxfObj.end.IsConnect(cmpDxfObj.start) && !curDxfObj.start.IsConnect(cmpDxfObj.end) && ArcArc)
                    {
                        sect = new Point();
                        if (classHelper.FindCollisionArcArc(arc, arc2, out sect) >= 1)
                        {
                            curDxfObj.end.X = sect.X;
                            curDxfObj.end.Y = sect.Y;
                            cmpDxfObj.start.X = sect.X;
                            cmpDxfObj.start.Y = sect.Y;
                            dxfPath.DxfObjects.RemoveRange(i + 1, removeCount);
                            break;
                        }
                    }
                    removeCount++;
                }
            }

        }





    }
}
