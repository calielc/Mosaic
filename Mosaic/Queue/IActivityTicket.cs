using System.Collections.Generic;

namespace Mosaic.Queue {
    internal interface IActivityTicket {
        TicketStatus Status { get; }

        IActivity Activity { get; }

        IActivityTicket Parent { get; }
        IReadOnlyCollection<IActivityTicket> Childs { get; }

        IReadOnlyCollection<IActivityTicket> After { get; }

        IActivityTicket Then(IActivity activity);
    }
}