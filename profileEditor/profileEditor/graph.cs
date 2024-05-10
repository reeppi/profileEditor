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
    public static class graph
    {

        public static Canvas createArrowCanvas(Color crossColor)
        {
            double size = 5;
            Canvas crossCanvas = new Canvas();
            PathFigure toolPathFigure1 = new PathFigure();
            toolPathFigure1.StartPoint = new Point(0, 0);
            toolPathFigure1.Segments.Add(new LineSegment(new Point(-size, size), true));
            PathFigure toolPathFigure2 = new PathFigure();
            toolPathFigure2.StartPoint = new Point(0, 0);
            toolPathFigure2.Segments.Add(new LineSegment(new Point(-size, -size), true));
            PathGeometry toolPathGeometry = new PathGeometry();
            toolPathGeometry.Figures.Add(toolPathFigure1);
            toolPathGeometry.Figures.Add(toolPathFigure2);
            Path selectedPath = new Path();
            selectedPath.Stroke = new SolidColorBrush(crossColor);
            selectedPath.StrokeThickness = 1;
            selectedPath.Data = toolPathGeometry;
            crossCanvas.Children.Add(selectedPath);
            return crossCanvas;
        }


        public static Canvas createCrossCanvas(Color crossColor)
        {
            double size = 5;
            Canvas crossCanvas = new Canvas();
            PathFigure toolPathFigure1 = new PathFigure();
            toolPathFigure1.StartPoint = new Point(size, size);
            toolPathFigure1.Segments.Add(new LineSegment(new Point(-size, -size), true));
            PathFigure toolPathFigure2 = new PathFigure();
            toolPathFigure2.StartPoint = new Point(-size, size);
            toolPathFigure2.Segments.Add(new LineSegment(new Point(size, -size), true));
            PathGeometry toolPathGeometry = new PathGeometry();
            toolPathGeometry.Figures.Add(toolPathFigure1);
            toolPathGeometry.Figures.Add(toolPathFigure2);
            Path selectedPath = new Path();
            selectedPath.Stroke = new SolidColorBrush(crossColor);
            selectedPath.StrokeThickness = 1;
            selectedPath.Data = toolPathGeometry;
            crossCanvas.Children.Add(selectedPath);
            return crossCanvas;
        }




        public static Canvas createCircleCanvas(Color dotColor)
        {
            double size = 12;
            Canvas circleCanvas = new Canvas();
            Ellipse circle = new Ellipse();
            circle.Stroke = new SolidColorBrush(Colors.Black);

            circle.Width = size;
            circle.Height = size;
            Canvas.SetLeft(circle, - circle.Width / 2);
            Canvas.SetTop(circle, - circle.Height / 2);

            circleCanvas.Children.Add(circle);
            return circleCanvas;
        }

        public static Canvas createFilledCircleCanvas(Color dotColor, double size)
        {
       
            Canvas circleCanvas = new Canvas();
            Ellipse circle = new Ellipse();
            circle.Stroke = new SolidColorBrush(dotColor);
            circle.Fill = new SolidColorBrush(dotColor);
            circle.Width = size;
            circle.Height = size;
            Canvas.SetLeft(circle, -circle.Width / 2);
            Canvas.SetTop(circle, -circle.Height / 2);

            circleCanvas.Children.Add(circle);
            return circleCanvas;
        }



    }
}
