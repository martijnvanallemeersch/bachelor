   M S B A M L   `   `   `  ���� E  ADevRegTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:DevRegTest
DevRegTest      DevRegTest.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     *localclr-namespace:DevRegTest     -     � Title$$  Skyline regression tester tool��$	��620��$
��1095�� � Icon$ SkylineIcon.ico��$	��620��$
��1095��$	��620��$
��1095��.��� .+��� -    ��Name$ 
gbSettings��$��Settings��$
��Left=�$��	10,15,0,0q�$	��Top=�$	��272��$	��343��.��� $��	0,0,-12,0q�$
��Left=�$	��343��.+��� -    ��Name$ label1��$#��Choose configuration to copy:��$
��Left=�$��10,0,0,0q�$	��Top=��� -    ��Name$ 	comboCopy��$��
10,26,23,0q�$��22��$	��Top=��� -   $ label2��$%��Choose initial upgrade package:��$
��Left=�$��	10,53,0,0q�$	��Top=��� -   $ comboVersion��$
��Left=�$��	10,84,0,0q�$	��Top=�$	��310���� -   $ label3��$%��Choose minimum upgrade package:��$
��Left=�$��
10,111,0,0q�$	��Top=��� -   $ comboVersionMinimum��$
��Left=�{�.  $��
31,142,0,0q�$	��Top=�$	��289���� -	    ��Name$ cbVersionMinimum��$
��Left=�$��
10,145,0,0q�$	��Top=�	� -
    	�Name$ cbBinary��{�.  $��Binary option��$
��Left=�$��
31,169,0,0q�$	��Top=� � ToolTip$� �The application will search for the last succeeded version and the first failed version between the initial and the minimum version choosen.��	� -   $ cbUpgradeSequential�� d� 	IsChecked$
 TrueY�$��Sequential option��{�.  $
��Left=�$��181,169,0,0q�$	��Top=�$` ZThe application will run every version, started from the initial one till the minimum one.���� -   $ label4��$$��Copy files from configuration:��$
��Left=�$��
10,189,0,0q�$	��Top=��� -   $ cbCopyFiles��$
��Left=�$��
10,223,0,0q�$	��Top=�$
 TrueY��� -   $ comboConfigs��$
��Left=�$��
31,220,0,0q�$	��Top=�$	��289���� -   $ 	gbScripts��$��Scripts��$��
358,15,0,0q�$	��Top=�$	��532��$
��Left=�$	��714��.��� $��0,0,-2,0q�.+��� -   $ label5��$��Email address:��$
��Left=�$��	10,10,0,0q�$	��Top=��� -   	 ��Name$	 tbEmailAddress��$
��Left=�$��23��$��
100,13,0,0q�
 �� TextWrapping$

 Wrap=�$	��Top=�$	��594���� -   $ label6��$��Search:��$
��Left=�$��	10,50,0,0q�$	��Top=�$��25��   DevRegTest.TreeListView  -     Name$ MultiColomTree��   AllowsColumnReorder . $��10,90,206,230q�
�����w�$��1q�����  �� ItemsSource �� *
SubItems+   Columns �� $��
ScriptName�� �� Width$	 200�� �� CellTemplate �� .R���  �� Orientation$ 
Horizontal=�.+�%   DevRegTest.TreeListViewExpander  �� ���� *Name+�� $��Author��$	 100�� �� .R��� $��Center=����� *Author+�� $��Process��$	 100�� �� .R��� $��Center=����� *	Process+�� $��Version��$	 100�� �� .R��� $��Center=����� *	Version+�� $��Extra��$	 100�� �� .R��� $��Center=����� *Extra+�� -   $ groupBox��$��Settings��$
��Left=�$��
503,85,0,0q�$	��Top=�$	��105��$	��191��.��� $��0,0,-2,0q�.+�	� -   $ cbSequentieel��$��
Sequential��$
 TrueY�$
��Left=�$��	10,17,0,0q�$	��Top=�	� -   $ 
cbParallel��$��Parallel��$
��Left=�$��	10,47,0,0q�$	��Top=��� -   $ cbMail��$
��Mail��$
 TrueY�$
