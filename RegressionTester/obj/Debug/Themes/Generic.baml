   M S B A M L   `   `   `  ����E  ADevRegTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:DevRegTest
DevRegTest  5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089&     DevRegTest.TreeListViewConverter   TreeListViewConverter  �� 
TargetType  TreeListViewItem ��Name �� Content �� Columns �� RelativeSource �� Mode �� AncestorType ��Name �� Triggers	 P� Property
 P� Value �� 
TargetName �� Property �� Value ]� 
Conditions �� Property �� Value%   DevRegTest.TreeListViewExpander  TreeListViewExpander ��Name �� Orientation �� Self �� 	Converter d� 	IsChecked �� Value �� Binding   DevRegTest.TreeListView �� 
TargetType � 	Resources � RowDefinitions   Columns   AllowsColumnReorder �� AllowsColumnReorder�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      "localclr-namespace:DevRegTest  5      %N  &	        &	      &	 r    7   '  |    7  7     5      6"   �� 5      "  Q�6   .R�5   
   �� .+�5      �� $ Border��6   ._�5      �� # z��6'    5      �� *	Columns+ �� $ FindAncestor=�$ local:TreeListViewL�5      �� 5      $ ItemsPresenter��6   $y�	Collapsed=�65   5       5   
   P� 5      	 �@Q�
IsExpanded
 . . �5      �� $ ItemsPresenter��6    � y�$ Visible=�6K   5      P� 5      	 �@Q�
IsSelected
 . . �5!      �� $ Border��6   # � ��5"       � ���� 5$      # � ��5%       � ��5&      ]� 5(       5)      �� 5*       �@Q�
IsSelected . 66   �� 5+       �@Q�IsSelectionActive .  6=   ./�5.      �� $ Border��6   # � ��5/       � ���� 51      # � ��52       � ��53      P� 55      	 � {�
 .  . �57      �� # � ��58       � ��59      5;      �� 5>      "   6   .R�5?   
   �� $ ContainerElement��6.   $ 
Horizontal=�6   .+�5A      � ��6   �� # Z 8   d� 5D      $��9��5E   .   $��9��68   $��Center=�6C   $��1q�6^    5D      �� *
IsExpanded+ �� $ FindAncestor=�$ TreeViewItemL�5E   ^   5F       5G   
   �� 5I      $ False��5J   .    5I      �� *
HasItems+ �� $ FindAncestor=�$ TreeViewItemL�.a�5K      �� $ ContainerElement��6    � y�$ Hidden=�6M   5L      5N      �� 5Q      "  6   .�5S   
   ��  � �� 5T      �� 5U      "   6"    5V      ���� 5X      'Q�       " Q�6    .�5Y      ��  � ��8   69   5Z      �� 5\      '        "  6    .�5]      ��  � ��8  69   5^      5_      .R�5a      �� #��z��6   #��z��5b      #��z��5c      ._�5f      �� $
��Auto=�6'   $��Disabled=�5g   '   .��5i      �  5j   "   �� 5l   &   $
��Auto �64   �� 5n   &   $��* �64   .+�5q   "   �� # z 6=   # z 5r   %   �� 5u   "   $��Disabled=�6/   $
��Auto=�5v   /   $��1��5w   /   .��5y   &   �� 5z   #   5{      5|      5}      5~      5�      5�      5�      