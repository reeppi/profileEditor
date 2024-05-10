using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Data;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;

namespace profileEditor
{
    public static class classHelper
    {
        public static double fOffset = 0.01;

        public static double Clamp(this double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static Vector clampVector(this Vector vect, double maxLen)
        {
            Vector newVector = new Vector(vect.X,vect.Y);
            double factor = 1;
            double len = vect.Length;
            if (len > maxLen)
                factor = maxLen / len;
            else
                factor = 1;
            newVector.X *= factor;
            newVector.Y *= factor;
            return newVector;
        }

        public static double distance(this Point p1, Point p2)
        {
            double deltaX = p1.X - p2.X;
            double deltaY = p1.Y - p2.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

       
        public static Vector Vector(this Point p1, Point p2)
        {
            Vector vect = new Vector();
            vect.X = p1.X - p2.X;
            vect.Y = p1.Y - p2.Y;
            return vect;

        }


        public static bool IsConnect(this IdxfObj testValue1, IdxfObj testValue2)
        {
            if (testValue1.end.X.IsBetween(testValue2.start.X - fOffset, testValue2.start.X + fOffset) && testValue1.end.Y.IsBetween(testValue2.start.Y - fOffset, testValue2.start.Y + fOffset))
                return true;
            else
                return false;
        }

        public static bool IsConnect(this dxfPoint testValue1, dxfPoint testValue2)
        {
            return testValue1.X.IsBetween(testValue2.X - fOffset, testValue2.X + fOffset) && testValue1.Y.IsBetween(testValue2.Y - fOffset, testValue2.Y + fOffset);
        }

        public static bool IsConnect(this Point testValue1, Point testValue2)
        {
            return testValue1.X.IsBetween(testValue2.X - fOffset, testValue2.X + fOffset) && testValue1.Y.IsBetween(testValue2.Y - fOffset, testValue2.Y + fOffset);
        }


        public static bool IsBetween(this double testValue, double bound1, double bound2)
        {
            return (testValue >= Math.Min(bound1, bound2) && testValue <= Math.Max(bound1, bound2));
        }

        public static IEnumerable<T> Circle<T>(this IEnumerable<T> list, int startIndex)
        {
            if (list != null)
            {
                List<T> firstList = new List<T>();
                using (var enumerator = list.GetEnumerator())
                {
                    int i = 0;
                    while (enumerator.MoveNext())
                    {
                        if (i < startIndex)
                        {
                            firstList.Add(enumerator.Current);
                            i++;
                        }
                        else
                        {
                            yield return enumerator.Current;
                        }
                    }
                }
                foreach (var first in firstList)
                {
                    yield return first;
                }
            }
            yield break;
        }

        public static dxfPoint getClosestDxfPoint(List<dxfPoint> dxfPointsList, dxfPoint findPoint)
        {
            dxfPoint fPt = null;
            double dist = 2 ^ 16;
            foreach (dxfPoint pt in dxfPointsList)
            {
                double xd = Math.Abs(findPoint.X - pt.X);
                double yd = Math.Abs(findPoint.Y - pt.Y);
                if (Math.Sqrt(xd * xd + yd * yd) < dist)
                {
                    dist = Math.Sqrt(xd * xd + yd * yd);
                    fPt = pt;
                }
            }
           return fPt;
        }

        public static double getDist(classDxf dxf, double dist, double refPointX, double refPointY)
        {
            foreach (IdxfObj dxfObj in dxf.DxfObjects)
            {
                foreach (dxfPoint pt in dxfObj.snapList())
                {
                    double xd = Math.Abs(refPointX - pt.X);
                    double yd = Math.Abs(refPointY - pt.Y);
                    if (Math.Sqrt(xd * xd + yd * yd) < dist)
                        dist = Math.Sqrt(xd * xd + yd * yd);
                }
            }
            return dist;
        }



        public static UserControl FindParentUserControl(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            //CHeck if this is the end of the tree
            if (parent == null) return null;

            UserControl parentControl = parent as UserControl;
            if (parentControl != null)
            {
                return parentControl;
            }
            else
            {
                //use recursion until it reaches a Window
                return FindParentUserControl(parent);
            }
        }

        public static HeaderedContentControl FindParentHeader(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            //CHeck if this is the end of the tree
            if (parent == null) return null;

            HeaderedContentControl parentControl = parent as HeaderedContentControl;
            if (parentControl != null)
            {
                return parentControl;
            }
            else
            {
                //use recursion until it reaches a Window
                return FindParentHeader(parent);
            }
        }





        public static T GetVisualChild<T>(DependencyObject element) where T : DependencyObject
        {
            T child = default(T);
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);

            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject obj = VisualTreeHelper.GetChild(element, i);
                if (obj is T)
                {
                    child = (T)obj;
                    break;
                }
                else
                {
                    child = GetVisualChild<T>(obj);
                    if (child != null)
                        break;
                }
            }

            return child;
        }


