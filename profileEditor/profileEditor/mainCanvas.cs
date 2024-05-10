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

    class classMainCanvas : Canvas
    {
        classTranform transform;
        Rectangle selRect;
        Point clickPoint;
        int selRectUI;

        Point selRectPos;
        Canvas selectedPointCanvas = null;

        classDxfView dxfView;

        public classMainCanvas(classDxfView dxfView_, classTranform transform_, double width, double height)
        {
            transform = transform_;
            dxfView = dxfView_;

            Canvas.SetLeft(this, transform.OffsetX);
            Canvas.SetTop(this, transform.OffsetY);

            Rectangle bgRect = new Rectangle();
            bgRect.Height = height;
            bgRect.Width =  width;
            bgRect.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0d7db"));
            bgRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0d7db"));
            Canvas.SetLeft(bgRect, -transform.OffsetX);
            Canvas.SetTop(bgRect, -transform.OffsetY);
            this.Children.Add(bgRect);

            selectedPointCanvas = new Canvas();
            this.Children.Add(selectedPointCanvas);
            Canvas.SetZIndex(selectedPointCanvas, 90);

            // Uusi
            foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
            {
                drawSelectedPoints(dxfTrack.dxf);
                if (dxfView.checkPathDxf == true)
                    drawSelectedPoints(dxfTrack.dxfPath);
            }

            drawBeginPoints();

            this.MouseMove += ClassMainCanvas_MouseMove;
            this.MouseUp += ClassMainCanvas_mouseUp;
            this.MouseDown += ClassMainCanvas_MouseDown;

            this.MouseWheel += ClassMainCanvas_MouseWheel;
        }

        private void ClassMainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            dxfView.fit = false;
            double zoom = 0;
            if (e.Delta > 0)
                zoom = 1;
            else
                zoom = -1;
            dxfView.updateZoom(zoom);
        }

        private void drawBeginPoints()
        {
            foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
            {
                foreach ( dxfPoint pt in dxfTrack.dxf.getAlldxfPoints())
                {
                    if ( pt.begin  )
                    {
                        Canvas canvasStartPoint = graph.createFilledCircleCanvas(Colors.Blue, 8);
                        Canvas.SetLeft(canvasStartPoint, transform.PosX(pt.X));
                        Canvas.SetTop(canvasStartPoint, transform.PosY(pt.Y));
                        this.Children.Add(canvasStartPoint);
                        Canvas.SetZIndex(canvasStartPoint, 90);
                        break;
                    }
                }
            }
        }

        private void setSelectionDxf(classDxf dxf)
        {

            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl);
      

            foreach (IdxfObj dxfObj in dxf.DxfObjects)
                {
                    foreach (dxfPoint pt in dxfObj.snapList())
                    {
                    if (pt.X.IsBetween(selRectPos.X, selRectPos.X + selRect.Width / transform.Scale) && pt.Y.IsBetween(selRectPos.Y, selRectPos.Y - selRect.Height / transform.Scale))
                        pt.Selected = true;
                    else
                        if ( !ctrl )
                            pt.Selected = false;
                    }
                }
            }

        private void ClassMainCanvas_mouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selRect != null)
            {
                if (selRect.Width > 0 && selRect.Height > 0)
                {
                    //UUSI
                    foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
                    {
                        setSelectionDxf(dxfTrack.dxf);
                        drawSelectedPoints(dxfTrack.dxf);
                        if (dxfView.checkPathDxf == true)
                        {
                            setSelectionDxf(dxfTrack.dxfPath);
                            drawSelectedPoints(dxfTrack.dxfPath);
                        }     
                    }

                }
                Children.RemoveAt(selRectUI);
                selRect = null;
            }
            dxfView.drawCanvas();
        }

       

        private void selectAllConnectedPoints(classDxf dxf)
        {
            double findOffset = classHelper.fOffset;
            int i = 0;
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl);

            //dxfView.listDxfSelection(false);
            foreach (IdxfObj dxfObj in dxf.DxfObjects)
            {
                foreach (dxfPoint pt in dxfObj.snapList())
                {
                    if (dxfView.mousePosSnap.IsConnect(pt.P))
                    //if (main.mousePosSnap.X.IsBetween(pt.X - findOffset, pt.X + findOffset) && main.mousePosSnap.Y.IsBetween(pt.Y - findOffset, pt.Y + findOffset))
                    {
                        /*
                        if (dxf.path == false)
                        {
                            if (i < main.listdxfObj.Items.Count)
                            {
                           
                                main.listdxfObj.SelectedIndex = i;
                                Console.WriteLine("Löyty vittupää mutta miksi et fokusoidu " + main.listdxfObj.SelectedIndex);
                                 main.listdxfObj.Focus();

                            }
                        }
                        else
                        {
                            if (i < main.listdxfPathObj.Items.Count)
                            {
                                main.listdxfPathObj.SelectedIndex = i;
                               main.listdxfPathObj.Focus();
                                Console.WriteLine("Löyty vittupää mutta miksi et fokusoidu " + main.listdxfPathObj.SelectedIndex);
                            }
                        }*/
                        pt.Selected = true;
                    }
                    else
                    {
                        if ( !ctrl)
                            pt.Selected = false;
                    }
                }
                i++;
            }
            //  dxfView.listDxfSelection(true);
        }

        bool panView = false;

        private void ClassMainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double xMouseInv = transform.PosInvX(Mouse.GetPosition(this).X);
            double yMouseInv = transform.PosInvY(Mouse.GetPosition(this).Y);
            clickPoint = new Point(xMouseInv, yMouseInv);
            selRect = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 1
            };
            Color color = Colors.Silver;
            color.A = 100;
            selRect.Fill = new SolidColorBrush(color);
            selRectUI = this.Children.Add(selRect);


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
                {
                    selectAllConnectedPoints(dxfTrack.dxf);
                    if (dxfView.checkPathDxf == true)
                        selectAllConnectedPoints(dxfTrack.dxfPath);
                }
                int trackIndex = dxfView.dxfFile.getTrackByPoint(new dxfPoint(xMouseInv, yMouseInv));
                //  dxfView.updateTrackSettings(trackIndex);
                if ( trackIndex != dxfView.trackIndex )
                    dxfView.trackIndex = trackIndex;
                updateSelectedPointCanvas();
            }
            
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                dxfView.fit = false;
                panView = true;
                mouseOldPosX = Mouse.GetPosition(dxfView).X;
                mouseOldPosY = Mouse.GetPosition(dxfView).Y;
            }
            if (e.MiddleButton == MouseButtonState.Released)
            {
                panView = false;
            }
        
        }

        

        public void updateSelectedPointCanvas()
        {
            if (selectedPointCanvas != null)
                 selectedPointCanvas.Children.Clear();

            foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
            {
                drawSelectedPoints(dxfTrack.dxf);
                if (dxfView.checkPathDxf == true)
                    drawSelectedPoints(dxfTrack.dxfPath);
            }
        }

        private void drawSelectedPoints(classDxf dxf)
        {
            if (dxf.DxfObjects == null) return;
            foreach (IdxfObj dxfObj in dxf.DxfObjects)
            {
                IEnumerable<dxfPoint> selPoints = from x in dxfObj.snapList() where x.Selected select x;
                if ( dxfObj.end.Selected)
                {
                    Vector vect;
                    netDxf.Vector2 vectorN;

                    double perAngle = 0;
                    /*
                    if (dxfObj.GetType() == typeof(dxfArc))
                    {
                        vect = ((dxfArc)dxfObj).end.P.Vector( ((dxfArc)dxfObj).centerPoint.P);
                        vectorN = netDxf.Vector2.Perpendicular(new netDxf.Vector2(vect.X, vect.Y));
                        double rad = netDxf.Vector2.Angle(vectorN);
                        Console.WriteLine("--- > "+ classHelper.getAngleFromRad(rad));
                        perAngle = classHelper.getAngleFromRad(rad);
                        if (((dxfArc)dxfObj).sweep == SweepDirection.Clockwise)
                            perAngle = 360 - perAngle;
                    }
                    if (dxfObj.GetType() == typeof(dxfLine))
                    {
                        perAngle = ((dxfLine)dxfObj).angle;
                    }*/

                    Canvas arrowCanvas = graph.createCrossCanvas(Colors.Blue);
                    RotateTransform rotateTransform = new RotateTransform(perAngle, 0, 0);
                    arrowCanvas.RenderTransform = rotateTransform;

                    Canvas.SetLeft(arrowCanvas, transform.PosX(dxfObj.end.X));
                    Canvas.SetTop(arrowCanvas, transform.PosY(dxfObj.end.Y));
                    selectedPointCanvas.Children.Add(arrowCanvas);
                }

                if (dxfObj.start.Selected)
                {
                    Canvas circleCanvas = graph.createCircleCanvas(Colors.Blue);
                    Canvas.SetLeft(circleCanvas, transform.PosX(dxfObj.start.X));
                    Canvas.SetTop(circleCanvas, transform.PosY(dxfObj.start.Y));
                    selectedPointCanvas.Children.Add(circleCanvas);
                }
            }
        }

        
        static double mouseOldPosX = 0;
        static double mouseOldPosY = 0;


        private void ClassMainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            classMainCanvas canvas = (classMainCanvas)sender;
            double xMouse = Mouse.GetPosition(canvas).X;
            double yMouse = Mouse.GetPosition(canvas).Y;
            dxfView.mousePos.X = transform.PosInvX(xMouse);
            dxfView.mousePos.Y = transform.PosInvY(yMouse);
            dxfView.labelCoord = "X : " + Math.Round(transform.PosInvX(xMouse), 2) + " Y :" + Math.Round(transform.PosInvY(yMouse), 2);

            double mouseMainX = Mouse.GetPosition(dxfView).X;
            double mouseMainY = Mouse.GetPosition(dxfView).Y;


            if (e.LeftButton == MouseButtonState.Pressed && selRect != null)
            {
                double invXMouse = transform.PosInvX(xMouse);
                double invYMouse = transform.PosInvY(yMouse);
                double xP = Math.Min(invXMouse, clickPoint.X);
               // double xP = Math.Min(invXMouse, clickPoint.X); xFlip
                double yP = Math.Max(invYMouse, clickPoint.Y);
                selRect.Width = Math.Abs(invXMouse - clickPoint.X) * transform.Scale;
                selRect.Height = Math.Abs(invYMouse - clickPoint.Y) * transform.Scale;
                Canvas.SetLeft(selRect, transform.PosX(xP));
                Canvas.SetTop(selRect, transform.PosY(yP));
                selRectPos = new Point(xP, yP);
            }

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                double deltaX = (mouseMainX - mouseOldPosX);
                double deltaY = (mouseMainY - mouseOldPosY);

                dxfView.panX= dxfView.panX+(deltaX/2);
                dxfView.panY= dxfView.panY+(deltaY/2);

                mouseOldPosX = mouseMainX;
                mouseOldPosY = mouseMainY;
                dxfView.drawCanvas();
            }


        }



    }
}
