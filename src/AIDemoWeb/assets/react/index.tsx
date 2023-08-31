import {createRoot} from "react-dom/client";
import ChatApp from "./components/chatApp";
import {ChatContextProvider} from "./hooks/useChat";
import {IdentityContextProvider} from "./hooks/useIdentity";

const appElement = document.getElementById('react-root');
if (!appElement) throw new Error("Could not find #react-root element!");
const username = appElement.dataset.username;
const root = createRoot(appElement);

root.render(
    <IdentityContextProvider username={username}>
        <ChatContextProvider>
            <ChatApp />
        </ChatContextProvider>
    </IdentityContextProvider>

);

