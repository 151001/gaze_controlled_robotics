   M S B A M L   `   `   `  ����?  ;User, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:UserUser  5   	       User.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   $localclr-namespace:User     5   	   -   5          �Name$  MainWin��5   	    � Title$ 
MainWindow��5   	   $	��800��6x   $	��600��6�   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��200* �6   �� 5      $
��200* �6   �� 5      $
��200* �6    � ColumnDefinitions 5   
   �� 5      $
��200* �6   �� 5      $
��200* �6   �� 5      $)��#200                               * �6   .+�5   
   �� -   5         ��Name$ label��5      $��System Information��6   $��2��6<   $
��Left=�6L   $��5,5,0,0q�6g   $��2��6x   $	��Top=�6�   $	��140��6�   $��Microsoft JhengHei UI)�6�   $��24��6�   �� 5   
   ���	#FFD8D8D8$��1q�6)   $��2��6=   $��
5,30,10,10q�6M   $��2��6a   ._�5      �� -   5         ��Name$ 
sysInfoTxt��5      $��0q�6*    �� TextWrapping$
 Wrap=�65   ���	#26DCDCDC$��Microsoft YaHei Light)�6w   $��11(�6�   ���	#FF5C5C5C��6`   Z� 6�   5      �� 5   
   ���	#FFD8D8D8$��1q�6)   $��	10,10,5,5q�6=   ._�5      �� -   5         ��Name$ lfImg��5      $
��Fill=�6#   $��images/btnbg.png��62   6�   5      �� 5   
   ���	#FFD8D8D8$��1q�6)   $��1��6=   $��5,10,5,5q�6M   ._�5      �� -   5        $
 fImg��5      $��images/btnbg.png��6"   $
��Fill=�6<   6�   5      �� 5   
   ���	#FFD8D8D8$��1q�6)   $��2��6=   $��	5,10,10,5q�6M   ._�5       �� -   5        $ rfImg��5       $
��Fill=�6#   $��images/btnbg.png��62   6�   5!      �� 5"   
   ���	#FFD8D8D8$��1q�6)   $��10,5,5,5q�6=   $��1��6O   ._�5#      �� -   5        $
 lImg��5#      $
��Fill=�6"   $��images/btnbg.png��61   6�   5$      �� 5%   
   ���	#FFD8D8D8$��1q�6)   $��1��6=   $��5q�6M   $��1��6X   ._�5&      �� -   5        $ stopImg��5&      $��images/btnbg.png��6%   $
��Fill=�6?   6�   5'      �� 5(   
   ���	#FFD8D8D8$��1q�6)   $��2��6=   $��5,5,10,5q�6M   $��1��6_   ._�5)      �� -	   5        $
 rImg��5)      $��images/btnbg.png��6"   $
��Fill=�6<   6�   5*      �� 5+   
   ���	#FFD8D8D8$��1q�6)   $��1��6=   $��5,5,5,10q�6M   $��2��6_   ._�5,      �� -
   5        $
 bImg��5,      $
��Fill=�6"   $��images/btnbg.png��61   6�   5-      �� 5.   
   -   5        $ ficon��5.      $��1��6   $	��104��6/   $��0q�6<   $��78��6G   $��images/fwd.png��6R   $
��Fill=�6j   $��Center=�6y   $��Center=�6�   6�   �� 5/   
   -   5        $ bicon��5/      $��1��6   $	��104��6/   $��0q�6<   $��78��6G   $��images/back.png��6R   $
��Fill=�6k   $��Center=�6z   $��Center=�6�   $��2��6�   6�   �� 50   
   -   5        $ licon��50      $��78��6   $��0q�6+   $	��104��66   $��images/left.png��6B   $
��Fill=�6[   $��Center=�6j   $��Center=�6�   $��1��6�   6�   �� 51   
   -   5        $ ricon��51      $��78��6   $��0q�6+   $	��104��66   $��images/right.png��6B   $
��Fill=�6\   $��Center=�6k   $��Center=�6�   $��1��6�   $��2��6�   6�   �� 52   
   -   5        $ lficon��52      $��91��6    $��0q�6,   $��91��67   $��images/lf.png��6B   $
��Fill=�6Y   $��Center=�6h   $��Center=�6�   6�   �� 53   
   -   5        $ rficon��53      $��91��6    $��0q�6,   $��91��67   $��images/rf.png��6B   $
��Fill=�6Y   $��Center=�6h   $��Center=�6�   $��2��6�   6�   �� 54   
   -   5        $ stopicon��54      $��1��6"   $��78��62   $��0q�6>   $��78��6I   $��images/stop.png��6T   $
��Fill=�6m   $��Center=�6|   $��Center=�6�   $��1��6�   6  55      56      