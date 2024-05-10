using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace profileEditor
{
    public class classEdge : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] String caller = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));}

        double left, right, top, floor;

        public double Left
        {
            get { return left; }
            set { left = value; }
        }
        public double Right
        {
            get { return right; }
            set {
                right = value;
                OnPropertyChanged();
            }
        }
        public double Top
        {
            get { return top; }
            set {
                top = value;
                OnPropertyChanged();
            }
        }
        public double Floor
        {
            get { return floor; }
            set { floor = value; }
        }

        public classEdge(double left_, double right_, double top_, double floor_)
        {
            left = left_;
            right = right_;
            top = top_;
            floor = floor_;
        }

        public classEdge()
        {


        }

        
       



        /*
        public classEdge(classDxf dxf)
        {

            double floorn = 10000;
            double leftn = 10000;
            double rightn = 0;
            double ceiln = 0;

            if (dxf.Lines == null) return;
            foreach (line line in dxf.Lines)
            {
                if (line.y1 < floorn) floorn = line.y1;
                if (line.y2 < floorn) floorn = line.y2;

                if (line.y1 > ceiln) ceiln = line.y1;
                if (line.y2 > ceiln) ceiln = line.y2;

                if (line.x1 < leftn) leftn = line.x1;
                if (line.x2 < leftn) leftn = line.x2;

                if (line.x1 > rightn) rightn = line.x1;
                if (line.x2 > rightn) rightn = line.x2;

            }
            Left = Math.Round(-leftn, 1);
            Top = Math.Round(ceiln, 1);
            Right = Math.Round(rightn, 0);
        }*/
    }
}
