import {ChatMessage} from "../models/ChatMessage";

export interface ChatLogProps {
    messages: ChatMessage[];
}

export default function ChatLog({messages}: ChatLogProps) {
    const messageElements = messages.map((message) => (
        <div key={message.timestamp.toISOString()}>
            <span className="font-bold">{message.author}</span>: <span>{message.text}</span>
        </div>
    ));

    return (
        <div className="border border-gray-300 m-4 p-2 min-h-[300px] overflow-y-auto">
            {messageElements}
        </div>
    );
}