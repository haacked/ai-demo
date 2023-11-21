export interface ChatMessage {
    readonly timestamp: Date,
    readonly text: string,
    readonly author: string,
    readonly isChatOwner?: boolean,
}