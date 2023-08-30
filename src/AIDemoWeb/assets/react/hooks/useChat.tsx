import * as React from "react";
import * as signalR from "@microsoft/signalr";
import {createContext, useContext, useMemo, useState} from "react";

export interface ChatContextState {
    messages: ChatMessage[],
    setMessages: (messages: ChatMessage[]) => void,
}

export interface ChatMessage {
    readonly text: string,
    readonly author: string,
}

export const ChatContext = createContext(null as ChatContextState | null);

export default function useChat() {
    const context = useContext(ChatContext);
    if (!context) throw new Error("ChatContext not found!");
    return context;
}

export function ChatContextProvider(props: {children: React.ReactNode}) {
    const [messages, setMessages] = useState([] as ChatMessage[]);
    const [connection, setConnection] = useState(null as signalR.HubConnection | null);

    const value = useMemo(() => ({
        messages,
        setMessages,
    }), [messages]);

    return (
        <ChatContext.Provider value={value}>
            { props.children }
        </ChatContext.Provider>
    );
}