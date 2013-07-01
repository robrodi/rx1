namespace Rx1
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.
    /// </summary>
    partial class MainWindow
    {
        public readonly StatsProcessorBase viewModel;

        public readonly GameState State = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            this.viewModel = new StatsProcessorObserver(this.State.Kills);
            this.DataContext = viewModel;
        }

        private void PubButton_Checked(object sender, RoutedEventArgs e)
        {
            uint value;
            if (!uint.TryParse(EventsPerSecond.Text, out value))
            {
                MessageBox.Show("Please enter a valid number.");
                return;
            }

            if (PubButton.IsChecked.IsTrue()) this.State.Start((int)value);
            else this.State.Stop();

        }

        public static int generated;
    }
}
