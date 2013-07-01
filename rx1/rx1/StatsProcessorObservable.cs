namespace Rx1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reactive.Linq;

    public class StatsProcessorObservable : StatsProcessorBase, IDisposable
    {
        private readonly ICollection<IDisposable> subscriptions = new List<IDisposable>();
        private readonly IDictionary<int, int> playerKillCounts = new Dictionary<int, int>();
        private readonly IDictionary<int, int> playerDeathCounts = new Dictionary<int, int>();

        public StatsProcessorObservable(IObservable<MagicEvent> eventStream)
            : base(eventStream)
        {
            this.EventStream.Subscribe(e => this.IncrementEvents());
            this.EventStream.Where(e => e.Victim != e.Killer).Subscribe(e => this.IncrementKills());
            this.EventStream.Where(e => e.Victim == e.Killer).Subscribe(e => this.IncrementSuicides());

            IObservable<IGroupedObservable<int, MagicEvent>> byKiller = this.EventStream.GroupBy(e => e.Killer);
            byKiller.Subscribe(player => player.Scan(0, (i, @event) => i + 1).Do(kills => this.PlayerKillCounts[player.Key] = kills).Subscribe());

            IObservable<IGroupedObservable<int, MagicEvent>> byVictim = this.EventStream.GroupBy(e => e.Victim);
            byVictim.Subscribe(player => player.Scan(0, (i, @event) => i + 1).Do(kills => this.PlayerDeathCounts[player.Key] = kills).Subscribe());
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