using Xceed.Wpf.Toolkit.PropertyGrid;

namespace pcsd.ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PropertyGrid_OnPropertyValueChanged(object o, PropertyValueChangedEventArgs e)
        {
            var dataContext = (MainWindowViewModel) DataContext;
            dataContext.Save();
        }
    }
}
