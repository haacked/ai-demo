import * as signalR from "@microsoft/signalr";

/*
 * Sets up a singleton connection to the SignalR hub. In particular, it sets up a listener for the
 * "messageReceived" event and the "aiContextReceived" event. The former is used to update the chat log
 * and the latter is used to add more details to the chat log about what the AI is doing.
 */
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