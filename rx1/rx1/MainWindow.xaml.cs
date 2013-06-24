namespace Rx1
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
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
            this.viewModel = new StatsProcessor(Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(5)).Where(_ => _isPushEnabled).Select(GenerateEvent));
            this.DataContext = viewModel;
        }

        private void PubButton_Checked(object sender, RoutedEventArgs e)
        {
            _isPushEnabled = !_isPushEnabled;
        }

        public static MagicEvent GenerateEvent(long id)
        {
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

    public class StatsProcessor : INotifyPropertyChanged, IDisposable
    {
        private readonly IObservable<MagicEvent> _eventStream;

        public StatsProcessor(IObservable<MagicEvent> eventStream)
        {
            this._eventStream = eventStream;
            this._eventStream.Where(e => e.Killed != e.Killer).Subscribe(e => { Kills++; });
            this._eventStream.Where(e => e.Killed == e.Killer).Subscribe(e => { Suicides++; });
        }

        private uint _kills;
        private uint _suicides;
        public uint Kills
        {
            get { return _kills; }
            set
            {
                _kills = value; 
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Kills"));
            }
        }
        public uint Suicides
        {
            get
            {
                return _suicides;
            }
            set
            {
                _suicides = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Suicides"));
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
    }
    
    internal static class Extensions
    {
        internal static bool IsTrue(this bool? value)
        {
            return value.HasValue && value.Value;
        }
    }
}
