   M S B A M L   `   `   `  ���� E  ADevRegTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:DevRegTest
DevRegTest  R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089&     DevRegTest.TreeListViewConverter   TreeListViewConverter  �� 
TargetType  TreeListViewItem ��Name �� Content �� Columns �� RelativeSource �� Mode �� AncestorType ��Name �� Triggers	 P� Property
 P� Value �� 
TargetName �� Property �� Value ]� 
Conditions �� Property �� Value%   DevRegTest.TreeListViewExpander  TreeListViewExpander ��Name �� Orientation �� Self �� 	Converter d� 	IsChecked �� Value �� Binding   DevRegTest.TreeListView �� 
TargetType � 	Resources � RowDefinitions   Columns   AllowsColumnReorder �� AllowsColumnReorder�� D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     "localclr-namespace:DevRegTest  %�  &	        &	      &	     7   '  c    7  7     �� "  Q�.R��� .+��� $ Border��._��� # z�� �� *	Columns+ �� $ FindAncestor=�$ local:TreeListViewL��� $ ItemsPresenter��$y�	Collapsed=� P� 	 �@Q�
IsExpanded
 . . ��� $ ItemsPresenter�� � y�$ Visible=�P� 	 �@Q�
IsSelected
 . . ��� $ Border��# � �� � ���� # � �� � ��]�  ��  �@Q�
IsSelected . ��  �@Q�IsSelectionActive .  ./��� $ Border��# � �� � ���� # � �� � ��P� 	 � {�
 .  . ��� # � �� � ���� "   .R��� $ ContainerElement��$ 
Horizontal=�.+�� ���� # Z 8   d� $��9��$��9��$��Center=�$��1q� �� *
IsExpanded+ �� $ FindAncestor=�$ TreeViewItemL� �� $ False�� �� *
HasItems+ �� $ FindAncestor=�$ TreeViewItemL�.a��� $ ContainerElement�� � y�$ Hidden=��� "  .���  � �� �� "    ���� 'Q�       " Q�.���  � ��8   �� '        "  .���  � ��8  .R��� #��z��#��z��#��z��._��� $
��Auto=�$��Disabled=�.���  �� $
��Auto ��� $��* �.+��� # z # z �� $��Disabled=�$
��Auto=�$��1��.���� 