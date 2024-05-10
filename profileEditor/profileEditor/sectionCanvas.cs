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


namespace profileEditor
{

    class classSectionCanvas : Canvas
    {
        classDxfView dxfView;
        classTranform transform;
        Canvas crossCanvas = null;

        Canvas selectedPointCanvas = null;
        int selectedPointCanvasId;

        double width;
        double height;
        double border;


        public classSectionCanvas(classDxfView dxfView_, classTranform transform_, double width_, double height_, double border_)
        {
            dxfView = dxfView_;
            transform = transform_;
            width = width_;
            height = height_;
            border = border_;
            createSectionCanvas();

            crossCanvas = graph.createCrossCanvas(Colors.Green);
            Canvas.SetLeft(crossCanvas, transform.PosX(0));
            Canvas.SetTop(crossCanvas, transform.PosY(0));
            this.Children.Add(crossCanvas);

            selectedPointCanvas = new Canvas();
            selectedPointCanvasId = this.Children.Add(selectedPointCanvas);

            this.MouseWheel += ClassSectionCanvas_MouseWheel;
            this.MouseMove += ClassSectionCanvas_MouseMove;

        }

        double getDist(classDxf dxf, double dist, double refPointX, double refPointY)
        {
            foreach (IdxfObj dxfObj in dxf.DxfObjects)
            {
                foreach (dxfPoint pt in dxfObj.snapList())
                {
                    double xd = Math.Abs(refPointX - pt.X);
                    double yd = Math.Abs(refPointY - pt.Y);
                    if (Math.Sqrt(xd * xd + yd * yd) < dist)
                    {
                        dist = Math.Sqrt(xd * xd + yd * yd);
                        dxfView.mousePosSnap.X = pt.X;
                        dxfView.mousePosSnap.Y = pt.Y;
                    }
                }
            }
            return dist;
        }
   
        private void ClassSectionCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            classSectionCanvas canvas = (classSectionCanvas)sender;
            double xMouse = Mouse.GetPosition(canvas).X;
            double yMouse = Mouse.GetPosition(canvas).Y;
            double grid = dxfView.grid;
 
            double divX = transform.PosInvX(xMouse) % grid;
            double divY = transform.PosInvY(yMouse) % grid;

            double xMouseInv = transform.PosInvX(xMouse);
            double yMouseInv = transform.PosInvY(yMouse);

            dxfView.mousePosSnap.X = xMouseInv;
            dxfView.mousePosSnap.Y = yMouseInv;

            double distC = 2 ^ 24;
            if (dxfView.checkPointSnap == true)
            {
                foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
                {
                    if (dxfView.checkPathDxf == true)
                        distC = getDist(dxfTrack.dxfPath, distC, xMouseInv, yMouseInv);
                    distC=getDist(dxfTrack.dxf, distC, xMouseInv, yMouseInv);
                }

            }
            if (dxfView.checkGridSnap == true && dxfView.checkPointSnap == false)
            {
                dxfView.mousePosSnap.X = (xMouseInv - divX) + Math.Round(divX / grid) * grid;
                dxfView.mousePosSnap.Y = (yMouseInv - divY) + Math.Round(divY / grid) * grid;
            }
            if (dxfView.checkGridSnap == true && dxfView.checkPointSnap == true)
            {
                double GridPosX = (xMouseInv - divX) + Math.Round(divX / grid) * grid;
                double GridPosY = (yMouseInv - divY) + Math.Round(divY / grid) * grid;
                double xd = Math.Abs(xMouseInv - GridPosX);
                double yd = Math.Abs(yMouseInv - GridPosY);
                double dist = Math.Sqrt(xd * xd + yd * yd);
                if (dist < distC)
                {
                    dxfView.mousePosSnap.X = GridPosX;
                    dxfView.mousePosSnap.Y = GridPosY;
                } 
            }

            dxfView.labelSnapCoord = "X: " + Math.Round(dxfView.mousePosSnap.X,3) + " Y: " + Math.Round(dxfView.mousePosSnap.Y,3);
            Canvas.SetLeft(crossCanvas, transform.PosX(dxfView.mousePosSnap.X));
            Canvas.SetTop(crossCanvas, transform.PosY(dxfView.mousePosSnap.Y));
        }

        private void createSectionCanvas()
        {
            clearCanvas();
            createBackground();

            //UUSI
            foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
            {
                createCanvasFromDxfObjects(dxfTrack.dxf);
                if (dxfView.checkPathDxf == true)
                    createCanvasFromDxfObjects(dxfTrack.dxfPath);
            }

            //createCanvasFromDxfObjects(main.dxf);
            //if (main.checkPathDxf.IsChecked == true)
            //   createCanvasFromDxfObjects(dxfTrack.dxfPath);

            createGrid();
        }

