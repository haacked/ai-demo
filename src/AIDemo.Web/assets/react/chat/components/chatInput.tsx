export interface ChatInputProps {
    onMessageSubmit: (value: string) => void;
}

export default function ChatInput({onMessageSubmit}: ChatInputProps) {
    function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
        e.preventDefault();
        const form = e.currentTarget as HTMLFormElement;
        const formData = new FormData(form);
        onMessageSubmit(formData.get('message') as string);
        form.reset();
    }

    return (
        <form onSubmit={handleSubmit}>
            <div className="align-middle flex flex-row m-2">
                <input type="text" name="message" className="form-input ml-1 mr-2 flex-1" />
                <button type="submit" className="btn btn-primary">Send</button>
            </div>
        </form>
    );
}