export class SseClientService {
    constructor($rootScope, $timeout, globalService) {
        this.$rootScope = $rootScope;
        this.$timeout = $timeout;
        this.globalService = globalService;
    }

    init(channel) {
        this._channel = channel;
        this.connect();
    }

    connect() {
        if (this.source) return;

        this.source = new EventSource(`/API/BusinessEngineStudio/Sse/Notify?channel=${this._channel}`);

        this.source.onopen = () => {
            console.log("SSE connected");
            this.isConnected = true;
            this.retryCount = 0;
        };

        this.source.onmessage = (e) => {
            const payload = JSON.parse(e.data);
            if (payload.channel !== this._channel) return;

            this.$timeout(() => {
                this.$rootScope.$broadcast("onListenToPushingServer", payload);
            });
        };

        this.source.onerror = () => {
            console.warn("SSE lost connection");

            this.isConnected = false;
            this.source.close();
            this.source = null;

            const delay = Math.min(1000 * Math.pow(2, this.retryCount++), 15000);
            console.log(`Reconnect in ${delay}ms`);

            this.$timeout(() => this.connect(), delay);
        };


    }
}
