   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5      e aParSurf - Parametric Surface Visualization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:ParSurfParSurf 5      %   ParSurf.BrushPickerComboControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      6sys&clr-namespace:System;assembly=mscorlib     5      4converterNamespaceclr-namespace:ParSurf     5      $
��Auto��5      $
��Auto��6   $��50��6      � 	Resources  5         bruhsesTypeDataProvider S� 
ObjectType   System.Type S� 
MethodName S� MethodParameters  brushNameDataProvider S� ObjectInstance*  $ParSurf.PropertyInfoToBrushConverter   propertyInfoToBrushConverter��%/  &	        &	 �     7   &	 �     S� 5   
   "  6=   $ GetType��6\    5	      ��5
      uqSystem.Windows.Media.Brushes, PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35��5      5      S� 5   
   8   $ GetProperties��6u     5      68   5      .��5      �� -   5        $��brushSelectionBox��5       �� 
IsReadOnly .  �� 
IsEditable .  $��1��6[  ��6M   ��  �� Source# [  �� SelectedItem 6�   �� 	 �� RelativeSource	 �� 
 �� Mode$
 FindAncestor=� �� AncestorType" >� �� Path$ SelectedBrush� �� Mode$ TwoWay=� �� 	Converter# [ ��5   
   �� 5      .R�5      ��  �� Orientation$ 
Horizontal=�6   .+�5      �� $��25��6   $��25��6)   $��0,3,0,3q�64   $��1q�6F   
���   ���6n   �� $
 Name�$ OneWay=��� 5      $��Center=�6    $
��Left=�6;   $��10,0,0,0q�6V   ��6h   �� $
 Name�$ OneWay=�5      5      5      5      