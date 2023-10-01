import ChatLog from "./ChatLog";
import useChat from "../hooks/useChat";
import {useEffect} from "react";
import useIdentity from "../hooks/useIdentity";
import Connector from "../models/Connector";
import ChatInput from "./ChatInput";

export default function ChatApp() {
    const {messages, appendMessage} = useChat()
    const {username} = useIdentity();
    const {newMessage, events} = Connector();

    useEffect(() => {
        // Handles appending new messages received from SignalR to the chat log.
        events((author, message) => {
            const newMessage = {
                timestamp: new Date(),
                text: message,
                author: author,
            };
            appendMessage(newMessage);
        });
    }, []);

    async function onNewMessage(message: string) {
        newMessage(username, message);
    }

    return (
        <div className="border border-gray-300">
            <ChatLog messages={messages} />
            <ChatInput onMessagesSubmit={onNewMessage} />
        </div>
    );
}