export class TaskQueueService {
    constructor($rootScope, $timeout, persist = false) {
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.concurrency = 20;
        this.persist = persist;

        this.taskQueue = {};      // { moduleId: { category: TaskEntry[] } }
        this.runningTasks = {};   // { moduleId: { category: Promise[] } }
        this.context = {};        // { moduleId: { category: Map<key, value> } }
        this.pausedCategories = {}; // { moduleId: Set<string> }
        this.autoRunDebouncers = {}; // { moduleId: Function }
        this.autoRunDelay = 15;
        this.tokens = {};
        this.total = {};
        this.allTasks = {};
        this.action = {};

        if (this.persist) this.loadFromStorage();
    }
    
    addTask( category, id, icon, label, desc) {
        // Init structures
        if (!this.taskQueue[moduleId]) this.taskQueue[moduleId] = {};
        if (!this.taskQueue[moduleId][category]) this.taskQueue[moduleId][category] = [];

        if (!this.context[moduleId]) this.context[moduleId] = {};
        if (!this.context[moduleId]) this.context[moduleId] = new Map();

        if (!this.pausedCategories[moduleId]) this.pausedCategories[moduleId] = new Set();
        if (!this.runningTasks[moduleId]) this.runningTasks[moduleId] = {};

        const taskEntry = { moduleId, id, icon, label, priority, task, cancelFn, status: 'pending' };

        var index = this.findIndex(moduleId, category, id)
        if (index !== -1) this.taskQueue[moduleId][category][index] = taskEntry;
        else this.taskQueue[moduleId][category].push(taskEntry);

        this.setAllTasks(moduleId);

        this.$rootScope.$broadcast('onCurrentTabChange');

        if (!this.autoRunDebouncers[moduleId])
            this.setDebounce(moduleId, moduleController);

        if (this.persist) this.saveToStorage();

        if (!moduleController.showTaskWidget) {
            this.$timeout(() => {
                moduleController.showTaskWidget = true;
                this.$timeout(() => {
                    moduleController.showTaskWidget = false;
                }, 4000);
            });
        }
    }

    setDebounce(moduleId, moduleController) {
        if (!this.autoRunDebouncers[moduleId]) {
            this.autoRunDebouncers[moduleId] = _.debounce(() => {
                moduleController.getLastActivities();
                if (!this.allTasks[moduleId] || !this.allTasks[moduleId].length) return;

                if (this.action && this.action.timerHandler) {
                    clearInterval(this.action.timerHandler)
                    delete this.action;
                }

                this.action = {
                    timer: this.autoRunDelay,
                    doCallback: {
                        text: 'Exec Tasks',
                        css: 'btn-green',
                        invoke: () => {
                            clearInterval(this.action.timerHandler);
                            this.executeTasks(moduleId, moduleController);
                            delete this.action;
                        }
                    },
                    cancelCallback: {
                        text: `Try ${this.autoRunDelay * 2} Seconds Late...`,
                        css: 'btn-cancel',
                        invoke: () => {
                            clearInterval(this.action.timerHandler);
                            delete this.action;
                            this.autoRunDebouncers[moduleId].cancel();
                            this.$timeout(() => {
                                this.autoRunDebouncers[moduleId]();
                            }, this.autoRunDelay * 1000);
                        }
                    }
                }

                this.setActionTimer(moduleController);
            }, this.autoRunDelay * 1000);
        }
        else {
            this.autoRunDebouncers[moduleId].cancel();
        }

        this.autoRunDebouncers[moduleId]();
    }

    reinitDebounce(moduleController) {
        const moduleId = moduleController.moduleId;

        if (this.action && this.action.timerHandler) {
            clearInterval(this.action.timerHandler)
            delete this.action;
        }

        if(moduleController.showTaskWidget) moduleController.showTaskWidget = false;

        this.autoRunDebouncers[moduleId]();
    }

