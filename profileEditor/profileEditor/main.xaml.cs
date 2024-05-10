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

namespace profileEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    

        public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public Point mousePos;
        public Point mousePosSnap;

        classSettings settings { get; set; }
        public classScale scale { get; set; }

        public double panX { get; set;  }
        public double panY { get; set; }
        public bool fit { get; set; }
      //  int trackIndex = 0;
  
       // classSectionCanvas sectionCanvas;
       // classMainCanvas mainCanvas;
  

        // Uutta 
        classDxfView dxfView = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            settings = new classSettings();
            addDxfFilesToListBox();
            scale = new classScale();
            scale.Value = 1;
            //sectionCanvas = null;
            listDxfSelection(true);
        }

        private void addDxfFilesToListBox()
        {
            string filePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string directory = System.IO.Path.GetDirectoryName(filePath);

            DirectoryInfo dinfo = new DirectoryInfo(directory);

    

            // DirectoryInfo dinfo = new DirectoryInfo(settings.FilesPath + "\\" + settings.ProfilesDir);
            if (!dinfo.Exists)
            {
                MessageBox.Show("Hakemistoa " + dinfo.ToString() + " ei löydy");
                return;
            }
            FileInfo[] Files = dinfo.GetFiles("*.dxf");
            filesBox.Items.Clear();
            foreach (FileInfo file in Files)
            {
                filesBox.Items.Add(System.IO.Path.GetFileName(file.Name));
                Console.WriteLine(System.IO.Path.GetFileName(file.Name));
            }
        }



        private void filesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string file = filesBox.Items[filesBox.SelectedIndex].ToString();

            string fileP = System.Reflection.Assembly.GetEntryAssembly().Location;
            string directory = System.IO.Path.GetDirectoryName(fileP);

            string filePath = directory+ "\\" + file;

            double toolDia = 0;
            Double.TryParse(textToolDia.Text, out toolDia);

            double grid_default = 1;
            bool checkPathDxf_default = true;
            bool checkPointSnap_default = true;
            bool checkGridSnap_default = false;

            if ( dxfView != null)
            {
               grid_default = dxfView.grid;
               checkPathDxf_default = dxfView.checkPathDxf;
               checkPointSnap_default = dxfView.checkPointSnap;
               checkGridSnap_default = dxfView.checkGridSnap;
            }

            classDxfFile dxfFile = new classDxfFile(filePath, toolDia);
            dxfView = new classDxfView(dxfFile);
            
            dxfView.fit = true;
            dxfView.grid = grid_default;
            dxfView.checkPathDxf = checkPathDxf_default;
            dxfView.checkPointSnap = checkPointSnap_default;
            dxfView.checkGridSnap = checkGridSnap_default;

            listdxfObj.ItemsSource = dxfView.trackDxf;
            listdxfPathObj.ItemsSource = dxfView.trackDxfPath;

            dxfView.trackIndex = 0;


            this.DataContext = dxfView;
            Title = "file : " + file;
          //  updateList();
            drawSectionCanvas();
        }

        public void drawSectionCanvas()
        {
            if (dxfView == null) return;
            if (canvasSection != null) canvasSection.Children.Clear();

            double viewWidth = mainGrid.ColumnDefinitions[0].ActualWidth;
            double viewHeight = mainGrid.RowDefinitions[0].ActualHeight;
            dxfView.drawCanvas(viewWidth, viewHeight);
            canvasSection.Children.Add(dxfView);

        }

 
        private void mainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (dxfView == null) return;
            if ( dxfView.fit)
            {
                dxfView.offsetX = 0;
                dxfView.offsetY = 0;
                dxfView.panX = 0;
                dxfView.panY = 0;
            }
            drawSectionCanvas();
        }

        private void buttonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            double width;
            double height;

            if (canvasSection != null)
                canvasSection.Children.Clear();

            Double.TryParse(textWidth.Text, out width);
            Double.TryParse(textHeight.Text, out height);
           // dxf = new classDxf(width,height);
          //  dxfPath = new classDxf(0,0);
          //  dxfPath.path = true;
            drawSectionCanvas();
        }

        private void textGrid_TextChanged(object sender, TextChangedEventArgs e)
        {
                drawSectionCanvas();
        }

        private void mainGrid_MouseMove(object sender, MouseEventArgs e)
        {
           // var p = Mouse.GetPosition(mainGrid);
           // Console.WriteLine("p.X : "+p.X+" p.Y :"+p.Y );
        }

        private void buttonMove_Click(object sender, RoutedEventArgs e)
        {
            double xMove = 0;
            double yMove = 0;
            Double.TryParse(textMoveX.Text, out xMove);
            Double.TryParse(textMoveY.Text, out yMove);

            foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
            {
                dxfTrack.dxf.movePoints(xMove, yMove);
                dxfTrack.dxfPath.movePoints(xMove, yMove);
            }
            dxfView.dxfFile.calculateEdgevaluesFromTrackList();

            //dxfView.generatePathTrackAndDrawCanvas();
           // dxfFile.generatePathTrack();
           drawSectionCanvas();
        }

        public void listDxfSelection(bool setOff)
        {       
            if (!setOff)
            {
              //  listdxfPathObj.SelectionChanged -= listdxfPathObj_SelectionChanged;
              //  listdxfObj.SelectionChanged -= listdxfObj_SelectionChanged;

            }
            else
            {
              //  listdxfPathObj.SelectionChanged += listdxfPathObj_SelectionChanged;
              //  listdxfObj.SelectionChanged += listdxfObj_SelectionChanged;
            }
        }

        
        private void updateList()
        {
            /*
            if (dxfView.dxfFile == null) return;
            listDxfSelection(false);

            listdxfObj.Items.Clear();
            listdxfPathObj.Items.Clear();
            Console.WriteLine("trackIndexUpdateList : "+trackIndex);
            int i = 0;
            foreach (IdxfObj dxfObj in dxfView.dxfFile.trackList[trackIndex].dxf.DxfObjects)
            {
                listdxfObj.Items.Add(i+": "+dxfObj.TypeStr + " X:" + Math.Round(dxfObj.start.X, 2) + " Y:" + Math.Round(dxfObj.start.Y, 2) + " X:" + Math.Round(dxfObj.end.X, 2) + " Y" + Math.Round(dxfObj.end.Y, 2));
                i++;
            }
            i = 0;
            foreach (IdxfObj dxfObj in dxfView.dxfFile.trackList[trackIndex].dxfPath.DxfObjects)
            {
                listdxfPathObj.Items.Add(i + ": " + dxfObj.TypeStr + "X:" + Math.Round(dxfObj.start.X, 2) + " Y:" + Math.Round(dxfObj.start.Y, 2) + " X:" + Math.Round(dxfObj.end.X, 2) + " Y" + Math.Round(dxfObj.end.Y, 2) + " A:" + Math.Round(dxfObj.angle));
                i++;
            }
            listDxfSelection(true);
            */
        }
        
 

        private void buttonGenList_Click(object sender, RoutedEventArgs e)
        {
            updateList();

        }

        private void checkPathDxf_Checked(object sender, RoutedEventArgs e)
        {
         
        }

        private void checkPathDxf_Click(object sender, RoutedEventArgs e)
        {
            drawSectionCanvas();
        }
        

        private void buttonGenPathDxf_Click(object sender, RoutedEventArgs e)
        {
            dxfView.generatePathTrackAndDrawCanvas();
        }

        private void listdxfObj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv.Name == "listdxfObj")
                dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.selectStartAndEndPointByDxfObj(lv.SelectedItems);
            if (lv.Name == "listdxfPathObj")
                dxfView.dxfFile.trackList[dxfView.trackIndex].dxfPath.selectStartAndEndPointByDxfObj(lv.SelectedItems);

            /*
            List<int> indexes = new List<int>();
            if (lv.Name == "listdxfObj")
            {
                foreach (IdxfObj lvItem in lv.SelectedItems)
                    indexes.Add(dxfView.trackDxf.IndexOf(lvItem));
            }
            if (lv.Name == "listdxfPathObj")
            {
                foreach (IdxfObj lvItem in lv.SelectedItems)
                    indexes.Add(dxfView.trackDxfPath.IndexOf(lvItem));
            }

            Console.WriteLine("listdxfObj_SelectionChanged "+ lv.SelectedIndex);
            if ( lv.Name == "listdxfObj")
              dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.selectStartAndEndPointByIndex(indexes);
            if ( lv.Name == "listdxfPathObj")
                dxfView.dxfFile.trackList[dxfView.trackIndex].dxfPath.selectStartAndEndPointByIndex(indexes);
            */

            drawSectionCanvas();
            //mainCanvas.updateSelectedPointCanvas();
        }


        private void listdxfObj_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        //    foreach (classDxfTrack dxfTrack in dxfFile.trackList)
          //      dxfTrack.dxf.selectEndPointByIndex(listdxfObj.SelectedIndex);
          //  mainCanvas.updateSelectedPointCanvas();
        }

        private void listdxfPathObj_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           // dxfPath.selectEndPointByIndex(listdxfPathObj.SelectedIndex);
           // mainCanvas.updateSelectedPointCanvas();
        }

       


        private void buttonSetStartIndex_Click(object sender, RoutedEventArgs e)
        {
            //labelStartIndex.Content = listdxfObj.SelectedIndex.ToString();
            int index = 0;
           
            foreach (classDxfTrack dxfTrack in dxfView.dxfFile.trackList)
            {
                foreach (dxfPoint dxfPt in dxfTrack.dxf.getAlldxfPoints().ToList() )
                {
                    if (dxfPt.Selected)
                    {
                        foreach (dxfPoint dxfPtTmp in dxfTrack.dxf.getAlldxfPoints())
                            dxfPtTmp.begin = false;
                        dxfPt.begin = true;
                        goto jump;
                    }      
                }
                index++;
            }

            foreach (dxfPoint dxfPt in dxfView.dxfFile.getAllDxfPointsFromDxfTrackList())
                dxfPt.Selected = false;

            jump:

            dxfView.generatePathTrackAndDrawCanvas();
        }

   


        private void checkDir_Click(object sender, RoutedEventArgs e)
        {
            dxfView.generatePathTrackAndDrawCanvas();
            // Console.WriteLine("trackIndex " + trackIndex);
            // dxfView.dxfFile.generatePathTrack();
            // drawSectionCanvas();
            //  updateList();
        }

        private void checkSide_Click(object sender, RoutedEventArgs e)
        {
            dxfView.generatePathTrackAndDrawCanvas();

           // dxfView.dxfFile.generatePathTrack();
           // drawSectionCanvas();
           // updateList();
        }

        private void checkSide_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            List<IdxfObj> selectedItems = listdxfObj.SelectedItems.Cast<IdxfObj>().ToList();

            foreach (IdxfObj item in selectedItems)
            {
                dxfView.trackDxf.Remove(item);
                dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects.Remove(item);
            }
            if ( dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects.Count() == 0 )
                dxfView.dxfFile.trackList.RemoveAt(dxfView.trackIndex);

            dxfView.generatePathTrackAndDrawCanvas();


            //  foreach (IdxfObj selItem in listdxfObj.SelectedItems)
            //     dxfView.trackDxf.Remove((IdxfObj)listdxfObj.SelectedItem);


            //dxfFile.generatePathTrack();
            //drawSectionCanvas();
            //updateList();
            //Point sect1 = new Point();
            //Point sect2 = new Point();

            //          Console.WriteLine("trackList Count " + dxfFile.trackList.Count());

            // dxfArc arc = (dxfArc)dxfFile.trackList[trackIndex].dxf.DxfObjects[listdxfObj.SelectedIndex];


            // classHelper.FindCollisionLineArc((dxfLine)dxfFile.trackList[1].dxfPath.DxfObjects[0], (dxfArc)dxfFile.trackList[0].dxfPath.DxfObjects[0], out sect);
            // classHelper.FindCollisionArcArc((dxfArc)dxfFile.trackList[1].dxfPath.DxfObjects[0], (dxfArc)dxfFile.trackList[0].dxfPath.DxfObjects[0], out sect1);

        }

        private void checkDir_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void textToolDia_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dxfView != null)
            {
                double toolDia = 1;
                double.TryParse(textToolDia.Text, out toolDia);
                dxfView.dxfFile.toolDia = toolDia;
        
                dxfView.generatePathTrackAndDrawCanvas();
            }
        }

        private void buttonFit_Click(object sender, RoutedEventArgs e)
        {
            dxfView.fit = true;
            dxfView.panX = 0;
            dxfView.panY = 0;
            dxfView.offsetX = 0;
            dxfView.offsetY = 0;
            drawSectionCanvas();
        }

        private void joinButton_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects.Count();i++)
            {
                IdxfObj curObj = dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects[i];
                if (curObj.start.Selected)
                {
                    int count = 0;
                    for (int ix = i + 1; ix < dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects.Count(); ix++)
                    {
                        count++;
                        IdxfObj fObj = dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects[ix];
                        if (fObj.start.Selected)
                        {
                            dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects.RemoveRange(i,count);
                            Console.WriteLine("Removed Count : "+count);

                            dxfLine line = new dxfLine(new dxfPoint(curObj.start.X, curObj.start.Y), new dxfPoint(fObj.start.X, fObj.start.Y));
                            dxfView.dxfFile.trackList[dxfView.trackIndex].dxf.DxfObjects.Insert(i, line);

                            dxfView.generatePathTrackAndDrawCanvas();
                            return;
                        }
                    }
                }
            }
         
        }
    }
}
