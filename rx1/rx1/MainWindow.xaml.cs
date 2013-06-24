namespace Rx1
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Windows;

    public struct MagicEvent
    {
        private readonly DateTime sent;

        private readonly int type;

        private readonly string killer;

        private readonly string killed;

        public MagicEvent(int type, string killer, string killed)
            : this(DateTime.UtcNow, type, killer, killed)
        {
        }

        public MagicEvent(DateTime sent, int type, string killer, string killed)
        {
            this.sent = sent;
            this.type = type;
            this.killer = killer;
            this.killed = killed;
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow
    {
        private IDisposable subscription;
        private IObservable<MagicEvent> push;
        private static readonly Random R = new Random(12345);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PubButton_Checked(object sender, RoutedEventArgs e)
        {
            this.push = this.PubButton.IsChecked.IsTrue()
                            ? Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(5)).Select(GenerateEvent)
                            : Observable.Timer(TimeSpan.MaxValue).Select(GenerateEvent);
        }

        private static readonly string[] Users = new[] { "Rob", "Jeff", "Jason", "Mike", "Scott", "Eduard" };


        public static MagicEvent GenerateEvent(long id)
        {
            Func<string> randomUser = () => Users[R.Next(Users.Length)];
            return new MagicEvent((int)id, randomUser(), randomUser());
        }

        private void SubButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            if (this.SubButton.IsChecked.IsTrue() && this.push != null)
            {
                this.subscription = this.push.Subscribe(new StatsProcessor().ProcessEvent);
            }
        }
    }

    public class StatsProcessor
    {
        public void ProcessEvent(MagicEvent e)
        {
            Debug.WriteLine("Event");
        }
    }
    
    internal static class Extensions
    {
        internal static bool IsTrue(this bool? value)
        {
            return value.HasValue && value.Value;
        }
    }
}
