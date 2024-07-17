import * as React from "react";
import {createContext, useContext, useMemo, useState} from "react";

export interface IdentityContextState {
    username: string,
}

export const IdentityContext = createContext(null as IdentityContextState | null);

export default function useIdentity() {
    const context = useContext(IdentityContext);
    if (!context) throw new Error("IdentityContext not found!");
    return context;
}

export function IdentityContextProvider(props: {username: string | null, children: React.ReactNode}) {
    const [username, setUsername] = useState(props.username);

    const value = useMemo(() => ({
        username,
        setUsername,
    }), [username]);

    return (
        <IdentityContext.Provider value={value}>
            { props.children }
        </IdentityContext.Provider>
    );
}