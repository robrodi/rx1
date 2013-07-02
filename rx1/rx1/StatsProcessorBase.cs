namespace Rx1
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    using rx1.Annotations;

    public abstract class StatsProcessorBase : INotifyPropertyChanged
    {
        protected readonly IObservable<MagicEvent> EventStream;

        private int events, eventsPerSecond;

        protected StatsProcessorBase(IObservable<MagicEvent> eventStream)
        {
            this.EventStream = eventStream;
        }
        
        public int EventsPerSecond
        {
            get { return this.eventsPerSecond; }
            set
            {
                if (this.eventsPerSecond == value) return;

                this.eventsPerSecond = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("EventsPerSecond"));
            }
        }

        public int TotalEvents { get { return this.events; } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void IncrementEvents()
        {
            Interlocked.Increment(ref this.events);
        }
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}