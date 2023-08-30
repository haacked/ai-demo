import {createRoot} from "react-dom/client";
import ChatApp from "./components/ChatApp";

const appElement = document.getElementById('react-root');
const root = createRoot(appElement);

root.render(
    <ChatApp />
);