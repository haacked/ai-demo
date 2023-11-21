import * as React from "react";
import {createContext, useContext, useMemo, useState} from "react";
import {ChatMessage} from "../models/chatMessage";

export interface ChatContextState {
    messages: ChatMessage[],
    appendMessage: (message: ChatMessage) => void,
}


export const ChatContext = createContext(null as ChatContextState | null);

export default function useChat() {
    const context = useContext(ChatContext);
    if (!context) throw new Error("ChatContext not found!");
    return context;
}

export function ChatContextProvider(props: {children: React.ReactNode}) {
    const [messages, setMessages] = useState([] as ChatMessage[]);

    function appendNewMessage(message: ChatMessage) {
        messages.push(message);
        setMessages([...messages]);
    }

    const value = useMemo(() => ({
        messages,
        appendMessage: appendNewMessage,
    }), [messages]);

    return (
        <ChatContext.Provider value={value}>
            { props.children }
        </ChatContext.Provider>
    );
}