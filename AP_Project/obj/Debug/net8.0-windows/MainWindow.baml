   M S B A M L   `   `   `  ����  
AP_Project    AP_Project.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  File Backup Utility��5   	   $	��400��6%   $	��600��62   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $	��0 5q�6.   .+�5      �� $��Source:��6   $��Center=�6'   $��50��6B   �� 5      -   5         ��Name$ SourceTextBox��5      $	��400��6-   $	��5 0q�69   �� 5      -   5        $��Browse��5      6&   5      �� 5   
   $ 
Horizontal=�6   $	��0 5q�6.   $��1��6;   .+�5      �� $��Destination:��6   $��Center=�6,   $��75��6G   �� 5      -   5        $ DestinationTextBox��5      $	��375��62   $	��5 0q�6>   �� 5      -   5        $��Browse��5      6&   5      �� 5   
   $ 
Horizontal=�6   $
��0 10q�6.   $��2��6<   .+�5      �� -   5        $��Start Backup��5      $	��120��6,   $��0 0 10 0q�68   6J   �� 5      -   5        $��Cancel��5      $	��120��6&   62   5       �� 5#   
   -   5        $ 
LogTextBox��5#      $��3��6&   $
��0 10q�63    �� 
IsReadOnly$
 True��6A    �� AcceptsReturn$
 True��6S   " �� VerticalScrollBarVisibility$
 Auto=�6h   �� 5&   
   $��4��6   $	��0 5q�6"   .+�5'      � -   5         �Name$ ProgressBar��5'      $��20��6/    � Minimum$ 0Q�6;   	 � Maximum$		 100Q�6G   $��0 0 0 5q�6U   �� 5(      -	   5        
 ��Name$
 StatusTextBlock��5(      $��Ready.��61   $
��Bold"�6?   5)      5*      5+      