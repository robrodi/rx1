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

        public StatsProcessorObservable(IObservable<MagicEvent> eventStream)
            : base(eventStream)
        {
            subscriptions.Add(this.EventStream.Subscribe(e => this.IncrementEvents()));
            subscriptions.Add(this.EventStream.Where(e => e.Killed != e.Killer).Subscribe(e => this.IncrementKills()));
            subscriptions.Add(this.EventStream.Where(e => e.Killed == e.Killer).Subscribe(e => this.IncrementSuicides()));

            IObservable<IGroupedObservable<int, MagicEvent>> byPlayer = this.EventStream.GroupBy(e => e.Killer);

            byPlayer.Subscribe(player => player.Scan(0, (i, @event) => i + 1).Do(kills => playerKillCounts[player.Key] = kills).Subscribe());
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
            Action action = value.Killed != value.Killer ? (Action)this.IncrementKills : (Action)this.IncrementSuicides;
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