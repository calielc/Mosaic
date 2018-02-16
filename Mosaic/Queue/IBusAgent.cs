namespace Mosaic.Queue
{
    internal interface IBusAgent
    {
        int Id { get; }
        AgentStatuses Status { get; }
        IBusAction Action { get; }
        int Worked { get; }
    }
}