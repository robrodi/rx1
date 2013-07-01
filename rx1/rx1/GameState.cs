namespace Rx1
{
    using System;
    using System.Reactive.Linq;
    using System.Threading;

    public class GameState
    {
        private static readonly Random R = new Random(12345);
        private static readonly string[] Users = new[] { "Rob", "Jeff", "Jason", "Mike", "Scott", "Eduard" };

        private Timer timer;
        private static readonly Func<string> RandomUser = () => Users[R.Next(Users.Length)];
        public delegate void MagicEventHandler(object sender, MagicEvent e);
        public event MagicEventHandler OnKill;
        public IObservable<MagicEvent> Kills { get; set; }

        public GameState()
        {
            this.Kills = Observable.FromEventPattern<MagicEvent>(this, "OnKill").Select(e => e.EventArgs);
        }

        private void GenerateAndFireKill()
        {
            if (this.OnKill != null)
                this.OnKill(this, GenerateEvent(DateTime.UtcNow.ToFileTime()));
        }

        public void Start(int rps)
        {
            this.timer = new Timer(_ => this.GenerateAndFireKill(), null, 0, 1000 / rps);
        }

        public static MagicEvent GenerateEvent(long id)
        {
            return new MagicEvent((int)id, RandomUser(), RandomUser());
        }

        public void Stop()
        {
            this.timer.Dispose();
        }

        protected virtual void OnOnKill(MagicEvent e)
        {
            var handler = this.OnKill;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}