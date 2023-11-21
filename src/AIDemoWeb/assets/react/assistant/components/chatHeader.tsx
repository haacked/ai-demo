import React from "react";
import Avatar from "./avatar";
import useIdentity from "../../hooks/useIdentity";

interface ChatHeaderProps {
    name: string;
    numberOfMessages: number;
}

export default function ChatHeader({name, numberOfMessages = 0}: ChatHeaderProps) {
    const {username} = useIdentity();

    return (
        <div className="border-b-2 border-b-gray-200 py-3 px-6 flex flex-row justify-between items-center">
            <div className="flex flex-row items-center space-x-1.5">
                <Avatar username={username} />
                <div className="flex flex-col">
                    <p className="text-xs text-gray-600">{name}</p>
                    <p className="text-xs text-gray-400">{numberOfMessages} messages</p>
                </div>
            </div>
            <div className="space-x-1">

            </div>
        </div>
    );
};
