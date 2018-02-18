namespace Mosaic.Queue
{
    internal interface IActivityAgent
    {
        int Id { get; }

        AgentStatus Status { get; }

        IActivityTicket Ticket { get; }

        int Activities { get; }
    }
}