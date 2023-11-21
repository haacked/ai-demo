import React from "react";
import Avatar from "../../components/avatar";
import {ChatMessage} from "../../models/chatMessage";
import useIdentity from "../../hooks/useIdentity";

interface ChatContentProps {
    messages: ChatMessage[];
}

export default function ChatContent({ messages }: ChatContentProps) {
    const {username} = useIdentity();
    return (
        <div className="max-h-96 h-96 px-6 py-1 overflow-auto">
            {messages.map((message: ChatMessage, index: number) => (
                <div
                    key={index}
                    className={`py-2 flex flex-row w-full ${
                        message.isChatOwner ? "justify-end" : "justify-start"
                    }`}
                >
                    <div className={`${message.isChatOwner ? "order-2" : "order-1"}`}>
                        <Avatar username={message.isChatOwner ? username : null} />
                    </div>
                    <div className={`px-2 w-fit py-3 flex flex-col rounded-lg text-white ${
                            message.isChatOwner ? "order-1 mr-2 bg-blue-500" : "order-2 ml-2 bg-green-500"
                        }`}
                    >
            <span className="text-xs text-gray-200">
              {message.author}&nbsp;-&nbsp;
                {new Date(message.timestamp).toLocaleTimeString("en-US", {
                    hour: "2-digit",
                    minute: "2-digit"
                })}
            </span>
                        <span className="text-md">{message.text}</span>
                    </div>
                </div>
            ))}
        </div>
    );
};