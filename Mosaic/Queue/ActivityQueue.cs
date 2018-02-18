using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Queue {
    internal sealed class ActivityQueue : IDisposable {
        private static readonly TimeSpan TimeToWait = TimeSpan.FromSeconds(2);

        private readonly BlockingCollection<Ticket> _queue = new BlockingCollection<Ticket>();
        private readonly ConcurrentBag<Ticket> _tickets = new ConcurrentBag<Ticket>();
        private IReadOnlyCollection<Agent> _agents;
        private IReadOnlyCollection<Task> _tasks;
        private int _workers = Environment.ProcessorCount;
        private bool _isWaiting;

        public int Workers {
            get => _workers;
            set => _workers = Math.Max(value, 1);
        }

        public IActivityTicket Add(IActivity activity) {
            var ticket = new Ticket(this, activity);
            _queue.Add(ticket);

            return ticket;
        }

        public IActivityTicket AddSubtask(IActivity parent, IActivity activity) {
            var parentTicket = _tickets.SingleOrDefault(item => item.Activity == parent) ?? throw new ArgumentNullException(nameof(parent));

            var ticket = new Ticket(this, parentTicket, activity);
            _queue.Add(ticket);

            return ticket;
        }

        public ActivityQueue Run() {
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

        public void Dispose() => _queue.Dispose();

        public event AgentStatusHandler AgentChange;
        public event AgentStatusHandler AgentReady;
        public event AgentStatusHandler AgentWorking;
        public event AgentStatusHandler AgentWorked;
        public event AgentStatusHandler AgentIdle;
        public event AgentStatusHandler AgentShutdown;
        public delegate void AgentStatusHandler(ActivityQueue sender, IActivityAgent agent);

        public event TicketStatusHandler TicketChange;
        public event TicketStatusHandler TicketRunning;
        public event TicketStatusHandler TicketRan;
        public event TicketStatusHandler TicketWaitingChild;
        public event TicketStatusHandler TicketClosed;
        internal delegate void TicketStatusHandler(ActivityQueue sender, IActivityTicket ticket);

        private bool IsAllJobDone() {
            if (_agents.Any(agent => agent.Status <= AgentStatus.Worked) || _queue.Any()) {
                return false;
            }

            return _isWaiting;
        }

        private bool TryTake(out Ticket registry) => _queue.TryTake(out registry, TimeToWait);

        private void FireAgentChange(Agent agent) {
            AgentChange?.Invoke(this, agent);
            switch (agent.Status) {
                case AgentStatus.Ready:
                    AgentReady?.Invoke(this, agent);
                    break;
                case AgentStatus.Working:
                    AgentWorking?.Invoke(this, agent);
                    break;
                case AgentStatus.Worked:
                    AgentWorked?.Invoke(this, agent);
                    break;
                case AgentStatus.Idle:
                    AgentIdle?.Invoke(this, agent);
                    break;
                case AgentStatus.Shutdown:
                    AgentShutdown?.Invoke(this, agent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FireActivityChange(Ticket ticket) {
            TicketChange?.Invoke(this, ticket);
            switch (ticket.Status) {
                case TicketStatus.Running:
                    TicketRunning?.Invoke(this, ticket);
                    break;
                case TicketStatus.Ran:
                    TicketRan?.Invoke(this, ticket);
                    break;
                case TicketStatus.WaitingChildren:
                    TicketWaitingChild?.Invoke(this, ticket);
                    break;
                case TicketStatus.Closed:
                    TicketClosed?.Invoke(this, ticket);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (ticket.Status == TicketStatus.Closed) {
                foreach (var after in ticket.After) {
                    _queue.Add(after);
                }
            }
        }

        [DebuggerDisplay("Id: {Id}, Status: {Status}")]
        private sealed class Agent : IActivityAgent {
            private readonly ActivityQueue _owner;

            public Agent(int id, ActivityQueue owner) {
                Id = id;
                _owner = owner;
            }

            public int Id { get; }

            public AgentStatus Status { get; private set; } = AgentStatus.Ready;

            public Ticket Ticket { get; private set; }

            public int Activities { get; private set; }

            IActivityTicket IActivityAgent.Ticket => Ticket;

            public Task Run() => Task.Run(async () => {
                ChangeStatus(AgentStatus.Ready);
                do {
                    while (_owner.TryTake(out var ticket)) {
                        ChangeStatus(AgentStatus.Working, ticket);

                        await ticket.Run();

                        Activities += 1;
                        ChangeStatus(AgentStatus.Worked, ticket);
                    }

                    if (_owner.IsAllJobDone()) {
                        break;
                    }

                    ChangeStatus(AgentStatus.Idle);
                    Thread.Sleep(TimeToWait);
                } while (true);
                ChangeStatus(AgentStatus.Shutdown);
            });

            private void ChangeStatus(AgentStatus status, Ticket ticket = default) {
                Status = status;
                Ticket = ticket;
                _owner.FireAgentChange(this);
            }
        }

        [DebuggerDisplay("Status: {Status}, Activity: {Activity}")]
        private sealed class Ticket : IActivityTicket {
            private readonly ActivityQueue _owner;
            private readonly Ticket _parent;
            private readonly ConcurrentBag<Ticket> _childs = new ConcurrentBag<Ticket>();
            private readonly ConcurrentBag<Ticket> _after = new ConcurrentBag<Ticket>();

            internal Ticket(ActivityQueue owner, IActivity activity) {
                _owner = owner;
                _owner._tickets.Add(this);

                Activity = activity;
            }

            internal Ticket(ActivityQueue owner, Ticket parent, IActivity activity) {
                _owner = owner;
                _owner._tickets.Add(this);

                Activity = activity;

                _parent = parent;
                _parent?._childs.Add(this);
            }

            public TicketStatus Status { get; private set; } = TicketStatus.Queued;

            public IActivity Activity { get; }

            public Ticket Parent => _parent;

            public IReadOnlyCollection<Ticket> Childs => _childs;

            public IReadOnlyCollection<Ticket> After => _after;

            IActivityTicket IActivityTicket.Parent => _parent;

            IReadOnlyCollection<IActivityTicket> IActivityTicket.Childs => _childs;

            IReadOnlyCollection<IActivityTicket> IActivityTicket.After => _after;

            public async Task Run() {
                ChangeStatus(TicketStatus.Running);

                await Activity.Run();

                ChangeStatus(TicketStatus.Ran);

                TryClose();
            }

            private void TryClose() {
                if (_childs.Count == 0) {
                    ChangeStatus(TicketStatus.Closed);
                }
                else {
                    var status = _childs.All(ticket => ticket.Status == TicketStatus.Closed) 
                        ? TicketStatus.Closed 
                        : TicketStatus.WaitingChildren;
                    ChangeStatus(status);
                }

                _parent?.TryClose();
            }

            private void ChangeStatus(TicketStatus status) {
                if (status != Status) {
                    Status = status;
                    _owner.FireActivityChange(this);
                }
            }

            public IActivityTicket Then(IActivity activity) {
                var result = new Ticket(_owner, _parent, activity);
                _after.Add(result);
                return result;
            }
        }
    }
}