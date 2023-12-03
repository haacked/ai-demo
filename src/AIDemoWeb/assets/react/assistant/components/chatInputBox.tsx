import React from "react";
import {ChatInputProps} from "../../chat/components/chatInput";
import {PaperAirplaneIcon} from "@heroicons/react/24/outline";


export default function ChatInputBox({onMessageSubmit}: ChatInputProps) {
  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const form = e.currentTarget as HTMLFormElement;
    const formData = new FormData(form);
    onMessageSubmit(formData.get('message') as string);
    form.reset();
  }

  return (
    <form onSubmit={handleSubmit}>
      <div className="px-6 py-3 bg-white w-100 overflow-hidden rounded-bl-xl rounded-br-xla">
        <div className="flex flex-row items-center space-x-5">
          <div className="relative w-full">
            <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
              <PaperAirplaneIcon className="w-4 h-4" />
            </div>
            <input
                type="text"
                name="message"
                className="w-full block p-1.5 pl-10 text-sm text-gray-900 border border-gray-300 rounded-lg bg-white focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <button
            type="submit"
            className="px-3 py-2 text-xs font-medium text-center text-white bg-blue-500 rounded-lg hover:bg-purple-800 focus:ring-4 focus:outline-none focus:ring-blue-300 disabled:opacity-50"
          >
            Send
          </button>
        </div>
      </div>
    </form>
  );
};
