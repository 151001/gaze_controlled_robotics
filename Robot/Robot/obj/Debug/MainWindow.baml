   M S B A M L   `   `   `  ����@  <Robot, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:RobotRobot  5   	       Robot.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:Robot     5   	   -   5          �Name$  MainWin��5   	    � Title$ 
MainWindow��5   	   $��831.4��6   $
��1576��6+    � WindowStyle$ ThreeDBorderWindow=�6f   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��300* �6   �� 5      $
��300* �6   �� 5      $
��300* �6    � RowDefinitions 5   
   �� 5      $
��300* �6   �� 5      $
��100* �6   .+�5   
   �� -   5         ��Name$ label��5      $��System Information��6   $��2��6<   $
��Left=�6L   $��5,5,0,0q�6g   $��1��6x   $	��Top=�6�   $��31��6�   $	��165��6�   $��Microsoft YaHei UI)�6�   �� 5   
   ���	#FFD8D8D8$��1q�6)   $��2��6=   $��
5,30,10,10q�6M   $��1��6a   ._�5      �� -   5         ��Name$ 
sysInfoTxt��5      $��0q�6*    �� TextWrapping$
 Wrap=�65   $��Microsoft YaHei Light)�6v   ���	#FF5C5C5C��6I   Z� ��6_   Z� 6�   5      �� 5   
   -   5         ��Name$ groupBox��5      $��1��6%   $��Robot Information��65   $��5,5,5,10q�6P   $��1��6b   $��Microsoft YaHei UI)�6o   �� 5   
   -   5        $ label1��5      $��Serial port:��6    $��1��67   $
��Left=�6G   $��
9.8,30,0,0q�6b   $	��Top=�6v   $	��100��6�   $��1��6�   $��Microsoft YaHei UI Light)�6�   �� 5   
   -   5        $ label1_Copy��5      $��GameOnTrack:��6%   $��1��6<   $
��Left=�6L   $��
9.8,60,0,0q�6g   $	��Top=�6{   $	��100��6�   $��1��6�   $��Microsoft YaHei UI Light)�6�   �� 5   
   -   5        $ label1_Copy1��5      $��X position:��6&   $��1��6<   $
��Left=�6L   $��
9.8,90,0,0q�6g   $	��Top=�6{   $	��100��6�   $��1��6�   $��Microsoft YaHei UI Light)�6�   �� 5   
   -   5        $ label1_Copy2��5      $��Y position:��6&   $��1��6<   $
��Left=�6L   $��9.8,120,0,0q�6g   $	��Top=�6|   $	��100��6�   $��1��6�   $��Microsoft YaHei UI Light)�6�   �� 5   
   -	   5        $ label1_Copy3��5      $��Orientation:��6&   $��1��6=   $
��Left=�6M   $��9.8,150,0,0q�6h   $	��Top=�6}   $	��100��6�   $��1��6�   $��Microsoft YaHei UI Light)�6�   �� 5   
   -
   5        $ 	serialTxt��5      $��1��6%   $
��Left=�65   $��23��6P   $��
120,30,0,0q�6\   $��1��6p   $
 Wrap=�6}   $��Close��6�   $	��Top=�6�   $	��120��6�   $��Center=�6�   ���	#FF5C5C5C$��Microsoft YaHei Light)�6  ��6�   Z� 6  �� 5   
   -   5        $ GOTTxt��5      $��1��6"   $
��Left=�62   $��23��6M   $��
120,60,0,0q�6Y   $��1��6m   $
 Wrap=�6z   $��Not connected��6�   $	��Top=�6�   $	��120��6�   $��Center=�6�   ���	#FF5C5C5C$��Microsoft YaHei Light)�6  ��6�   Z� 6  �� 5   
   -   5        $ RobXTxt��5      $��1��6#   $
��Left=�63   $��23��6N   $��
120,90,0,0q�6Z   $��1��6n   $
 Wrap=�6{   $��0��6�   $	��Top=�6�   $	��120��6�   $��Center=�6�   ���	#FF5C5C5C$��Microsoft YaHei Light)�6  ��6�   Z� 6  �� 5    
   -   5        $ RobYTxt��5       $��1��6#   $
��Left=�63   $��23��6N   $��120,120,0,0q�6Z   $��1��6o   $
 Wrap=�6|   $��0��6�   $	��Top=�6�   $	��120��6�   $��Center=�6�   ���	#FF5C5C5C$��Microsoft YaHei Light)�6  ��6�   Z� 6  �� 5!   
   -   5        $ RobThTxt��5!      $��1��6$   $
��Left=�64   $��23��6O   $��120,150,0,0q�6[   $��1��6p   $
 Wrap=�6}   $��0��6�   $	��Top=�6�   $	��120��6�   $��Center=�6�   ���	#FF5C5C5C$��Microsoft YaHei Light)�6  ��6�   Z� 6  5"      5#      