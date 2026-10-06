export class ActionCenterService {
    constructor($rootScope, $timeout, $interval) {
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$interval = $interval;
        this.tasks = [];
        this.timers = new Map(); // Timer map for cleanup

        $rootScope.$on('onListenToPushingServer', (e, args) => {
            if (args.type === 'ActionCenter') {
                this.updateTaskState(args);
            }
        });
    }

    addTask(task) {
        const existing = this.tasks.find(t => t.taskId === task.taskId);
        if (existing) return existing;

        this.tasks.push(task);
        this.$rootScope.$broadcast('onShowHideActionCenterWidget', { show: true });

        const taskTimers = [];
        for (const action of task.actions ?? []) {
            if (typeof action.timer === 'number' && !isNaN(action.timer)) {
                const timer = this.$interval(() => {
                    if (task.isRemoved) {
                        this.$interval.cancel(timer);
                        return;
                    }

                    if (action.timer <= 0) {
                        this.$interval.cancel(timer);
                        if (typeof action.callback === 'function') {
                            action.callback();
                        }
                    } else {
                        action.timer--;
                    }
                }, 1000);

                taskTimers.push(timer);
            }
        }

        this.timers.set(task.taskId, taskTimers);
        return task;
    }

    updateTaskState(entry) {
        const task = this.tasks.find(t => t.taskId === entry?.taskId);
        if (!task) {
            if (entry?.end !== true && entry?.close !== true)
                this.addTask(entry);

            return;
        }

        // update only the existing props
        Object.assign(task, {
            message: entry.message,
            link: entry.link,
            isError: entry.isError,
            percent: !entry.isError ? entry.percent : this._lastPercent,
            end: entry.end,
            close: entry.close
        });

        if (!entry.isError) this._lastPercent = entry.percent;

        if (task.end || task.close === true) {
            this.$timeout(() => this.removeTask(task.taskId), 2000);
        }
    }

    removeTask(taskId) {
        const index = this.tasks.findIndex(t => t.taskId === taskId);
        if (index === -1) return;

        const task = this.tasks[index];
        task.isRemoved = true;

        // cancel timers
        const taskTimers = this.timers.get(taskId);
        if (taskTimers) {
            taskTimers.forEach(t => this.$interval.cancel(t));
            this.timers.delete(taskId);
        }

        this.tasks.splice(index, 1);
        if (!this.tasks.length) {
            this.$rootScope.$broadcast('onShowHideActionCenterWidget', { show: false });
        }
    }
}
