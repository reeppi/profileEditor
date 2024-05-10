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
    public class classDxfFile
    {
        DxfDocument doc;
        private List<IdxfObj> dxfObjectsDXF;
        public List<classDxfTrack> trackList { get;  set; }
        public classEdge edge { get; private set; }
        public double toolDia { get; set; }

        public classDxfFile(string filePath, double toolDia_)
        {
            toolDia = toolDia_;
            if (filePath == "")
            {
                dxfObjectsDXF = null;
                edge = new classEdge();
                return;
            }
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Missing file : " + filePath);
                return;
            }
            doc = new DxfDocument();
            doc = DxfDocument.Load(filePath);
            if (doc != null)
            {
                createDxfObjects(filePath);
                createTracks(true);
                calculatePathTracks();
                Console.WriteLine("Loaded dxf " + filePath);
            }
        }


        public List<dxfPoint> getAllDxfPointsFromDxfTrackList()
        {
            List<dxfPoint> dxfPointList = new List<dxfPoint>();
            foreach (classDxfTrack dxfTrack in trackList)
                foreach (dxfPoint pt in dxfTrack.dxf.getAlldxfPoints())
                    dxfPointList.Add(pt);

            return dxfPointList;
        }

        public int getTrackByPoint(dxfPoint pt)
        {
            int trackIndex = 0;
            //Asetetaan trackIndex jokaiseen pisteeseen
            foreach (classDxfTrack dxfTrack in trackList)
            {
                foreach (dxfPoint dxfPt in dxfTrack.dxf.getAlldxfPoints())
                    dxfPt.trackIndex = trackIndex;
                trackIndex++;
            }
            List<dxfPoint> allDxfPoints = getAllDxfPointsFromDxfTrackList();
            dxfPoint ptFound = classHelper.getClosestDxfPoint(allDxfPoints, pt);

            if (ptFound != null)
            {
                trackIndex = ptFound.trackIndex;
            } else
            {
                trackIndex = 0;
            }

            return trackIndex;
        }



        public void generatePathTrack()
        {
            createTracks(false);
            calculatePathTracks();
        }

        private void calculatePathTracks()
        {
            // INTERPOLOINTI
            foreach (classDxfTrack dxfTrack in trackList)
                dxfTrack.generatePath();

        }

        public class trackdata
        {
            public bool dir;
            public bool side;
            public trackdata(bool dir_, bool side_)
            {
                dir = dir_;
                side = side_;
            }
        }


        private void createTracks(bool fromNewDxf)
        {
            List<IdxfObj> dxfObjects = null;
            List<trackdata> trackData = new List<trackdata>();
            if (fromNewDxf)
            {
                dxfObjects = dxfObjectsDXF;
            }
            else
            {
                if (trackList != null)
                {
                    dxfObjects = new List<IdxfObj>();
                    foreach (classDxfTrack dxfTrackEntry in trackList)
                    {
                        trackData.Add(new trackdata(dxfTrackEntry.dir, dxfTrackEntry.side));
                        foreach (IdxfObj dxfObj in dxfTrackEntry.dxf.DxfObjects)
                            dxfObjects.Add(dxfObj);
                    }
                }

            }
            foreach (IdxfObj dxfObj in dxfObjects)
                dxfObj.join = false;

            bool found = false;
            bool flip = false;

            int index = 0;

            trackList = new List<classDxfTrack>();

            begin:
            IEnumerable<IdxfObj> dxfObjectsNotJoined = (from dxfObj in dxfObjects where dxfObj.@join == false select dxfObj).ToList();
            if (dxfObjectsNotJoined.Count() == 0)
            {
                return;
            }

            classDxfTrack dxfTrack = new classDxfTrack();
            dxfObjectsNotJoined.First().join = true;

            IdxfObj newObj = (IdxfObj)dxfObjectsNotJoined.First().clone(false);
            dxfTrack.dxf.DxfObjects.Add(newObj);
            IdxfObj newPathObj = (IdxfObj)dxfObjectsNotJoined.First().clone(false);
            dxfTrack.dxfPath.DxfObjects.Add(newPathObj);
            dxfTrack.dxfPath.path = true;
            dxfTrack.dxfPath.toolDia = toolDia;

            if (!fromNewDxf && index < trackData.Count())
            {
                dxfTrack.dir = trackData[index].dir;
                dxfTrack.side = trackData[index].side;
            }

            trackList.Add(dxfTrack);

            start:
            foreach (IdxfObj dxfObj in dxfObjectsNotJoined)
            {
                if (!dxfObj.join)
                {
                    found = false;
                    flip = false;
                    if (newObj.end.IsConnect(dxfObj.start))
                        found = true;
                    if (newObj.end.IsConnect(dxfObj.end))
                    {
                        flip = true;
                        found = true;
                    }
                    if (found)
                    {
                        dxfObj.join = true;

                        newObj = (IdxfObj)dxfObj.clone(flip);
                        dxfTrack.dxf.DxfObjects.Add(newObj);
                        newPathObj = (IdxfObj)dxfObj.clone(flip);
                        dxfTrack.dxfPath.DxfObjects.Add(newPathObj);

                        goto start;
                    }
                }
            }

            // Etsitään lisää
            newObj = dxfTrack.dxf.DxfObjects[0];
            start2:
            foreach (IdxfObj dxfObj in dxfObjectsNotJoined)
            {
                if (!dxfObj.join)
                {
                    found = false;
                    flip = false;
                    if (newObj.start.IsConnect(dxfObj.end))
                        found = true;
                    if (newObj.start.IsConnect(dxfObj.start))
                    {
                        flip = true;
                        found = true;
                    }
                    if (found)
                    {
                        dxfObj.join = true;
                        newObj = (IdxfObj)dxfObj.clone(flip);
                        dxfTrack.dxf.DxfObjects.Insert(0, newObj);
                        newPathObj = (IdxfObj)dxfObj.clone(flip);
                        dxfTrack.dxfPath.DxfObjects.Insert(0, newPathObj);
                        goto start2;
                    }
                }
            }
            setOrder(dxfTrack);
            //removeLargeArcs(dxfTrack.dxf);
            // removeLargeArcs(dxfTrack.dxfPath);

            index++;
            goto begin;
        }

        private void removeLargeArcs(classDxf dxf)
        {

            for (int i = 0; i < dxf.DxfObjects.Count(); i++)
            {
                IdxfObj dxfObj = dxf.DxfObjects[i];

                if (dxfObj.GetType() == typeof(dxfArc))
                {
                    dxfArc curArc = (dxfArc)dxfObj;
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

                        }
                        else
                        {
                            double sp = (startAngle - sweep / 2);
                            if (sp <= 0)
                                splitAngle = 360 + sp;
                            else
                                splitAngle = sp;
                        }

                        dxfArc arc1 = classDxf.createArc(startAngle, splitAngle, curArc.centerPoint.X, curArc.centerPoint.Y, curArc.radius);
                        arc1.isLargeArc = false;
                        arc1.sweep = curArc.sweep;
                        dxfArc arc2 = classDxf.createArc(splitAngle, endAngle, curArc.centerPoint.X, curArc.centerPoint.Y, curArc.radius);
                        arc2.isLargeArc = false;
                        arc2.sweep = curArc.sweep;
                        dxf.DxfObjects.RemoveAt(i);
                        dxf.DxfObjects.Insert(i, arc1);
                        i++;
                        dxf.DxfObjects.Insert(i, arc2);
                    }
                }
            }
        }

        private void setOrder(classDxfTrack track)
        {
            bool reverseObjects = false;
            // Etsitään merkattu aloituspiste dxf-objekteista ja asetetaan se aloituspisteeksi.
            dxfPoint beginPt = null;
            foreach (dxfPoint pt in track.dxf.getAlldxfPoints())
            {
                if (pt.begin)
                {
                    beginPt = new dxfPoint(pt.X, pt.Y);
                    break;
                }
            }
            // Jos aloituspistettä ei oltu määritelty valitaan ensimmäinen piste dxf.
            if (beginPt == null)
            {
                beginPt = new dxfPoint(track.dxf.DxfObjects[0].start.X, track.dxf.DxfObjects[0].start.Y);
                foreach (dxfPoint pt in track.dxf.getAlldxfPoints())
                {
                    if (pt.IsConnect(beginPt))
                    {
                        pt.begin = true;
                        break;
                    }
                }
            }

            // Selvitetään dxf-tiedostosta millä indexillä  alkupiste on jotta voidaan tehdään alkupisteen valinta
            int indexBegin = 0;
            if (beginPt != null)
            {
                foreach (IdxfObj dxfObj in track.dxf.DxfObjects)
                {
                    if (beginPt.IsConnect(dxfObj.start))
                        goto endloop;
                    else if (beginPt.IsConnect(dxfObj.end))
                    {
                        if (indexBegin + 1 < track.dxf.DxfObjects.Count())
                            indexBegin++;
                        else
                            reverseObjects = true;
                        goto endloop;
                    }
                    indexBegin++;
                }
            }
            endloop:

            if (!reverseObjects)
            {
                track.dxfPath.setBeginPoint(indexBegin);
                track.dxf.setBeginPoint(indexBegin);
            }

            if (track.dir)
            {
                track.dxfPath.reverseOrder();
                foreach (IdxfObj dxfObj in track.dxfPath.DxfObjects)
                    dxfObj.flip();

                /*
                track.dxf.reverseOrder();
                foreach (IdxfObj dxfObj in track.dxf.DxfObjects)
                    dxfObj.flip();
               */
            }

        }



        dxfArc createArc(double startAngle, double endAngle, double centerX, double centerY, double radius)
        {
            double startAngleRad = classHelper.getRadFromAngle(startAngle);
            double endAngleRad = classHelper.getRadFromAngle(endAngle);
            double startX = Math.Cos(startAngleRad) * radius;
            double startY = Math.Sin(startAngleRad) * radius;
            double endX = Math.Cos(endAngleRad) * radius;
            double endY = Math.Sin(endAngleRad) * radius;

            bool isLargeArc = false;
            double sweep1 = 0.0;
            if (endAngle < startAngle)
                sweep1 = (360 + endAngle) - startAngle;
            else
                sweep1 = Math.Abs(endAngle - startAngle);
            if (sweep1 >= 180) isLargeArc = true;

            dxfPoint startP = new dxfPoint((centerX + startX), centerY + startY, PointType.START);
            dxfPoint endP = new dxfPoint((centerX + endX), centerY + endY, PointType.END);
            dxfPoint centerP = new dxfPoint(centerX, centerY, PointType.CENTER);
            dxfArc arc1 = new dxfArc(startP, endP, centerP, radius, isLargeArc);

            return arc1;
        }



        private void createDxfObjects(string file)
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


            dxfObjectsDXF = new List<IdxfObj>();

            foreach (netDxf.Entities.Circle circle in doc.Circles)
            {
                dxfArc arc1 = createArc(0, 90, (circle.Center.X - xMove), (circle.Center.Y - yMove), circle.Radius);
                dxfObjectsDXF.Add(arc1);
                arc1 = createArc(90, 180, (circle.Center.X - xMove), (circle.Center.Y - yMove), circle.Radius);
                dxfObjectsDXF.Add(arc1);
                arc1 = createArc(180, 270, (circle.Center.X - xMove), (circle.Center.Y - yMove), circle.Radius);
                dxfObjectsDXF.Add(arc1);
                arc1 = createArc(270, 360, (circle.Center.X - xMove), (circle.Center.Y - yMove), circle.Radius);
                dxfObjectsDXF.Add(arc1);
            }

            foreach (netDxf.Entities.Arc ark in doc.Arcs)
            {
                dxfArc arc1 = createArc(ark.StartAngle, ark.EndAngle, (ark.Center.X - xMove), (ark.Center.Y - yMove), ark.Radius);
                dxfObjectsDXF.Add(arc1);
            }

            foreach (netDxf.Entities.Line lineEntry in doc.Lines)
            {
                Vector2 startPoint = new Vector2(lineEntry.StartPoint.X - xMove, lineEntry.StartPoint.Y - yMove);
                Vector2 endPoint = new Vector2(lineEntry.EndPoint.X - xMove, lineEntry.EndPoint.Y - yMove);
                netDxf.Entities.Line lineEntryC = new netDxf.Entities.Line(startPoint, endPoint);
                dxfPoint startP = new dxfPoint(lineEntryC.StartPoint.X, lineEntryC.StartPoint.Y, PointType.START);
                dxfPoint endP = new dxfPoint(lineEntryC.EndPoint.X, lineEntryC.EndPoint.Y, PointType.END);
                dxfLine line = new dxfLine(startP, endP);
                dxfObjectsDXF.Add(line);
            }

            double rightMax = 0;
            double topMax = 0;
            foreach (IdxfObj dxfObj in dxfObjectsDXF)
            {
                double xCmp = Math.Max(dxfObj.start.X, dxfObj.end.X);
                double yCmp = Math.Max(dxfObj.start.Y, dxfObj.end.Y);
                if (xCmp > rightMax)
                    rightMax = xCmp;
                if (yCmp > topMax)
                    topMax = yCmp;
            }
            edge = new classEdge(Math.Abs(rightMax), Math.Abs(rightMax), Math.Abs(topMax), 0);
        }


        public void calculateEdgevaluesFromTrackList()
        {
            double rightMax = 0;
            double topMax = 0;


            List<IdxfObj> dxfObjectsTmp = new List<IdxfObj>();
            List<IdxfObj> dxfPathObjectsTmp = new List<IdxfObj>();
            foreach (classDxfTrack dxfTrackEntry in trackList)
            {
                foreach (IdxfObj dxfObj in dxfTrackEntry.dxf.DxfObjects)
                    dxfObjectsTmp.Add(dxfObj);
                foreach (IdxfObj dxfObj in dxfTrackEntry.dxfPath.DxfObjects)
                    dxfPathObjectsTmp.Add(dxfObj);
            }

            double xMinStart = dxfObjectsTmp.Min(e => e.start.X);
            double xMinEnd = dxfObjectsTmp.Min(e => e.end.X);
            double xOffset = Math.Min(xMinStart, xMinEnd);

            double yMinStart = dxfObjectsTmp.Min(e => e.start.Y);
            double yMinEnd = dxfObjectsTmp.Min(e => e.end.Y);
            double yOffset = Math.Min(yMinStart, yMinEnd);


            foreach (IdxfObj dxfObj in dxfObjectsTmp)
            {
                dxfObj.setOffsetX(-xOffset);
                dxfObj.setOffsetY(-yOffset);
            }

            foreach (IdxfObj dxfObj in dxfPathObjectsTmp)
            {
                dxfObj.setOffsetX(-xOffset);
                dxfObj.setOffsetY(-yOffset);
            }


            foreach (classDxfTrack track in trackList)
            {
                foreach (IdxfObj dxfObj in track.dxf.DxfObjects)
                {
                    double xCmp = Math.Max(dxfObj.start.X, dxfObj.end.X);
                    double yCmp = Math.Max(dxfObj.start.Y, dxfObj.end.Y);
                    if (xCmp > rightMax)
                        rightMax = xCmp;
                    if (yCmp > topMax)
                        topMax = yCmp;
                }
            }
            edge.Right = rightMax;
            edge.Top = topMax;

            //     this.NotifyPropertyChanged("Right");

            Console.WriteLine("edge.Top  : " + edge.Top);
            Console.WriteLine("edge.Right : " + edge.Right);


        }
    

        

    }



   
}
