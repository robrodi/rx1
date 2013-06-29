namespace Rx1
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;

    using rx1.Annotations;

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

        public string Killed
        {
            get
            {
                return this.killed;
            }
        }

        public string Killer
        {
            get
            {
                return this.killer;
            }
        }

        public int Type
        {
            get
            {
                return this.type;
            }
        }

        public DateTime Sent
        {
            get
            {
                return this.sent;
            }
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow
    {
        //private readonly IObservable<MagicEvent> push;
        private static readonly Random R = new Random(12345);
        private static readonly string[] Users = new[] { "Rob", "Jeff", "Jason", "Mike", "Scott", "Eduard" };
        private static bool _isPushEnabled;

        internal readonly StatsProcessor viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var eventStream = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(100)).Where(_ => _isPushEnabled).Select(i => GenerateEvent(i));
            this.viewModel = new StatsProcessor(eventStream);
            eventStream.Subscribe(viewModel);
            this.DataContext = viewModel;
        }

        private void PubButton_Checked(object sender, RoutedEventArgs e)
        {
            _isPushEnabled = !_isPushEnabled;
        }

        public static int generated;
        public MagicEvent GenerateEvent(long id)
        {
            generated++;
            Dispatcher.Invoke(() => { this.Generated.Content = generated.ToString(); });
            Func<string> randomUser = () => Users[R.Next(Users.Length)];
            return new MagicEvent((int)id, randomUser(), randomUser());
        }

        private void SubButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (this.subscription != null)
            //{
            //    this.subscription.Dispose();
            //    this.subscription = null;
            //}

            //if (this.SubButton.IsChecked.IsTrue() && this.push != null)
            //{
            //    //this.subscription = this.push.Subscribe(viewModel.ProcessEvent);
            //}
        }
    }

    public class StatsProcessor : INotifyPropertyChanged, IDisposable, IObserver<MagicEvent>
    {
        private readonly IObservable<MagicEvent> _eventStream;

        public StatsProcessor(IObservable<MagicEvent> eventStream)
        {
            //this._eventStream = eventStream;
            //this._eventStream.Subscribe(e => this.IncrementEvents());
            //this._eventStream.Where(e => e.Killed != e.Killer).Subscribe(e => this.IncrementKills());
            //this._eventStream.Where(e => e.Killed == e.Killer).Subscribe(e => this.IncrementSuicides());
        }

        private void IncrementEvents()
        {
            int eventCount = Interlocked.Increment(ref _events);
            if (eventCount % 100 == 0)
                Debug.Write(eventCount);
        }

        private int _kills, _suicides, _events;

        private void IncrementKills()
        {
            Interlocked.Increment(ref _kills);
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Kills"));
        }
        private void IncrementSuicides()
        {
            Interlocked.Increment(ref _suicides);
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Suicides"));
        }

        public int Kills
        {
            get { return _kills; }
        }

        public int Suicides
        {
            get
            {
                return _suicides;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Dispose()
        {
        }

        public void OnNext(MagicEvent value)
        {
            this.IncrementKills();
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
            Debug.WriteLine("!");
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
