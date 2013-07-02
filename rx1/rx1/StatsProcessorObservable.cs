namespace Rx1
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;

    public class ScanningCounter<TKey> : Dictionary<TKey, int>, IDisposable
    {
        private IDisposable outer, inner;
        public ScanningCounter(IObservable<MagicEvent> eventStream, Func<MagicEvent, TKey> keySelector)
        {
            Func<int, MagicEvent, int> countEvents = (i, @event) => i + 1;
            outer = eventStream.GroupBy(keySelector).Subscribe(group => inner = group.Scan(0, countEvents).Do(kills => this[group.Key] = kills).Subscribe());
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

        public StatsProcessorObservable(IObservable<MagicEvent> eventStream)
            : base(eventStream)
        {
            this.EventStream.Subscribe(e => this.IncrementEvents());
            
            // Sum Kills
            this.killsAndSuicides = new ScanningCounter<bool>(eventStream, e => e.Killer != e.Victim);

            // Sum Kills / player
            playerKillCounts = new ScanningCounter<int>(eventStream, e => e.Killer);
            playerDeathCounts = new ScanningCounter<int>(eventStream, e => e.Victim);
        }

        private readonly IDictionary<int, int> playerKillCounts;
        public IDictionary<int, int> PlayerKillCounts
        {
            get
            {
                return this.playerKillCounts;
            }
        }

        private readonly IDictionary<int, int> playerDeathCounts;
        public IDictionary<int, int> PlayerDeathCounts
        {
            get
            {
                return this.playerDeathCounts;
            }
        }

        private readonly IDictionary<bool, int> killsAndSuicides;
        public int Kills
        {
            get
            {
                return killsAndSuicides[true];
            }
        }
        public int Suicides { get { return killsAndSuicides[false]; } }

        public void Dispose()
        {
            foreach (var subscription in subscriptions)
                subscription.Dispose();
        }
    }
}