        public static double getRadFromAngle(double angle)
        {
            return (3.14159 * 2) / 360 * angle;
        }

        public static double getAngleFromRad(double rad)
        {
            return 360 / (3.14159 * 2) * rad;
        }



        /*
        public static int FindCollisionArcArc(dxfArc arc1, dxfArc arc2, out Point sect)
        {
            Point sect1 = new Point();
            Point sect2 = new Point();
            sect = new Point();
            bool bSect1 = false;
            bool bSect2 = false;
            int count = FindCircleCircleIntersections(arc1.centerPoint.X, arc1.centerPoint.Y, arc1.radius, arc2.centerPoint.X, arc2.centerPoint.Y, arc2.radius, out sect1, out sect2);

            int ct = 0;
            if (count >= 2)
            {
                netDxf.Vector2 vectSect1Arc1 = new netDxf.Vector2(sect1.X - arc1.centerPoint.X, sect1.Y - arc1.centerPoint.Y);
                netDxf.Vector2 vectSect2Arc1 = new netDxf.Vector2(sect2.X - arc1.centerPoint.X, sect2.Y - arc1.centerPoint.Y);
                netDxf.Vector2 vectStartArc1 = new netDxf.Vector2(arc1.start.X - arc1.centerPoint.X, arc1.start.Y - arc1.centerPoint.Y);
                netDxf.Vector2 vectEndArc1 = new netDxf.Vector2(arc1.end.X - arc1.centerPoint.X, arc1.end.Y - arc1.centerPoint.Y);

                netDxf.Vector2 vectSect1Arc2 = new netDxf.Vector2(sect1.X - arc2.centerPoint.X, sect1.Y - arc2.centerPoint.Y);
                netDxf.Vector2 vectSect2Arc2 = new netDxf.Vector2(sect2.X - arc2.centerPoint.X, sect2.Y - arc2.centerPoint.Y);
                netDxf.Vector2 vectStartArc2 = new netDxf.Vector2(arc2.start.X - arc2.centerPoint.X, arc2.start.Y - arc2.centerPoint.Y);
                netDxf.Vector2 vectEndArc2 = new netDxf.Vector2(arc2.end.X - arc2.centerPoint.X, arc2.end.Y - arc2.centerPoint.Y);

                double radWideArc1 = netDxf.Vector2.AngleBetween(vectStartArc1, vectEndArc1);
                double radWideArc2 = netDxf.Vector2.AngleBetween(vectStartArc2, vectEndArc2);

                double radSect1StartArc1 = netDxf.Vector2.AngleBetween(vectStartArc1, vectSect1Arc1);
                double radSect1EndArc1 = netDxf.Vector2.AngleBetween(vectEndArc1, vectSect1Arc1);
                double radSect1StartArc2 = netDxf.Vector2.AngleBetween(vectStartArc2, vectSect1Arc2);
                double radSect1EndArc2 = netDxf.Vector2.AngleBetween(vectEndArc2, vectSect1Arc2);

                double radSect2StartArc1 = netDxf.Vector2.AngleBetween(vectStartArc1, vectSect2Arc1);
                double radSect2EndArc1 = netDxf.Vector2.AngleBetween(vectEndArc1, vectSect2Arc1);
                double radSect2StartArc2 = netDxf.Vector2.AngleBetween(vectStartArc2, vectSect2Arc2);
                double radSect2EndArc2 = netDxf.Vector2.AngleBetween(vectEndArc2, vectSect2Arc2);

                bool sect1Arc1;
                if (!arc1.isLargeArc)
                    sect1Arc1 = ((radSect1StartArc1 + radSect1EndArc1) < radWideArc1);
                else
                    sect1Arc1 = ((radSect1StartArc1 + radSect1EndArc1) > radWideArc1);
                bool sect1Arc2;
                if (!arc2.isLargeArc)
                    sect1Arc2 = ((radSect1StartArc2 + radSect1EndArc2) < radWideArc2);
                else
                    sect1Arc2 = ((radSect1StartArc2 + radSect1EndArc2) > radWideArc2);

                if (sect1Arc1 && sect1Arc2)
                {
                    Console.WriteLine("Collision section 1  X:" + sect1.X + " Y:" + sect1.Y);
                    ct++;
                    sect = sect1;
                    bSect1 = true;
                }

                bool sect2Arc1;
                if (!arc1.isLargeArc)
                    sect2Arc1 = (radSect2StartArc1 < radWideArc1 && radSect2EndArc1 < radWideArc1);
                else
                    sect2Arc1 = (radSect2StartArc1 < radWideArc1 && radSect2EndArc1 > radWideArc1);
                bool sect2Arc2;
                if (!arc2.isLargeArc)
                    sect2Arc2 = (radSect2StartArc2 < radWideArc2 && radSect2EndArc2 < radWideArc2);
                else
                    sect2Arc2 = (radSect2StartArc2 < radWideArc2 && radSect2EndArc2 > radWideArc2);

                if (sect2Arc1 && sect2Arc2)
                {
                    Console.WriteLine("Collision section 2  X:" + sect2.X + " Y:" + sect2.Y);
                    ct++;
                    sect = sect2;
                    bSect2 = true;
                }

                if (bSect1 && bSect2)
                {
                    // if ( arc1.start.P.distance(new Point(sect1.X+) <    )

                }

                //  Console.WriteLine("arc1 large : " + arc1.isLargeArc + "  arc2 large :" + arc2.isLargeArc);



            }

            return ct;

        }
        */

