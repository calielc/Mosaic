namespace Mosaic.Queue {
    internal enum AgentStatuses {
        Ready = 0,
        Working = 10,
        Worked = 11,
        Idle = 20,
        Shutdown = 31
    }
}