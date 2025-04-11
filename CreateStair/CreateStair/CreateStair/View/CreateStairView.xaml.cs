using System.Windows;
using System.Windows.Controls;

namespace GetDiameterInRevitDesign.GetDiameterAndPatternInRevit.View
{
    /// <summary>
    /// Interaction logic for GetDiameterInRevitView.xaml
    /// </summary>
    public partial class GetDiameterInRevitView : Window
    {
        public GetDiameterInRevitView()
        {
            InitializeComponent();
        }

        private void CbbLang_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbLang.SelectedIndex == 0)
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("pack://application:,,,/GetDiameterInRevitDesign;component/Resources/ResourceString.en.xaml") });
            }
            else
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("pack://application:,,,/GetDiameterInRevitDesign;component/Resources/ResourceString.jp.xaml") });
            }
        }
    }
}
