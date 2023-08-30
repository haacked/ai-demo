import ChatLog from "./ChatLog";
import ChatInput from "./ChatInput";
import useChat, {ChatContext, ChatContextProvider, ChatMessage} from "../hooks/useChat";
import {useEffect, useState} from "react";
import * as signalR from "@microsoft/signalr";
import Connector from "../models/Connector";

export default function ChatApp() {
    const {messages, setMessages} = useChat()
    const {newMessage, events} = Connector();

    const username = 'unknown';

    useEffect(() => {
        events((userName, message) => {
            const newMessage = {
                text: message,
                author: userName,
            };
            setMessages([...messages, newMessage]);
        });
    }, []);

    async function onNewMessage(message: string) {
        newMessage(message);
    }

    return (
        <div className="border border-gray-300">
            <ChatLog messages={messages} />
            <ChatInput onMessagesSubmit={onNewMessage} />
        </div>
    );
}