    async executeTasks(moduleId, moduleController) {
        moduleController.showTaskWidget = true;

        this.autoRunDebouncers[moduleId].cancel();

        if (this.action) {
            clearInterval(this.action.timerHandler)
            delete this.action;
        }

        const executeCategoryTasks = async (currentCategory) => {
            if (!this.taskQueue[moduleId] || !this.taskQueue[moduleId][currentCategory]) return;
            if (this.pausedCategories[moduleId]?.has(currentCategory)) return;

            const queue = this.taskQueue[moduleId][currentCategory];
            if (queue.length === 0) return;

            if (!this.runningTasks[moduleId][currentCategory]) this.runningTasks[moduleId][currentCategory] = [];

            const currentQueue = this.taskQueue[moduleId][currentCategory];
            const sortedQueue = currentQueue.sort((a, b) => a.priority - b.priority);

            while (
                sortedQueue.some(t => t.status === 'pending') ||
                this.runningTasks[moduleId][currentCategory].length > 0
            ) {
                if (this.pausedCategories[moduleId]?.has(currentCategory)) break;

                while (this.runningTasks[moduleId][currentCategory].length < this.concurrency) {
                    const nextTask = sortedQueue.find(t => t.status === 'pending');
                    if (!nextTask) break;

                    nextTask.status = 'running';
                    const promise = nextTask.task()
                        .then(() => nextTask.status = 'completed')
                        .catch((e) => {
                            console.error(e);
                            nextTask.status = 'failed'
                        })
                        .finally(() => {
                            this.runningTasks[moduleId][currentCategory] =
                                this.runningTasks[moduleId][currentCategory].filter(p => p !== promise);

                            this.onRemoveTask(moduleController, moduleId, nextTask.category, nextTask.id);

                            if (this.persist) this.saveToStorage();
                        });

                    this.runningTasks[moduleId][currentCategory].push(promise);
                }

                await Promise.all(this.runningTasks[moduleId][currentCategory]);
            }
        };

        const categories = Object.keys(this.taskQueue[moduleId]);
        for (let i = 0; i < categories.length; i++) {
            const currentCategory = categories[i];
            await executeCategoryTasks(currentCategory);
        }
    }

    setActionTimer(moduleController) {
        if (this.action.timer) {
            this.action.timerHandler = setInterval(() => {
                if (this.action.timer <= 1) {
                    moduleController.$scope.$apply(() => {
                        this.action.doCallback.invoke();
                    });
                }
                else {
                    this.action.timer--;
                    moduleController.$scope.$apply();
                }
            }, 1000)
        }
    }

    setAllTasks(moduleId) {
        this.allTasks[moduleId] = [];
        const categories = this.taskQueue[moduleId];
        for (const category in categories) {
            if (categories.hasOwnProperty(category)) {
                const tasks = categories[category];
                tasks.forEach(task => {
                    task.category = category;
                    this.allTasks[moduleId].push(task);
                });
            }
        }

        const count = this.allTasks[moduleId].length;
        this.total[moduleId].count = count;
        this.total[moduleId].title = count ? `${count} Tasks For Execution` : '';

    }

    removeTask(moduleController, item) {
        this.onRemoveTask(moduleController, item.moduleId, item.category, item.id);
        if (item.cancelFn()) item.cancelFn();
    }

    onRemoveTask(moduleController, moduleId, category, id) {
        this.$timeout(() => {
            var index = this.findIndex(moduleId, category, id)
            if (index !== -1) {
                const queue = this.taskQueue[moduleId][category];
                queue.splice(index, 1);
            }

            this.setAllTasks(moduleId);

            index = _.findIndex(this.allTasks, (t) => { return t.id == id });
            if (index == -1) moduleController.removeFieldTaskFlag(id);

            if (!this.allTasks.length)
                this.autoRunDebouncers[moduleId].cancel();
            else
                this.autoRunDebouncers[moduleId]();
        }, 1000);
    }

    findIndex(moduleId, category, id) {
        let index = -1;
        if (this.taskQueue[moduleId]) {
            const queue = this.taskQueue[moduleId][category];
            index = _.findIndex(queue, item => item.id == id);
        }

        return index;
    }

    pauseCategory(moduleId, category) {
        if (!this.pausedCategories[moduleId]) this.pausedCategories[moduleId] = new Set();
        this.pausedCategories[moduleId].add(category);
    }

    resumeCategory(moduleId, category) {
        this.pausedCategories[moduleId]?.delete(category);
        this.executeTasks(moduleId, category);
    }

    saveToStorage() {
        const data = JSON.stringify(this.taskQueue, (key, value) =>
            typeof value === 'function' ? undefined : value
        );
        localStorage.setItem('taskQueue', data);
    }

    loadFromStorage() {
        const raw = localStorage.getItem('taskQueue');
        if (!raw) return;
        try {
            const parsed = JSON.parse(raw);
            for (const moduleId in parsed) {
                this.taskQueue[moduleId] = this.taskQueue[moduleId] ?? {};
                for (const category in parsed[moduleId]) {
                    this.taskQueue[moduleId][category] = parsed[moduleId][category];
                    this.taskQueue[moduleId][category].forEach(t => t.status = 'pending');
                }
            }
        } catch {
            console.warn('Failed to parse persisted task queue');
        }
    }
}


