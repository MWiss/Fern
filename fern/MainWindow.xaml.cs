using System.Windows;

namespace Fern
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FernDraw f = new FernDraw(depthSlider.Value, angleSlider.Value, growthSlider.Value, frondsSlider.Value, canvas);
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            FernDraw f = new FernDraw(depthSlider.Value, angleSlider.Value, growthSlider.Value, frondsSlider.Value, canvas);
        }
    }
}
