import ChatLog from "./chatLog";
import useChat from "../../hooks/useChat";
import {useEffect} from "react";
import useIdentity from "../../hooks/useIdentity";
import Connector from "../models/connector";
import ChatInput from "./chatInput";

export default function ChatApp() {
    const {messages, appendMessage} = useChat()
    const {username} = useIdentity();
    const {newMessage, events} = Connector();

    useEffect(() => {
        // Handles appending new messages received from SignalR to the chat log
        // as well as new thoughts from the AI.
        events(
            // Handles a new incoming message.
            (author, message) => {
                const newMessage = {
                    timestamp: new Date(),
                    text: message,
                    author: author,
                };
                appendMessage(newMessage);
            },

            // Handles a new incoming thought.
            (thought) => {
                console.log(`%c${thought}`, 'color: navy; font-size: 16px; font-family: arial');
            },

            // Handles a function call.
            (name, args) => {
                console.log(`%c${name}%c(${args})`, 'color: maroon; font-size: 16px; background-color: #eee', 'color: navy; font-size: 16px; background-color: #eee');
            },
        );
    }, []);

    async function onNewMessage(message: string) {
        newMessage(username, message);
    }

    return (
        <div className="border border-gray-300">
            <ChatLog messages={messages} />
            <ChatInput onMessageSubmit={onNewMessage} />
        </div>
    );
}