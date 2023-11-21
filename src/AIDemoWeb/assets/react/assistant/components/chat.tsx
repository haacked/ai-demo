import React, {useEffect} from "react";
import ChatHeader from "./chatHeader";
import ChatContent from "./chatContent";
import ChatInputBox from "./chatInputBox";
import useIdentity from "../../hooks/useIdentity";
import useChat from "../../hooks/useChat";
import {ChatMessage} from "../../models/chatMessage";

export default function Chat(props:{assistantName: string | null}) {
    const {messages, appendMessage} = useChat();
    const {username} = useIdentity();

    useEffect(() => {
        // Load initial message.
        if (messages.length === 0) {
            const newMessage: ChatMessage = {
                timestamp: new Date(),
                author: 'Assistant Bot',
                isChatOwner: false,
                text: "Hi, I’m the bot. How can I help you?"
            };
            appendMessage(newMessage);
        }
    }, []);

    async function onNewMessage(message: string) {
        const newMessage: ChatMessage = {
            timestamp: new Date(),
            author: username,
            isChatOwner: true,
            text: message
        };
        appendMessage(newMessage);
    }

    return (
        <div className="max-w-sm mx-auto">
            <div className="flex flex-row justify-between items-center py-2">
                <p className="text-md text-white bg-blue-500 px-2 py-1 font-semibold animate-pulse">
                    {props.assistantName}
                </p>
            </div>
            <div className="bg-white border border-gray-200 rounded-lg shadow relative">
                <ChatHeader name={username} numberOfMessages={messages.length} />
                <ChatContent messages={messages} />
                <ChatInputBox onMessageSubmit={onNewMessage} />
            </div>
        </div>
    );
}

