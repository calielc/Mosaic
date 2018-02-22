using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivityQueue {
    public sealed class GroupActivities : IActivity {
        private readonly Queue _queue;

        public GroupActivities(Queue queue) {
            _queue = queue;
        }

        public GroupActivities(Queue queue, IEnumerable<IActivity> activities) {
            _queue = queue;
            Activities = activities;
        }

        public IEnumerable<IActivity> Activities { get; set; }

        public async Task Run() => await Task.Run(() => {
            _queue.AddSubtask(this, Activities);
        });
    }
}