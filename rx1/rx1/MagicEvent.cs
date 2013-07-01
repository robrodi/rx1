namespace Rx1
{
    using System;

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
}