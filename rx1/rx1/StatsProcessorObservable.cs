namespace Rx1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reactive.Linq;

    public class Counter : Dictionary<int, int>
    {
        public Counter(IObservable<MagicEvent> eventStream, Func<MagicEvent, int> keySelector)
        {
            eventStream.GroupBy(keySelector).Subscribe(whatever => whatever.Scan(0, (i, @event) => i + 1).Do(kills => this[whatever.Key] = kills).Subscribe());
        }
    }

    public class StatsProcessorObservable : StatsProcessorBase, IDisposable
    {
        private readonly ICollection<IDisposable> subscriptions = new List<IDisposable>();

        private readonly IDictionary<int, int> playerKillCounts;
        private readonly IDictionary<int, int> playerDeathCounts;

        public StatsProcessorObservable(IObservable<MagicEvent> eventStream)
            : base(eventStream)
        {
            this.EventStream.Subscribe(e => this.IncrementEvents());
            
            // Sum Kills
            this.EventStream.Where(e => e.Victim != e.Killer).Subscribe(e => this.IncrementKills());
            this.EventStream.Where(e => e.Victim == e.Killer).Subscribe(e => this.IncrementSuicides());

            // Sum Kills / player
            playerKillCounts = new Counter(eventStream, e => e.Killer);
            playerDeathCounts = new Counter(eventStream, e => e.Victim);
        }

        public IDictionary<int, int> PlayerKillCounts
        {
            get
            {
                return this.playerKillCounts;
            }
        }

        public IDictionary<int, int> PlayerDeathCounts
        {
            get
            {
                return this.playerDeathCounts;
            }
        }

        public void Dispose()
        {
            foreach (var subscription in subscriptions)
                subscription.Dispose();
        }
    }

    [Obsolete]
    public class StatsProcessorObserver : StatsProcessorBase, IObserver<MagicEvent>
    {
        public StatsProcessorObserver(IObservable<MagicEvent> eventStream)
            : base(eventStream)
        {
            this.EventStream.Subscribe(this);
        }
        public void OnNext(MagicEvent value)
        {
            Action action = value.Victim != value.Killer ? (Action)this.IncrementKills : (Action)this.IncrementSuicides;
            action.Invoke();
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
            Debug.WriteLine("!");
        }
    }
}