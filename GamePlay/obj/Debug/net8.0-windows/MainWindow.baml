   M S B A M L   `   `   `  ����C  ?GamePlay, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:GamePlayGamePlay  5   	       GamePlay.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:GamePlay     5   	     � Title$  Game Pikachu��5   
   $	��506��6   $	��840��6,    � WindowStartupLocation$ CenterScreen=�6:   ���	#FF4CCBB8.��5	      � $��0,0,0,-6q�6    � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��10q�6.   .+�5      �� -   5         ��Name$ 
btnNewGame��5      $��
Game mới��6)   $	��100��6<   $��30��6H   6T   �� 5      $��  Điểm: ��6   $��20(�6(   $��Center=�66   �� 5      -   5         ��Name$ 	ScoreText��5      $��0��6+   $��20(�64   $��Center=�6B   �� 5      $��  Thời gian: ��6   $��20(�6-   $��20,0,0,0q�6;   $��Center=�6M   �� 5      -   5        $ TimeText��5      $	��100��6*   $��20(�65   $��Center=�6C   5      �� 5   
   $��1��6   $��10,10,10,10q�6   ���	#FF085FA1._�5      �� -   5         ��Name$ 
GameCanvas��5      5      5      5      