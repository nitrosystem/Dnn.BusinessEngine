export class WebSocketClientService {
    constructor($rootScope, globalService, apiService) {
        this.socket = null;
        this.isConnected = false;
        this.reconnectDelay = 3000;
        this.maxRetryDelay = 15000;
        this.retryCount = 0;

        this.$rootScope = $rootScope;
        this.globalService = globalService;
        this.apiService = apiService;

        this._isConnecting = false;
    }

    init(channel) {
        this.channel = channel;

        this.apiService.post("Studio", "InitWebSocket").then(data => {
            if (data) {
                this.port = data;
                this.connect();
            }
            else
                this.scheduleReconnect();
        });
    }

    connect() {
        if (this._isConnecting || this.isConnected) {
            return;
        }

        this._isConnecting = true;

        const host = window.location.hostname; // دامنه یا آی‌پی فعلی
        const protocol = window.location.protocol === 'https:' ? 'wss' : 'ws'; // انتخاب ws یا wss

        try {
            const url = `${protocol}://${host}:${this.port}/notify?channel=${this.channel}`;
            this.socket = new WebSocket(url);
        } catch (ex) {
            this.scheduleReconnect();
            return;
        }

        this.socket.onopen = () => {
            this.$rootScope.isWebSocketConnected = true;
            this.isConnected = true;
            this._isConnecting = false;
            this.retryCount = 0;
        };

        this.socket.onmessage = (event) =>
            this.onMessage(event.data);

        this.socket.onclose = () => {
            this.$rootScope.isWebSocketConnected = false;
            this.isConnected = false;
            this._isConnecting = false;
            this.scheduleReconnect();
        };

        this.socket.onerror = () => {
            this.$rootScope.isWebSocketConnected = false;
            this.isConnected = false;
            this._isConnecting = false;
            this.scheduleReconnect();
        };
    }

    scheduleReconnect() {
        this.retryCount++;

        const delay = Math.min(
            this.reconnectDelay * this.retryCount,
            this.maxRetryDelay
        );

        setTimeout(() => this.connect(), delay);
    }

    send(msg) {
        if (this.isConnected && this.socket.readyState === WebSocket.OPEN) {
            this.socket.send(msg);
        }
    }

    onMessage(payload) {
        try {
            const listenedData = JSON.parse(payload);
            const data = this.globalService.keysToCamelCase(listenedData);

            if (data.channel !== this.channel) return;

            if (!this.$rootScope.$$phase) {
                this.$rootScope.$apply(() => {
                    this.$rootScope.$emit("onListenToPushingServer", data.message);
                });
            }

        } catch (ex) { }
    }
}
