import {ChatMessage} from "../../models/chatMessage";
import Avatar from "../../components/avatar";

export interface ChatLogProps {
    messages: ChatMessage[];
}

export default function ChatLog({messages}: ChatLogProps) {
    const messageElements = messages.map((message) => (
        <div key={message.timestamp.toISOString()}>
            <div className="flex">
                <div className="flex flex-col p-1">
                    <Avatar username={message.author} />
                </div>
                <div className="flex flex-col ml-2">
                    <div className="flex-row">
                        <span className="font-semibold ">{message.author}</span>
                        <span className="font-normal text-xs ml-2 text-gray-500">{message.timestamp.toLocaleTimeString()}</span>
                    </div>
                    <div className="flex-row">{message.text}</div>
                </div>
            </div>
        </div>
    ));

    return (
        <div className="border border-gray-300 m-4 p-2 min-h-[300px] overflow-y-auto">
            {messageElements}
        </div>
    );
}