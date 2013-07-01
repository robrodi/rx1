namespace Rx1
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;

    public class ScanningCounter : Dictionary<int, int>, IDisposable
    {
        private IDisposable outer, inner;
        public ScanningCounter(IObservable<MagicEvent> eventStream, Func<MagicEvent, int> keySelector)
        {
            outer = eventStream.GroupBy(keySelector).Subscribe(whatever => inner = whatever.Scan(0, (i, @event) => i + 1).Do(kills => this[whatever.Key] = kills).Subscribe());
        }

        public void Dispose()
        {
            if (outer != null) outer.Dispose();
            if (inner != null) inner.Dispose();
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
            playerKillCounts = new ScanningCounter(eventStream, e => e.Killer);
            playerDeathCounts = new ScanningCounter(eventStream, e => e.Victim);
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
}