        public void clearCanvas()
        {
            this.Children.Clear();
        }

        
        private void ClassSectionCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                dxfView.scale.Value++;
            else
                dxfView.scale.Value--;
            dxfView.drawCanvas();
        }
        
        public void updateCanvas()
        {
            this.Children.Clear();
            createSectionCanvas();
        }

        private void createGrid()
        {

            double grid = dxfView.grid;
           // double.TryParse(main.textGrid.Text, out grid);
            if (grid <= 0) grid = 1;

            if (dxfView.scale.Value*grid > 8)
            {
                Path gridPath = new Path();
                gridPath.Stroke = Brushes.Black; 
                StreamGeometry geometryP = new StreamGeometry();
                geometryP.FillRule = FillRule.EvenOdd;
                
                using (StreamGeometryContext ctx = geometryP.Open())
                {
                    for (double y = 0; y <= dxfView.dxfFile.edge.Top; y+=grid)
                    {
                        for (double x = 0; x <= dxfView.dxfFile.edge.Right; x+=grid)
                        {
                            Point startP = transform.PointXY(x, y);
                            Point endP = transform.PointXY(x, y);
                            endP.Y += 0.5;
                            ctx.BeginFigure(startP, false, false);
                            ctx.LineTo(endP, true, false);
                        }
                    }
                }
                geometryP.Freeze();
                gridPath.Data = geometryP;
                this.Children.Add(gridPath);
            }


        }

        private void createBackground()
        {
            PathFigure toolPathFigure = new PathFigure();
            double bd = border / 2;
            toolPathFigure.StartPoint = new Point(transform.PosX(-bd), transform.PosY(-bd));
            toolPathFigure.Segments.Add(new LineSegment(transform.PointXY(-bd, height+bd), true));
            toolPathFigure.Segments.Add(new LineSegment(transform.PointXY(width+bd, height+bd), true));
            toolPathFigure.Segments.Add(new LineSegment(transform.PointXY(width+bd, -bd), true));
            toolPathFigure.Segments.Add(new LineSegment(transform.PointXY(-bd, -bd), true));

            PathGeometry toolPathGeometry = new PathGeometry();
            toolPathGeometry.Figures.Add(toolPathFigure);
            Path toolPath = new Path();
            toolPath.Fill = new SolidColorBrush(Colors.White);
            toolPath.Data = toolPathGeometry;
            SolidColorBrush brush = new SolidColorBrush(Colors.Silver);
            toolPath.StrokeDashArray = new DoubleCollection() { transform.Scale, transform.Scale, transform.Scale };
            toolPath.Stroke = brush;
            toolPath.StrokeThickness = 1;
            toolPath.StrokeStartLineCap = PenLineCap.Flat;
            toolPath.StrokeEndLineCap = PenLineCap.Flat;
            toolPath.Data = toolPathGeometry;
            this.Children.Add(toolPath);
        }

        public void createCanvasFromDxfObjects(classDxf dxf)
        {
            if (dxf.DxfObjects == null) return;
           

            /*
            if ( !dxf.path)
                brush = Brushes.Black;
            else
                brush =  new SolidColorBrush(Color.FromArgb(150, 255, 50, 0)); ;*/

          
            StreamGeometry geometryP;

            Path pathN;

            if (!dxf.path)
            {
                pathN = new Path();
                pathN.Stroke = Brushes.Black;

                geometryP = new StreamGeometry();
                geometryP.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometryP.Open())
                {
                    foreach (IdxfObj dxfObj in dxf.DxfObjects)
                        if (!(dxfObj.start.Selected & dxfObj.end.Selected))
                            dxfObj.draw(ctx, transform);
                }
                geometryP.Freeze();
                pathN.Data = geometryP;
                this.Children.Add(pathN);

                pathN = new Path();
                geometryP = new StreamGeometry();
                geometryP.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometryP.Open())
                {
                    foreach (IdxfObj dxfObj in dxf.DxfObjects)
                        if (dxfObj.start.Selected && dxfObj.end.Selected)
                            dxfObj.draw(ctx, transform);
                }
                geometryP.Freeze();
                pathN.Stroke = Brushes.Black;
                pathN.StrokeThickness = 3;
                pathN.Data = geometryP;
                this.Children.Add(pathN);

            }

            if ( dxf.path )
            {
                pathN = new Path();

                pathN.StrokeThickness = dxf.toolDia * transform.Scale;
                pathN.StrokeStartLineCap = PenLineCap.Round;
                pathN.StrokeEndLineCap = PenLineCap.Round;
                pathN.Stroke = new SolidColorBrush(Color.FromArgb(150, 255, 50, 0));

                geometryP = new StreamGeometry();
                geometryP.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometryP.Open())
                {
                    foreach (IdxfObj dxfObj in dxf.DxfObjects)
                        dxfObj.draw(ctx, transform);
                }
                geometryP.Freeze();
                pathN.Data = geometryP;
                this.Children.Add(pathN);

                pathN = new Path();
                pathN.Stroke = Brushes.Gray;
                pathN.StrokeThickness = 1;
        
                geometryP = new StreamGeometry();
                geometryP.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometryP.Open())
                {
                    foreach (IdxfObj dxfObj in dxf.DxfObjects)
                        if (  !(dxfObj.start.Selected & dxfObj.end.Selected))
                            dxfObj.draw(ctx, transform);
                }
                geometryP.Freeze();
                pathN.Data = geometryP;
                this.Children.Add(pathN);

                Path pathSel = new Path();
                pathSel.Stroke = Brushes.DarkRed;
                pathSel.StrokeThickness = 3;
                geometryP = new StreamGeometry();
                geometryP.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometryP.Open())
                {
                    foreach (IdxfObj dxfObj in dxf.DxfObjects)
                    {
                        if ( dxfObj.start.Selected && dxfObj.end.Selected)
                            dxfObj.draw(ctx, transform);
                    }
                }
                geometryP.Freeze();
                pathSel.Data = geometryP;
                this.Children.Add(pathSel);

            }



        }


    }
}
