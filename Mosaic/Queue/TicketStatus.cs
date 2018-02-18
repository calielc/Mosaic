namespace Mosaic.Queue {
    public enum TicketStatus {
        Queued,
        Running,
        Ran,
        WaitingChildren,
        Closed,
    }
}