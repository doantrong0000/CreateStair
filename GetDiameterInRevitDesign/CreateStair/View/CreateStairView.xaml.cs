using System.Windows;
using System.Windows.Controls;

namespace CreateStairDesign.CreateStair.View
{
    /// <summary>
    /// Interaction logic for CreateStairView.xaml
    /// </summary>
    public partial class CreateStairView : Window
    {
        public CreateStairView()
        {
            InitializeComponent();
        }

        private void CbbLang_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbLang.SelectedIndex == 0)
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("pack://application:,,,/CreateStairDesign;component/Resources/ResourceString.en.xaml") });
            }
            else
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri("pack://application:,,,/CreateStairDesign;component/Resources/ResourceString.jp.xaml") });
            }
        }
    }
}
