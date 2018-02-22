using System;
using System.Collections.Generic;

namespace ActivityQueue {
    public interface IActivityTicket {
        TicketStatus Status { get; }

        IActivity Activity { get; }

        IActivityTicket Parent { get; }
        IReadOnlyCollection<IActivityTicket> Childs { get; }

        IReadOnlyCollection<IActivityTicket> After { get; }

        IActivityTicket Then(IActivity activity);
        IActivityTicket Then(Func<IActivity, IActivity> action);

        event TicketStatusHandler OnChange;
        event TicketStatusHandler OnRunning;
        event TicketStatusHandler OnRan;
        event TicketStatusHandler OnWaitingChildren;
        event TicketStatusHandler OnClosed;
    }
}