import { GlobalSettings } from "../angular-configs/global.settings";

export class HubService {
    constructor(globalService) {
        "ngInject";

        this.globalService = globalService;

        this.hubs = [];
        this.options = {
            pingTimer: 500
        }
    }

    subscribe(parent, name, type, handler, callback, endCallback, options, steps) {
        const hubId = this.globalService.generateGuid();

        this.hubs.push({
            id: hubId,
            parent: parent,
            name: name,
            type: type,
            handler: handler,
            callback: callback,
            endCallback: endCallback,
            options: { ...this.options, ...options },
            steps: steps ?? 1
        });

        return hubId;
    }

    getHub(id) {
        let hub = _.find(this.hubs, (h) => { return h.id == id });
        return hub;
    }

    init(hub) {
        hub.logFile = `/Portals/${GlobalSettings.portalId}-System/business-engine/monitoring-progress/${GlobalSettings.scenarioName}/${hub.parent}/${hub.type}-${hub.name}-log.txt`;
        hub.progressFile = `/Portals/${GlobalSettings.portalId}-System/business-engine/monitoring-progress/${GlobalSettings.scenarioName}/${hub.parent}/${hub.type}-${hub.name}-progress.txt`;

        var result = {
            MonitoringFile: hub.logFile,
            ProgressFile: hub.progressFile
        }

        this.start(hub, 1);

        return result;
    }

    start(hub, index) {
        let status = 0;
        let started = false;
        let pinged = 0;
        let lastContent = '';
        let lastContentRepeatCount = 0;
        let error404Count = 0;
        var ping = setInterval(() => {
            const ver = '?ver=' + new Date().getTime().toString();
            fetch(hub.logFile + ver).then((stream) => {
                if (stream.status == 404) error404Count++;
                if (error404Count >= 50) clearInterval(ping);

                status = stream.status;
                return stream.text();
            }).then((content) => {
                if (lastContent == content) lastContentRepeatCount++;
                else lastContentRepeatCount = 0;

                lastContent = content;
                if (lastContentRepeatCount > 50) clearInterval(ping);

                if (status == 200) hub.callback.apply(hub.handler, [hub, 'log', content]);
            });

            fetch(hub.progressFile + ver).then((stream) => {
                status = stream.status;
                return stream.text();
            }).then((content) => {
                if (content == 'b---end' && started) {
                    hub.endCallback.apply(hub.handler, [hub, 'log', content]);
                    clearInterval(ping);

                    if (hub.steps > index) this.start(hub, index + 1);
                }
                else if (content != 'b---end' && status == 200) {
                    started = true;

                    hub.callback.apply(hub.handler, [hub, 'progress', content]);

                    if (parseInt(content) >= 100 || pinged > 1000) {
                        if (hub.steps > index) {
                            clearInterval(ping);
                            this.start(hub, index + 1);
                        }
                    }
                }
            });

            pinged++;
        }, 100);
    }
}