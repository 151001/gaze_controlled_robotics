   M S B A M L   `   `   `  ���� ?  ;User, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:UserUser      User.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     $localclr-namespace:User     -     �Name$  MainWin�� � Title$ 
MainWindow��$	��800��$	��600��.���  � RowDefinitions �� $	��20* ��� $	��60* ��� $	��20* � � ColumnDefinitions �� $
��200* ��� $
��600* ��� $
��200* �.+��� -    ��Name$ image1��$��0,0,0,3q�$��3��$��3��$
��Fill=�$��images/bg.png���� -   $
 gaze��$
��Left=�$��30��$��0q�$	��Top=�$��50��$��images/Gaze.png��$
��Fill=�$��1��$��1���� ���	#FFD8D8D8$��1q�$��1��$��5,10,5,5q�._��� -   $ fwdImg��$��1��$��0q�$��images/btnbg.png��$
��Fill=��� ���	#FFD8D8D8$��1q�$��10,5,5,5q�$��1��._��� -   $ leftImg��$��0q�$��1��$��images/btnbg.png��$
��Fill=��� ���	#FFD8D8D8$��1q�$��1��$��5,5,5,10q�$��2��._��� -   $ backImg��$��1��$��0q�$��2��$��images/btnbg.png��$
��Fill=��� ���	#FFD8D8D8$��1q�$��2��$��5,5,10,5q�$��1��._��� -   $ rightImg��$��2��$��0q�$��1��$��images/btnbg.png��$
��Fill=��� ���	#FFD8D8D8$��1q�$��	10,10,5,5q�._��� -   $ viewImg��$��0q�$��images/btnbg.png��$
��Fill=��� -	   $ 	fwd_arrow��$��1��$��52��$��0q�$��39��$��images/fwd.png��$
��Fill=�$��Center=�$��Center=��� -
   $ 
left_arrow��$��Center=�$��39��$��0q�$��1��$��Center=�$��51��$
��Fill=�$��images/left.png���� -   $ 
back_arrow��$��1��$��Center=�$��51��$��0q�$��2��$��Center=�$��39��$
��Fill=�$��images/back.png���� -   $ image��$��2��$��Center=�$��39��$��0q�$��1��$��Center=�$��51��$
��Fill=�$��images/right.png���� -   $ mapview��$��0q�$��35��$��52��$��images/view.png���� -    ��Name$ label��$��System Information��$��2��$
��Left=�$��5,5,0,0q�$��2��$	��Top=�$��Microsoft YaHei UI)��� ���	#FFD8D8D8$	��0.5q�$��2��$��
5,30,10,10q�$��2��._��� -    ��Name$ 
sysInfoTxt��$��0q� �� TextWrapping$
 Wrap=�$��Microsoft YaHei Light)����	#FF5C5C5C��Z� ��Z� �� ���	#FFD8D8D8$��1q�$��2��$��	5,10,10,5q�._��� -   $ stopImg��$��0q�$��images/btnbg.png��$
��Fill=��� -   $ image2��$��2��$��0q�$��52��$��52��$��Center=�$��Center=�$
��Fill=�$��images/stop.png��