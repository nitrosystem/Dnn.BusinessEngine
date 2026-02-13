export class ActionCenterService {
    constructor($rootScope, $timeout, $interval) {
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.$interval = $interval;

        this.tasks = [];

        $rootScope.$on('onListenToPushingServer', (e, args) => {
            if (args.type == 'ActionCenter') {
                this.updateTaskState(args);
            }
        })
    }

    addTask(task) {
        this.$rootScope.$broadcast('onShowHideActionCenterWidget', { show: true });

        const tsk = this.tasks.find(t => t.taskId === task.taskId);
        if (!tsk) this.tasks.push(task);

        for (const action of task.actions ?? []) {
            if (!isNaN(action.timer)) {
                const actionTimer = this.$interval(() => {
                    if (task.isRemoved)
                        this.$interval.cancel(actionTimer);
                    else if (action.timer <= 0) {
                        this.$interval.cancel(actionTimer);
                        action.callback();
                    } else {
                        action.timer--;
                    }
                }, 1000);
            }
        }
    }

    removeTask(taskId) {
        const task = this.tasks.find(t => t.taskId === taskId);
        task.isRemoved = true;
        this.tasks.splice(this.tasks.indexOf(task, 1));

        if (!this.tasks.length)
            this.$rootScope.$broadcast('onShowHideActionCenterWidget', { show: false });
    }

    updateTaskState(entry) {
        const task = this.tasks.find(t => t.taskId == entry.taskId);
        if (!task) {
            this.addTask(entry);
            return;
        }

        task.message = entry.message;
        task.link = entry.link;
        task.percent = entry.percent;
        task.end = entry.end;
        task.close = entry.close;

        if (task.end && task.close !== false)
            this.$timeout(() => this.removeTask(task.taskId), 2000);
    }
}