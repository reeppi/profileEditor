   M S B A M L   `   `   `  ���� H  DprofileEditor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:WpfApplication1WpfApplication1  -clr-namespace:profileEditorprofileEditor      profileEditor.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     /localclr-namespace:WpfApplication1     .local1clr-namespace:profileEditor     -     � Title$  profileEditor��$	��762��$	��913�� � 	Resources  �� MergedDictionaries �� Source��  �� $ styles.xaml?��� $ templates.xaml?�%    .��� -    �Name$ mainGrid�� � ColumnDefinitions �� $��* ��� $	��200 � � RowDefinitions �� $��* �.+��� -    ��Name$ canvasSection��$
��Left=�$	��Top=�� $
��Left=�$	��Top=�� $��1�� �� $	��450 ��� $��4 ��� $��* �.+��� $��1��$��Stretch=���  �� Orientation$ Vertical=�$	��Top=�.+��� -   	 ��Name$	 filesBox��$��122.514���� $ 
Horizontal=�.+��� $��W���� -   
 ��Name$
 	textWidth��$��40������ *dxfFile.edge.Right+ �� Mode$ TwoWay=��� $��H���� -   $
 
textHeight��$��40������ *dxfFile.edge.Top+$ TwoWay=��� -    ��Name$ buttonCreateNew��   
blueButton#��[  $	��New���� $ 
Horizontal=�.+��� $
��Grid���� -   $
 textGrid��$��40������ *grid+$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=��� -	   $ 	buttonFit��#��[  $��	FitToView��$��75���� -
    ��Name$ checkGridSnap��$��1��$��GridSnap�� d� 	IsChecked �� *checkGridSnap+$ TwoWay=��� -   $ checkPointSnap��$��1��$��	PointSnap�� �� *checkPointSnap+$ TwoWay=��� $ 
Horizontal=�.+��� $��X���� -   $
 	textMoveX��$��40��$��-5���� $��Y���� -   $
 	textMoveY��$��40���� -   $ 
buttonMove��#��[  $
��Move���� -   $ checkPathDxf��$��1��$��Näytä polku�� �� *checkPathDxf+$ TwoWay=��� -    ��Name$ 
labelCoord��$��1��$
��Left=����� *
labelCoord+�� -   $ mouseSnapPos��$��1��$
��Left=����� *labelSnapCoord+�� $ 
Horizontal=�.+��� -   $ buttonSetStartIndex��#��[  $�� StartPoint ���� $��:���� -   $ labelStartIndex��$��0���� $ Vertical=�.+��� -   $ checkDir��$��Suunta��$��14.96�� �� *
checkDir+$ TwoWay=��� -   $ 	checkSide��$
��Sivu��$��14.96�� �� *	checkSide+$ TwoWay=��� -    ��Name$ 
listdxfObj��$	��100��  	dTemplate#��[  �� AlternationCount$
 1000���� $��0��$��2��$ Vertical=�.+��� -   $ listdxfPathObj��$	��183��#��[ $
 1000���� -   $ buttonGenList��#��[  $��genList���� -   $ buttonGenPathDxf��#��[  $��
genPathDxf���� $ 
Horizontal=�.+��� $��
Halkaisija���� -   $
 textToolDia��$��2��$��40���� -   $ button��#��[  $��Button��$��75���� -   $ 
joinButton��#��[  $	��100��$
��Join��