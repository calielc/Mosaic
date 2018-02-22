namespace ActivityQueue {
    public enum TicketStatus {
        Queued,
        Running,
        Ran,
        WaitingChildren,
        Closed,
    }
}