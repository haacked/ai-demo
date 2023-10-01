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
                console.log(`%c ${thought}`, 'color: navy; font-size: 24px;');
            }
        );
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