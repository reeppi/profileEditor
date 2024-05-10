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
    public class classScale
    {
        double val;

        public Double Value
        {
            get
            {
                return this.val;
            }
            set
            {
                if (value < 1) value = 1;
                this.val = value;
            }
        }
    }


    public class classTransUnit
    {
        double max;
        double offset;
        bool flip;
        classTranform transform;

        public classTransUnit(classTranform transform_, double max_, double offset_, bool flip_)
        {
            max = max_;
            offset = offset_;
            flip = flip_;
            transform = transform_;
        }

        public double Offset
        {
            get
            {
                return offset;
            }
        }

        public double Pos(double value)
        {
            double valueReturn = 0;
            if (!flip)
                valueReturn = value * transform.Scale;
            else
            {
                valueReturn = (max - value) * transform.Scale;
            }

            return valueReturn;
        }

        public double PosInv(double value)
        {
            double valueReturn = 0;
            if (!flip)
                valueReturn = value / transform.Scale;
            else
            {
                 valueReturn = max - (value / transform.Scale);
                //valueReturn = (value / transform.Scale);
            }
            return valueReturn;

        }
    }



    public class classTranform
    {
        classTransUnit xTransUnit, yTransUnit;
        classScale classScale;

        public classTranform(classScale classScale_)
        {
            classScale = classScale_;
        }

        public classTransUnit createTransX(double max_, double offset_, bool flip_)
        {
            xTransUnit = new classTransUnit(this, max_, offset_, flip_);
            return xTransUnit;
        }

        public classTransUnit createTransY(double max_, double offset_, bool flip_)
        {
            yTransUnit = new classTransUnit(this, max_, offset_, flip_);
            return yTransUnit;
        }


        public double Scale
        {
            get
            {
                return classScale.Value;
            }
        }


        public double OffsetX
        {
            get
            {
                return xTransUnit.Offset * Scale;
            }
        }

        public double OffsetY
        {
            get
            {
                return yTransUnit.Offset * Scale;
            }
        }


        public classTransUnit XTransUnit
        {
            set
            {
                xTransUnit = value;
            }
        }

        public classTransUnit YTransUnit
        {
            set
            {
                yTransUnit = value;
            }
        }

        public Point PointXY(double xValue, double yValue)
        {
            Point pointXY = new Point(xTransUnit.Pos(xValue), yTransUnit.Pos(yValue));
            return pointXY;
        }


        public double PosInvX(double value)
        {
            return xTransUnit.PosInv(value);
        }

        public double PosInvY(double value)
        {
            return yTransUnit.PosInv(value);
        }


        public double PosX(double xValue)
        {
            return xTransUnit.Pos(xValue);
        }

        public double PosY(double yValue)
        {
            return yTransUnit.Pos(yValue);
        }

    }



}
