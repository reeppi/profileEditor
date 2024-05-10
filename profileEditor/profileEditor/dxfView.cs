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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;


namespace profileEditor
{
    class classDxfView : Canvas, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] String caller = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller)); }

        public classDxfFile dxfFile { get; set; }
        public classScale scale { get; set; }

        public Point mousePos;
        public Point mousePosSnap;

        public double panX { get; set; }
        public double panY { get; set; }
        public bool   fit { get; set; }

        int TrackIndex;
        bool CheckPathDxf;
        bool CheckPointSnap;
        bool CheckGridSnap;
        bool CheckDir;
        bool CheckSide;
        string LabelCoord;
        string LabelSnapCoord;
        double Grid;

        public double offsetX = 0;
        public double offsetY = 0;
        double border = 0;


        public ObservableCollection<IdxfObj> trackDxf { get; set; }
        public ObservableCollection<IdxfObj> trackDxfPath { get; set; }

        public int trackIndex {
            get { return TrackIndex; }
            set {
                TrackIndex = value;
                OnPropertyChanged();
                Console.WriteLine("trackIndex changed " + TrackIndex);
                checkSide = dxfFile.trackList[trackIndex].side;
                checkDir = dxfFile.trackList[trackIndex].dir;
                generateTrackCollection();
            } }
        public bool checkPathDxf {
            get { return CheckPathDxf; }
            set { CheckPathDxf = value; OnPropertyChanged();  } }
        public bool checkPointSnap {
            get { return CheckPointSnap; }
            set { CheckPointSnap = value; OnPropertyChanged(); } }
        public bool checkGridSnap {
            get { return CheckGridSnap; }
            set { CheckGridSnap = value;
                OnPropertyChanged();
            } }
        public string labelCoord {
            get { return LabelCoord; }
            set { LabelCoord = value; OnPropertyChanged(); } }
        public string labelSnapCoord {
            get { return LabelSnapCoord; }
            set { LabelSnapCoord = value; OnPropertyChanged(); } }
        public double grid {
            get { return Grid; }
            set {
                Grid = value;
                OnPropertyChanged();
                Console.WriteLine("Grid Changed = "+Grid);
            } }
        public bool checkDir {
            get { return CheckDir; }
            set {
                CheckDir = value;
                OnPropertyChanged();
                dxfFile.trackList[trackIndex].dir = value;
                Console.WriteLine("CheckDir Changed = " + CheckDir);
            } }
        public bool checkSide {
            get { return CheckSide; }
            set { CheckSide = value;
                OnPropertyChanged();
                dxfFile.trackList[trackIndex].side = value;
                Console.WriteLine("CheckSide Changed = " + checkSide);
            } }


        public void generateTrackCollection()
        {
            trackDxf.Clear();
            trackDxfPath.Clear();

            foreach (IdxfObj dxfObj in dxfFile.trackList[trackIndex].dxf.DxfObjects)
                trackDxf.Add(dxfObj);
            foreach (IdxfObj dxfObj in dxfFile.trackList[trackIndex].dxfPath.DxfObjects)
                trackDxfPath.Add(dxfObj);

        }

        public void generatePathTrackAndDrawCanvas()
        {
            dxfFile.generatePathTrack();
            generateTrackCollection();
            drawCanvas();
        }


        public classDxfTrack selectedTrack
        {
            get
            {
               // if (trackIndex >= dxfFile.trackList.Count()) return null;
                return dxfFile.trackList[trackIndex];
            }
        }

        public double viewWidth, viewHeight;

        public classDxfView(classDxfFile dxfFile_)
        {
            scale = new classScale();
            dxfFile = dxfFile_;
            panX = 0;
            panY = 0;
            offsetX = 0;
            offsetY = 0;
            border = 0;
            trackDxf = new ObservableCollection<IdxfObj>();
            trackDxf.CollectionChanged += this.OnCollectionChanged;
            trackDxfPath = new ObservableCollection<IdxfObj>();
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }


        public void drawCanvas(double viewWidth_, double viewHeight_)
        {
            viewWidth = viewWidth_;
            viewHeight = viewHeight_;
            drawCanvas();
        }


        public void drawCanvas()
        {
            classTranform transform;
            classSectionCanvas sectionCanvas;
            classMainCanvas mainCanvas;

            if (dxfFile == null) return;
                Children.Clear();
            double toolDia = dxfFile.toolDia;
            border = toolDia;
            double width = dxfFile.edge.Right + border * 2;
            double height = dxfFile.edge.Top + border * 2;
            double newScale;
            newScale = viewWidth / width;
            if (fit)
            {
                offsetY = (viewHeight / 2) / newScale - height / 2;
                if (newScale * height > viewHeight)
                {
                    offsetY = 0;
                    newScale = viewHeight / height;
                    offsetX = (viewWidth / 2) / newScale - width / 2;
                }
                scale.Value = newScale;
            }
            transform = new classTranform(scale);
            transform.XTransUnit = transform.createTransX(0, offsetX + border - panX, false);
            transform.YTransUnit = transform.createTransY(height, offsetY - border - panY, true);
            sectionCanvas = new classSectionCanvas(this, transform, dxfFile.edge.Right, dxfFile.edge.Top, border);
            mainCanvas = new classMainCanvas(this, transform, viewWidth, viewHeight);
            mainCanvas.Children.Add(sectionCanvas);
            this.Children.Add(mainCanvas);
            RectangleGeometry rectGeo = new RectangleGeometry(new Rect(0, 0, viewWidth, viewHeight));
            this.Clip = rectGeo;

        }



        public void updateZoom(double zoom)
        {
            double toolDia = dxfFile.toolDia;
            border = toolDia;
            double mouseMainX = Mouse.GetPosition(this).X;
            double mouseMainY = Mouse.GetPosition(this).Y;
            double xMouseFact = (mouseMainX / viewWidth) - 0.5;
            double yMouseFact = 1 - (mouseMainY / viewHeight) - 0.5;
            double xMouseClamp = mousePos.X.Clamp(0, dxfFile.edge.Right);
            double yMouseClamp = mousePos.Y.Clamp(0, dxfFile.edge.Top);
            scale.Value += zoom;
            double MoveXFact = xMouseClamp / dxfFile.edge.Right - 0.5;
            double MoveYFact = yMouseClamp / dxfFile.edge.Top - 0.5;
 
            if (zoom > 0)
            {
                panX += ((viewWidth / scale.Value) / 2 * xMouseFact )/2;
                panY += (-(viewHeight / scale.Value) / 2 * yMouseFact)/2;
            }
            double width = dxfFile.edge.Right + border * 2;
            double height = dxfFile.edge.Top + border * 2;
            offsetX = (viewWidth / scale.Value - width) / 2;
            offsetY = (viewHeight / scale.Value - height) / 2;
            drawCanvas();
        }




    }
}
