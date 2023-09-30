export interface ChatInputProps {
    onMessagesSubmit: (value: string) => void;
}

export default function ChatInput({onMessagesSubmit}: ChatInputProps) {
    function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
        e.preventDefault();
        const form = e.currentTarget as HTMLFormElement;
        const formData = new FormData(form);
        onMessagesSubmit(formData.get('message') as string);
        form.reset();
    }

    return (
        <form onSubmit={handleSubmit}>
            <div className="align-middle flex flex-row m-2">
                <label>Message:
                    <input name="message" className="form-input ml-1 mr-2 flex-1" type="text" />
                </label>
                <button type="submit" className="btn btn-primary">Send</button>
            </div>
        </form>
    );
}