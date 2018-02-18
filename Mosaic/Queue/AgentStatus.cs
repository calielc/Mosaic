namespace Mosaic.Queue {
    internal enum AgentStatus {
        Ready = 0,
        Working = 10,
        Worked = 11,
        Idle = 20,
        Shutdown = 31
    }
}