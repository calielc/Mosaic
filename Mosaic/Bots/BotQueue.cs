using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mosaic.Creators;

namespace Mosaic.Bots {
    internal class BotQueue {
        private readonly ICreator _creator;
        private readonly BlockingCollection<IBot> _queue;
        private int _runningAgents;

        public BotQueue(ICreator creator) {
            _creator = creator;
            _queue = new BlockingCollection<IBot>();
        }

        public void Enqueue(IBot bot) {
            _queue.Add(bot);
        }

        public async Task WaitAll(int consumers) {
            var tasks = Enumerable.Range(0, consumers).Select(id => new Agent(id, this).Run());

            await Task.WhenAll(tasks);
        }

        private sealed class Agent {
            private static readonly TimeSpan TimeToWait = TimeSpan.FromSeconds(2);

            private readonly int _id;
            private readonly BotQueue _owner;

            public Agent(int id, BotQueue owner) {
                _id = id;
                _owner = owner;
            }

            public Task Run() => Task.Run(async () => {
                do {
                    Broadcast.Start(this, $"Agent {_id:00}");
                    _owner._runningAgents += 1;
                    try {
                        while (_owner._queue.TryTake(out var bot, TimeToWait)) {
                            await bot.Process(_owner._creator);
                            Broadcast.Step(this);
                        }
                    }
                    finally {
                        _owner._runningAgents -= 1;
                        Broadcast.End(this);
                    }

                    while (_owner._queue.Count == 0 && _owner._runningAgents > 0) {
                        Thread.Sleep(TimeToWait);
                    }
                }
                while (_owner._queue.Count > 0);
            });
        }
    }
}