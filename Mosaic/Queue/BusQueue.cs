using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Queue {
    internal sealed class BusQueue : IDisposable {
        private static readonly TimeSpan TimeToWait = TimeSpan.FromSeconds(2);

        private readonly BlockingCollection<IBusAction> _queue;
        private IReadOnlyCollection<Agent> _agents;
        private IReadOnlyCollection<Task> _tasks;
        private int _workers = Environment.ProcessorCount;
        private bool _isWaiting;

        public BusQueue() {
            _queue = new BlockingCollection<IBusAction>();
        }

        public int Remaining => _queue.Count;

        public int Workers {
            get => _workers;
            set => _workers = Math.Max(value, 1);
        }

        public void Enqueue(IBusAction action) {
            _queue.Add(action);
        }

        public BusQueue Run() {
            _agents = new ConcurrentBag<Agent>(Enumerable.Range(0, Workers).Select(id => new Agent(id, this)));

            _isWaiting = false;
            _tasks = _agents.Select(agent => agent.Run()).ToArray();

            return this;
        }

        public async Task Wait() {
            _isWaiting = true;

            await Task.WhenAll(_tasks);
        }

        public async Task RunAndWait() => await Run().Wait();

        public void Dispose() {
            _queue.Dispose();
        }

        public event AgentStatusHandler AgentChange;
        public event AgentStatusHandler AgentReady;
        public event AgentStatusHandler AgentWorking;
        public event AgentStatusHandler AgentWorked;
        public event AgentStatusHandler AgentIdle;
        public event AgentStatusHandler AgentShutdown;

        internal delegate void AgentStatusHandler(object sender, IBusAgent agent);

        private void OnAgentChange(Agent agent) {
            AgentChange?.Invoke(this, agent);
            switch (agent.Status) {
                case AgentStatuses.Ready:
                    AgentReady?.Invoke(this, agent);
                    break;
                case AgentStatuses.Working:
                    AgentWorking?.Invoke(this, agent);
                    break;
                case AgentStatuses.Worked:
                    AgentWorked?.Invoke(this, agent);
                    break;
                case AgentStatuses.Idle:
                    AgentIdle?.Invoke(this, agent);
                    break;
                case AgentStatuses.Shutdown:
                    AgentShutdown?.Invoke(this, agent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsAllJobDone() {
            if (_agents.Any(agent => agent.Status <= AgentStatuses.Worked)) {
                return false;
            }

            if (_queue.Count > 0) {
                return false;
            }

            return _isWaiting;
        }

        [DebuggerDisplay("Id: {Id}, Status: {Status}")]
        private sealed class Agent : IBusAgent {
            private readonly BusQueue _owner;

            public Agent(int id, BusQueue owner) {
                Id = id;
                _owner = owner;
            }

            public int Id { get; }

            public AgentStatuses Status { get; private set; } = AgentStatuses.Ready;

            public IBusAction Action { get; private set; }

            public int Worked { get; private set; }

            public Task Run() => Task.Run(async () => {
                ChangeStatus(AgentStatuses.Ready);
                do {
                    while (_owner._queue.TryTake(out var action, TimeToWait)) {
                        ChangeStatus(AgentStatuses.Working, action);

                        await action.Run();

                        Worked += 1;
                        ChangeStatus(AgentStatuses.Worked, action);
                    }

                    if (_owner.IsAllJobDone()) {
                        break;
                    }

                    ChangeStatus(AgentStatuses.Idle);
                    Thread.Sleep(TimeToWait);
                } while (true);
                ChangeStatus(AgentStatuses.Shutdown);
            });

            private void ChangeStatus(AgentStatuses status, IBusAction action = default) {
                Status = status;
                Action = action;
                _owner.OnAgentChange(this);
            }
        }
    }
}