        public static int FindCollisionArcArc(dxfArc arc1, dxfArc arc2, out Point sect)
        {
            Point sect1 = new Point();
            Point sect2 = new Point();
            sect = new Point();
            bool bSect1 = false;
            bool bSect2 = false;
            int count = FindCircleCircleIntersections(arc1.centerPoint.X, arc1.centerPoint.Y, arc1.radius, arc2.centerPoint.X, arc2.centerPoint.Y, arc2.radius, out sect1, out sect2);

            int ct = 0;
            if (count >= 2)
            {
                netDxf.Vector2 vectSect1Arc1 = new netDxf.Vector2(sect1.X - arc1.centerPoint.X, sect1.Y - arc1.centerPoint.Y);
                netDxf.Vector2 vectSect2Arc1 = new netDxf.Vector2(sect2.X - arc1.centerPoint.X, sect2.Y - arc1.centerPoint.Y);
                netDxf.Vector2 vectStartArc1 = new netDxf.Vector2(arc1.start.X - arc1.centerPoint.X, arc1.start.Y - arc1.centerPoint.Y);
                netDxf.Vector2 vectEndArc1 = new netDxf.Vector2(arc1.end.X - arc1.centerPoint.X, arc1.end.Y - arc1.centerPoint.Y);

                netDxf.Vector2 vectSect1Arc2 = new netDxf.Vector2(sect1.X - arc2.centerPoint.X, sect1.Y - arc2.centerPoint.Y);
                netDxf.Vector2 vectSect2Arc2 = new netDxf.Vector2(sect2.X - arc2.centerPoint.X, sect2.Y - arc2.centerPoint.Y);
                netDxf.Vector2 vectStartArc2 = new netDxf.Vector2(arc2.start.X - arc2.centerPoint.X, arc2.start.Y - arc2.centerPoint.Y);
                netDxf.Vector2 vectEndArc2 = new netDxf.Vector2(arc2.end.X - arc2.centerPoint.X, arc2.end.Y - arc2.centerPoint.Y);

                double radWideArc1 = netDxf.Vector2.AngleBetween(vectStartArc1, vectEndArc1);
                double radWideArc2 = netDxf.Vector2.AngleBetween(vectStartArc2, vectEndArc2);

                double radSect1StartArc1 = netDxf.Vector2.AngleBetween(vectStartArc1, vectSect1Arc1);
                double radSect1EndArc1 = netDxf.Vector2.AngleBetween(vectEndArc1, vectSect1Arc1);
                double radSect1StartArc2 = netDxf.Vector2.AngleBetween(vectStartArc2, vectSect1Arc2);
                double radSect1EndArc2 = netDxf.Vector2.AngleBetween(vectEndArc2, vectSect1Arc2);

                double radSect2StartArc1 = netDxf.Vector2.AngleBetween(vectStartArc1, vectSect2Arc1);
                double radSect2EndArc1 = netDxf.Vector2.AngleBetween(vectEndArc1, vectSect2Arc1);
                double radSect2StartArc2 = netDxf.Vector2.AngleBetween(vectStartArc2, vectSect2Arc2);
                double radSect2EndArc2 = netDxf.Vector2.AngleBetween(vectEndArc2, vectSect2Arc2);

                bool sect1Arc1;
                if (!arc1.isLargeArc)
                    sect1Arc1 = (radSect1StartArc1 < radWideArc1 && radSect1EndArc1 < radWideArc1);
                else
                    sect1Arc1 = (radSect1StartArc1 < radWideArc1 || radSect1EndArc1 > radWideArc1);
                bool sect1Arc2;
                if (!arc2.isLargeArc)
                    sect1Arc2 = (radSect1StartArc2 < radWideArc2 && radSect1EndArc2 < radWideArc2);
                else
                    sect1Arc2 = (radSect1StartArc2 > radWideArc2 || radSect1EndArc2 > radWideArc2);

                if (sect1Arc1 && sect1Arc2)
                {
                  //  Console.WriteLine("Collision section 1  X:" + sect1.X + " Y:" + sect1.Y);
                    ct++;
                    sect = sect1;
                    bSect1 = true;
                }

                bool sect2Arc1;
                if (!arc1.isLargeArc)
                    sect2Arc1 = (radSect2StartArc1 < radWideArc1 && radSect2EndArc1 < radWideArc1);
                else
                    sect2Arc1 = (radSect2StartArc1 < radWideArc1 || radSect2EndArc1 > radWideArc1);
                bool sect2Arc2;
                if (!arc2.isLargeArc)
                    sect2Arc2 = (radSect2StartArc2 < radWideArc2 && radSect2EndArc2 < radWideArc2);
                else
                    sect2Arc2 = (radSect2StartArc2 > radWideArc2 || radSect2EndArc2 > radWideArc2);

                if (sect2Arc1 && sect2Arc2)
                {
                   // Console.WriteLine("Collision section 2  X:" + sect2.X + " Y:" + sect2.Y);
                    ct++;
                    sect = sect2;
                    bSect2 = true;
                }

                if (bSect1 && bSect2)
                {
                    // if ( arc1.start.P.distance(new Point(sect1.X+) <    )

                }

                //  Console.WriteLine("arc1 large : " + arc1.isLargeArc + "  arc2 large :" + arc2.isLargeArc);

                /*
                  if ( 
                       arc1.start.P.distance(new Point(sect1.X, sect1.Y)) <
                       arc1.start.P.distance(new Point(sect2.X, sect2.Y))
                      )
                        sect = sect1;
                    else
                        sect = sect2;
                        */
                  


            }

            return ct;

        }





