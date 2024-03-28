import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import Chat from "./components/chat";
import {IdentityContextProvider} from "../hooks/useIdentity";
import {ChatContextProvider} from "../hooks/useChat";

const appElement = document.getElementById("assistant-root");
if (!appElement) throw new Error("Could not find #react-root element!");
const assistantName = appElement.dataset.assistantName;
const username = appElement.dataset.username;
const root = createRoot(appElement!);

root.render(
    <StrictMode>
        <IdentityContextProvider username={username}>
            <ChatContextProvider>
                <Chat assistantName={assistantName} />
            </ChatContextProvider>
        </IdentityContextProvider>
    </StrictMode>
);