��Left=�$��
110,17,0,0q�$	��Top=��� -   $ cbAll��$	��All��$
��Left=�$��
110,47,0,0q�$	��Top=��� -    ��Name$ bRunningScripts��$��Start Running��$
��Left=�$��503,195,0,0q�$	��Top=�$	��191��$��29���� -   $	 	tbConsole��" �� VerticalScrollBarVisibility$ Visible=�$
��Left=�$	��105��$��
10,285,0,0q�$

 Wrap=�$	��Top=�$	��684���� -    ��Name$ 
rtbConsole�� �� IsDocumentEnabled . $
��Left=�$	��105��$��
10,395,0,0q�$	��Top=�$	��684���� -   $	 tbSearch��$
��Left=�$��23��$��	63,52,0,0q�$

 Wrap=�$�� ��$	��Top=�$	��506���� -   $ comboSearch��$
��Left=�$��
574,52,0,0q�$	��Top=�$	��120��$��23���� -   $ bSaveSettings��$��Save Settings��$
��Left=�$��
10,292,0,0q�$	��Top=�$	��172��$��28���� $��Bottom=�$��30��$��Bottom=�.@��� .���� $��0,-5,0,0q�$��Status programma:���� .���� -   $��statusProgramStrip��$��
-12,-5,0,0q��� $��Advanced Options:��$
��Left=�$��
10,520,0,0q�$	��Top=�$	��115���� -    $ AdvancedRegexOpen��$
��Left=�$��125,525,0,0q�$	��Top=�$��20��$��20��.���� .+��� $��ArrowDown.png��$��10��$��10��$��Center=�$��Center=��� -!   $ AdvancedRegexClose��$
��Left=�$��150,525,0,0q�$	��Top=�$��20��$��20��.���� .+��� $��ArrowUp.png��$��10��$��10��$��Center=�$��Center=�� -"    �Name$ MyPopupRegex�� � 	StaysOpen .  $	��905��$	��200��$��Bottom=� � VerticalOffset$ 37�� � HorizontalOffset$
 -125�� � AllowsTransparency . $
��Fade=���� -#   $��	UpperGrid�� � ColumnDefinitions �� $	��455 ��� $	��455 �.+�� $��0�� � RowDefinitions �� $	��175 ��� $��25 �.+�&   System.Windows.Controls.DataGrid  -$   $��dataGridRegexes��$��0��   HeadersVisibility$	 All=�$	��170��$	��455��$   HorizontalScrollBarVisibility$ Visible=�"   VerticalScrollBarVisibility$ Visible=�    AutoGenerateColumns  .  !   CanUserAddRows! .  ���� *
RegexClass+"   Columns" 4  .System.Windows.Controls.DataGridTemplateColumn  ,  &System.Windows.Controls.DataGridColumn#   Header$# Check��$   Width5  /System.Windows.Controls.DataGridLengthConverter$$ 50 %   CellTemplate% �� .R��� ���� *Check+  $# Files��$	$ 100 % �� .R��� ���� *Files+  $# 
Collection��$	$ 100 % �� .R��� ���� *
Collection+  $# Regex��$	$ 100 % �� .R��� ���� *Regex+  $# Output��$	$ 100 % �� .R��� ���� *Output+� $��1�� �� $	��225 ��� $	��225 �.+��� -%   $��0��$��Center=�$
��Left=�$	��100��$��20��$��Remove Regex���� -&   $��1��$��Center=�$��
-120,0,0,0q�$
��Left=�$��20��$��	New Regex��$	��100��� $��1�� �� $��25 ��� $	��125 ��� $��25 ��� $��25 � �� $	��105 ��� $	��345 �.+��� $��0��$��0��$��Center=�$��Center=�$��50��$��25��$��Regex:���� $��0��$��1��$��Center=�$��Center=�$��50��$��25��$��File(s):���� $��0��$��2��$��Center=�$��Center=�$��50��$��25��$��Output:���� -'   $��btnAdd��$��0��$��3��$��Center=�$��Center=�$	��100��$��20��$	��Add���� -(   $��tbRegex��$��1��$��0��$��Center=�$��Center=�$	��340��$��20��R� -)   $!��treeViewDirectoriesScripts2��$��1��$��1��$��Center=�$	��120��$��Center=�$	��340���� -*   $��btnChooseFile��$��1��$��2��$��Center=�$��Center=�$	��340��$��20��$	��...���� -+   $��btnClear��$��1��$��3��$��
-240,0,0,0q�$��Center=�$��Center=�$	��100��$��20��$��Clear��