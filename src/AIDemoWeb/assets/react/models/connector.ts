import * as signalR from "@microsoft/signalr";
class Connector {
    private connection: signalR.HubConnection;
    public events: (onMessageReceived: (username: string, message: string) => void) => void;
    static instance: Connector;
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hub')
            .withAutomaticReconnect()
            .build();
        this.connection.start().catch(err => document.write(err));
        this.events = (onMessageReceived) => {
            this.connection.on("messageReceived", (username, message) => {
                onMessageReceived(username, message);
            });
        };
    }
    public newMessage = (username: string, messages: string) => {
        this.connection.send("newMessage", username, messages).then(_ => console.log("sent"));
    }
    public static getInstance(): Connector {
        if (!Connector.instance)
            Connector.instance = new Connector();
        return Connector.instance;
    }
}
export default Connector.getInstance;