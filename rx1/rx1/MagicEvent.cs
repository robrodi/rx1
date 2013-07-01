namespace Rx1
{
    using System;

    public struct MagicEvent
    {
        private readonly DateTime sent;

        private readonly int type;

        private readonly int killer;

        private readonly int killed;

        public MagicEvent(int type, int killer, int killed)
            : this(DateTime.UtcNow, type, killer, killed)
        {
        }

        public MagicEvent(DateTime sent, int type, int killer, int killed)
        {
            this.sent = sent;
            this.type = type;
            this.killer = killer;
            this.killed = killed;
        }

        public int Victim
        {
            get
            {
                return this.killed;
            }
        }

        public int Killer
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
}