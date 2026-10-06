ComponentRegistry.register("TimerText", function (controller, globalService) {
    this.init = (field, element) => {
        field.__ignoreReinit = true;

        if (field.Settings.TimerVariable) {
            const start = controller.get(field.Settings.TimerVariable) ?? false;
            if (start === true) this._raiseTimer(field, element);

            controller.watch(field.Settings.TimerVariable, (start) => {
                if (!field._ignoreWatch) {
                    if (start === true) this._raiseTimer(field, element);
                    else clearInterval(field._timer);
                }

                field._ignoreWatch = true;

                globalService.nextMicroTask(() => {
                    field._ignoreWatch = false;
                });
            });
        }
    }

    this._raiseTimer = (field, element) => {
        const text = field.Settings.TimerText || '[Timer]';
        const count = controller.get(field.Settings.Duration) ?? 0;//parseInt(value, 10);
        if (isNaN(count) || count <= 0) {
            element.style.display = "none";
            return;
        }

        const interval = parseInt(field.Settings.TimerInterval, 10) || 1000;

        let current = count;

        element.style.display = "block";
        element.innerHTML = text.replace('[Timer]', current);

        field._timer = setInterval(() => {
            current--;
            if (current <= 0) {
                controller.set(field.Settings.TimerVariable, false, true);
                this._callAction(field);

                clearInterval(field._timer);
                element.style.display = "none";
            } else {
                element.innerHTML = text.replace('[Timer]', current);
            }
        }, interval);
    };

    this._callAction = async (field) => {
        await controller.callAction(field.Id, null, 'OnTimerComplete');
    }
});