namespace Rx1
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
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
    partial class MainWindow
    { 
        public class GameState
        {
            private Timer timer;
            private static readonly Func<string> RandomUser = () => Users[R.Next(Users.Length)];
            public delegate void MagicEventHandler(object sender, MagicEvent e);
            public event MagicEventHandler OnKill;

            public GameState()
            {
                Kills = Observable.FromEventPattern<MagicEvent>(this, "OnKill").Select(e => e.EventArgs);
            }
            
            private void GenerateAndFireKill()
            {
                if (this.OnKill != null)
                    OnKill(this, GenerateEvent(DateTime.UtcNow.ToFileTime()));
            }

            public void Start(int rps)
            {
                timer = new Timer(_ => this.GenerateAndFireKill(), null, 0, 1000 / rps);
            }
            
            private static MagicEvent GenerateEvent(long id)
            {
                generated++;
                return new MagicEvent((int)id, RandomUser(), RandomUser());
            }

            public void Stop()
            {
                timer.Dispose();
            }

            protected virtual void OnOnKill(MagicEvent e)
            {
                var handler = this.OnKill;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public IObservable<MagicEvent> Kills;
        }

        //private readonly IObservable<MagicEvent> push;
        private static readonly Random R = new Random(12345);
        private static readonly string[] Users = new[] { "Rob", "Jeff", "Jason", "Mike", "Scott", "Eduard" };
        private static bool _isPushEnabled;

        public readonly StatsProcessor viewModel;

        public readonly GameState state = new GameState();

        public MainWindow()
        {
            InitializeComponent();

            //var eventStream = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(5)).Where(_ => _isPushEnabled).Select(this.GenerateEvent);
            this.viewModel = new StatsProcessor(state.Kills);
            this.DataContext = viewModel;
        }

        private void PubButton_Checked(object sender, RoutedEventArgs e)
        {
            //_isPushEnabled = !_isPushEnabled;
            uint value;
            if (!uint.TryParse(EventsPerSecond.Text, out value))
            {
                MessageBox.Show("Please enter a valid number.");
                return;
            }

            if (PubButton.IsChecked.IsTrue()) state.Start((int)value);
            else state.Stop();

        }

        public static int generated;
    }

    public class StatsProcessor : INotifyPropertyChanged, IDisposable, IObserver<MagicEvent>
    {
        private int _eventsPerSecond;

        public int EventsPerSecond
        {
            get
            {
                return _eventsPerSecond;
            }
            set
            {
                if (_eventsPerSecond == value) return;

                _eventsPerSecond = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("EventsPerSecond"));
            }
        }
        private readonly IObservable<MagicEvent> _eventStream;

        public StatsProcessor(IObservable<MagicEvent> eventStream)
        {
            this._eventStream = eventStream;
            this._eventStream.Subscribe(e => this.IncrementEvents());
            this._eventStream.Where(e => e.Killed != e.Killer).Subscribe(e => this.IncrementKills());
            this._eventStream.Where(e => e.Killed == e.Killer).Subscribe(e => this.IncrementSuicides());
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
    
    public static class Extensions
    {
        public static bool IsTrue(this bool? value)
        {
            return value.HasValue && value.Value;
        }
    }
}