        public static bool FindCollisionLineArc(dxfLine line, dxfArc arc, out Point sect)
        {
            bool col = false;
            Point closest = new Point();
            double dist = FindDistanceToSegment(arc.centerPoint.P, line.start.P, line.end.P, out closest);

            bool col1 = false;
            bool col2 = false;

            double arcStartAngle = 0, arcEndAngle = 0;
            double sectAngle1 = 0, sectAngle2 = 0;

            Point sect1 = new Point();
            Point sect2 = new Point();
            sect = new Point();
            int count = 0;
            int colindex = 0;
            if (dist < arc.radius && ((line.end.P.distance(arc.centerPoint.P) >= arc.radius) || (line.start.P.distance(arc.centerPoint.P) >= arc.radius)))
            {
                // Segmentti on arkin lähettyvillä.
                //Console.WriteLine("TEHDÄÄN TARKEMPI ANALYYSI");
                double rad;
                count = FindLineCircleIntersections(arc.centerPoint.X, arc.centerPoint.Y, arc.radius, line.start.P, line.end.P, out sect1, out sect2);

                rad = netDxf.Vector2.Angle(new netDxf.Vector2(arc.start.X - arc.centerPoint.X, arc.start.Y - arc.centerPoint.Y));
                arcStartAngle = classHelper.getAngleFromRad(rad);
                rad = netDxf.Vector2.Angle(new netDxf.Vector2(arc.end.X - arc.centerPoint.X, arc.end.Y - arc.centerPoint.Y));
                arcEndAngle = classHelper.getAngleFromRad(rad);

                netDxf.Vector2 vectArcStart = new netDxf.Vector2(arc.start.X - arc.centerPoint.X, arc.start.Y - arc.centerPoint.Y);
                netDxf.Vector2 vectArcEnd = new netDxf.Vector2(arc.end.X - arc.centerPoint.X, arc.end.Y - arc.centerPoint.Y);

                rad = netDxf.Vector2.AngleBetween(vectArcStart, vectArcEnd);
                double arcWide = classHelper.getAngleFromRad(rad);
                //Console.WriteLine("angle betweeen start and end: " + arcWide);
                if (count >= 1)
                {
                    netDxf.Vector2 vectSect1 = new netDxf.Vector2(sect1.X - arc.centerPoint.X, sect1.Y - arc.centerPoint.Y);
                    rad = netDxf.Vector2.Angle(vectSect1);
                    sectAngle1 = classHelper.getAngleFromRad(rad);
                   // Console.WriteLine("arcStartAngle :" + arcStartAngle + " arcEndAngle :" + arcEndAngle);
                   // Console.WriteLine("Collision Point 1 X:" + sect1.X + " Y:" + sect1.Y + "ANGLE " + sectAngle1);
                    col1 = false;


                    rad = netDxf.Vector2.AngleBetween(vectArcStart, vectSect1);
                    double arcSect1Start = classHelper.getAngleFromRad(rad);
                    rad = netDxf.Vector2.AngleBetween(vectArcEnd, vectSect1);
                    double arcSect1End= classHelper.getAngleFromRad(rad);

                    if (FindDistanceToSegment(sect1, line.start.P, line.end.P, out closest) < 0.01)
                    {
                        if (!arc.isLargeArc)
                        {
                            if (arcSect1Start < arcWide && arcSect1End < arcWide)
                                col1 = true;
                        }
                        else
                        {
                            if (arcSect1Start > arcWide || arcSect1End > arcWide)
                                col1 = true;
                        }
                    }
                }
                if (count >= 2)
                {
                    netDxf.Vector2 vectSect2 = new netDxf.Vector2(sect2.X - arc.centerPoint.X, sect2.Y - arc.centerPoint.Y);
                    rad = netDxf.Vector2.Angle(vectSect2);
                    sectAngle2 = classHelper.getAngleFromRad(rad);
                   // Console.WriteLine("Collision Point 2 X:" + sect2.X + " Y:" + sect2.Y + " ANGLE " + sectAngle2);
                    col2 = false;

                    rad = netDxf.Vector2.AngleBetween(vectArcStart, vectSect2);
                    double arcSect2Start = classHelper.getAngleFromRad(rad);
                    rad = netDxf.Vector2.AngleBetween(vectArcEnd, vectSect2);
                    double arcSect2End = classHelper.getAngleFromRad(rad);

                    if (FindDistanceToSegment(sect2, line.start.P, line.end.P, out closest) < 0.01)
                    {
                        if (!arc.isLargeArc)
                        {
                            if (arcSect2Start < arcWide && arcSect2End < arcWide)
                                col2 = true;
                        }
                        else
                        {
                            if (arcSect2Start > arcWide || arcSect2End > arcWide)
                                col2 = true;
                        }
                    }

                }

            }

            if (count >= 2)
            {
                if (sect1.distance(line.end.P) < sect2.distance(line.end.P))
                {
                    col = col1;
                    sect = new Point(sect1.X, sect1.Y);
                    colindex = 1;
                    if (!col)
                    {
                        col = col2;
                        sect = new Point(sect2.X, sect2.Y);
                        colindex = 2;
                    }
                }
                else
                {
                    col = col2;
                    sect = new Point(sect2.X, sect2.Y);
                    colindex = 2;
                    if (!col)
                    {
                        col = col1;
                        sect = new Point(sect1.X, sect1.Y);
                        colindex = 1;
                    }
                }

            }
            if (count == 1)
            {
                col = col1;
                sect = new Point(sect1.X, sect1.Y);
            }

            if (col)
            {
                //Console.WriteLine("COLLISION X=" + sect.X + " Y=" + sect.Y + " colIndex : "+colindex);
                return col;
            }
            return col;
        }


