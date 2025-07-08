export class WebSocketService {
    constructor($rootScope) {
        this.$rootScope = $rootScope;

        this.service = {};
        this.socket = null;
        this.listeners = [];
    }

    connect() {
        if (this.socket && this.socket.readyState === WebSocket.OPEN) return;

        this.socket = new WebSocket("ws://dnndev.new/WebSocketHandler.ashx");

        this.socket.onopen = () => {
            console.log("✅ WebSocket connected!");
        };

        this.socket.onmessage = (event) => {  //recive message
            this.reciveMessage(event);
        };

        this.socket.onclose = () => {
            console.log("❌ WebSocket disconnected! Retrying...");
            setTimeout(connect, 5000); // اتصال مجدد در صورت قطع شدن
        };
    }

    register(handler, callback) {
        this.listeners.push({ handler: handler, callback: callback });
        return () => {
            this.listeners = this.listeners.filter(l => l.handler !== handler);
        };
    }

    reciveMessage(event) {
        var data = JSON.parse(event.data);
        this.listeners.forEach((listener) => {
            listener.callback(data);
        });
    }

    sendMessage(message) {
        if (this.socket && this.socket.readyState === WebSocket.OPEN) {
            this.socket.send(JSON.stringify(message));
        }
    }
}
