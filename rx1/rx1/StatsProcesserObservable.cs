namespace Rx1
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Linq;

    public class StatsProcesserObservable : StatsProcessorBase
    {
        public StatsProcesserObservable(IObservable<MagicEvent> eventStream)
            : base(eventStream)
        {
            this.EventStream.Subscribe(e => this.IncrementEvents());
            this.EventStream.Where(e => e.Killed != e.Killer).Subscribe(e => this.IncrementKills());
            this.EventStream.Where(e => e.Killed == e.Killer).Subscribe(e => this.IncrementSuicides());
        }
    }

    public class StatsProcesserObserver : StatsProcessorBase, IObserver<MagicEvent>
    {
        public StatsProcesserObserver(IObservable<MagicEvent> eventStream)
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