        public static double FindDistanceToSegment(Point pt, Point p1, Point p2, out Point closest)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }


        public static void FindIntersection(Point p1, Point p2, Point p3, Point p4, out bool lines_intersect, out bool segments_intersect,out Point intersection,out Point close_p1, out Point close_p2)
        {
            // Get the segments' parameters.
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p4.X - p3.X;
            double dy34 = p4.Y - p3.Y;
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (double.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new Point(double.NaN, double.NaN);
                close_p1 = new Point(double.NaN, double.NaN);
                close_p2 = new Point(double.NaN, double.NaN);
                return;
            }
            lines_intersect = true;

            double t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new Point(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }



        public static int FindLineCircleIntersections(double cx, double cy, double radius, Point point1, Point point2, out Point intersection1, out Point intersection2)
        {
            double dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) +
                (point1.Y - cy) * (point1.Y - cy) -
                radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new Point(Double.NaN, Double.NaN);
                intersection2 = new Point(Double.NaN, Double.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 =
                    new Point(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new Point(double.NaN, double.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (double)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 =
                    new Point(point1.X + t * dx, point1.Y + t * dy);
                t = (double)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 =
                    new Point(point1.X + t * dx, point1.Y + t * dy);
                return 2;
            }
        }


        public static int FindCircleCircleIntersections(double cx0, double cy0, double radius0, double cx1, double cy1, double radius1, out Point intersection1, out Point intersection2)
        {
            // Find the distance between the centers.
            double dx = cx0 - cx1;
            double dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                intersection1 = new Point(float.NaN, float.NaN);
                intersection2 = new Point(float.NaN, float.NaN);
                return 0;
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                intersection1 = new Point(float.NaN, float.NaN);
                intersection2 = new Point(float.NaN, float.NaN);
                return 0;
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                intersection1 = new Point(float.NaN, float.NaN);
                intersection2 = new Point(float.NaN, float.NaN);
                return 0;
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 -
                    radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                // Get the points P3.
                intersection1 = new Point(
                    (double)(cx2 + h * (cy1 - cy0) / dist),
                    (double)(cy2 - h * (cx1 - cx0) / dist));
                intersection2 = new Point(
                    (double)(cx2 - h * (cy1 - cy0) / dist),
                    (double)(cy2 + h * (cx1 - cx0) / dist));

                // See if we have 1 or 2 solutions.
                if (dist == radius0 + radius1) return 1;
                return 2;
            }
        }





    }

}
