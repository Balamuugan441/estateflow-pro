(function () {

    "use strict";

    const config = document.getElementById("chatConfig");

    if (!config) {
        return;
    }

    const conversationId =
        Number(config.dataset.conversationId);

    const currentUserId =
        Number(config.dataset.currentUserId);

    /*
     * No active conversation.
     * We still don't need to create a connection here.
     */
    if (!conversationId) {
        return;
    }

    if (!window.signalR) {
        console.error("SignalR library is not loaded.");
        return;
    }

    const messagesContainer =
        document.getElementById("chatMessages");

    const messageInput =
        document.getElementById("chatMessageInput");

    const sendButton =
        document.getElementById("chatSendButton");

    if (!messagesContainer ||
        !messageInput ||
        !sendButton) {

        return;
    }

    const connection =
        new signalR.HubConnectionBuilder()
            .withUrl("/hubs/chat")
            .withAutomaticReconnect()
            .build();

    /*
     * Receive a message from the server.
     */
    connection.on("ReceiveMessage", async function (message) {

    if (Number(message.conversationID) !== conversationId) {
        return;
    }

    addMessage(message);

    /*
     * If the current user is the receiver and this conversation
     * is currently open, immediately mark the message as read.
     */
    if (Number(message.receiverID) === currentUserId) {

        try {

            await connection.invoke(
                "MarkConversationAsRead",
                conversationId
            );

        }
        catch (error) {

            console.error(
                "Failed to mark conversation as read:",
                error
            );

        }
    }

    });
    connection.on(
        "ConversationUpdated",
        function (conversation) {

            const id =
                Number(conversation.conversationID);

            const item =
                document.getElementById(
                    `conversation-${id}`
                );

            if (!item) {
                return;
            }

            /*
             * Update last message.
             */
            const lastMessage =
                item.querySelector(
                    ".conversation-last-message"
                );

            if (lastMessage) {

                lastMessage.textContent =
                    conversation.lastMessage;
            }

            /*
             * Update date.
             */
            const timeElement =
                item.querySelector(
                    ".conversation-time"
                );

            if (timeElement) {

                timeElement.textContent =
                    formatConversationDate(
                        conversation.lastMessageAt
                    );
            }

            /*
             * Move the conversation to the top.
             */
            const list =
                document.querySelector(
                    ".conversation-list"
                );

            if (list) {

                list.prepend(item);
            }

            /*
             * Only increment unread count when
             * the current user is the receiver.
             */
            if (
                Number(conversation.receiverID)
                === currentUserId
                &&
                id !== conversationId
            ) {

                increaseUnreadCount(item);
            }

        }
    );
    connection.on(
        "ConversationRead",
        function (data) {

            const id =
                Number(data.conversationID);

            const item =
                document.getElementById(
                    `conversation-${id}`
                );

            if (!item) {
                return;
            }

            const badge =
                item.querySelector(
                    "[data-unread-badge]"
                );

            if (badge) {
                badge.remove();
            }

        }
    );
    /*
     * Add one message to the chat UI.
     */
    function addMessage(message) {

        const emptyMessage =
            document.getElementById("chatEmptyMessages");

        if (emptyMessage) {
            emptyMessage.remove();
        }

        const messageWrapper =
            document.createElement("div");

        const isMine =
            Number(message.senderID) === currentUserId;

        messageWrapper.className =
            `chat-message ${isMine ? "mine" : "theirs"}`;

        const bubble =
            document.createElement("div");

        bubble.className =
            "chat-bubble";

        const messageText =
            document.createElement("div");

        messageText.textContent =
            message.messageText;

        const messageTime =
            document.createElement("div");

        messageTime.className =
            "chat-message-time";

        messageTime.textContent =
            formatMessageTime(message.sentAt);

        bubble.appendChild(messageText);
        bubble.appendChild(messageTime);

        messageWrapper.appendChild(bubble);

        messagesContainer.appendChild(messageWrapper);

        scrollToBottom();
    }

    /*
     * Format the server timestamp.
     */
    function formatMessageTime(sentAt) {

        const date =
            new Date(sentAt);

        return date.toLocaleString(
            [],
            {
                day: "2-digit",
                month: "short",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit"
            }
        );
    }

    /*
     * Send button.
     */
    sendButton.addEventListener(
        "click",
        sendMessage
    );

    /*
     * Enter sends the message.
     * Shift + Enter creates a new line.
     */
    messageInput.addEventListener(
        "keydown",
        function (event) {

            if (
                event.key === "Enter" &&
                !event.shiftKey
            ) {
                event.preventDefault();

                sendMessage();
            }
        }
    );

    async function sendMessage() {

        const messageText =
            messageInput.value.trim();

        if (!messageText) {
            return;
        }

        if (messageText.length > 2000) {
            alert("Message cannot exceed 2000 characters.");
            return;
        }

        sendButton.disabled = true;
        messageInput.disabled = true;

        try {

            await connection.invoke(
                "SendMessage",
                conversationId,
                messageText
            );



            messageInput.value = "";

        }
        catch (error) {

            console.error(
                "Failed to send message:",
                error
            );

            alert(
                "The message could not be sent."
            );

        }
        finally {

            sendButton.disabled = false;
            messageInput.disabled = false;

            messageInput.focus();
        }
    }

    /*
     * Start SignalR connection.
     */
    async function startConnection() {

        try {

            await connection.start();

            console.log(
                "SignalR chat connection: CONNECTED"
            );

            try {

                await connection.invoke(
                    "MarkConversationAsRead",
                    conversationId
                );

                console.log(
                    "Conversation marked as read:",
                    conversationId
                );

            }
            catch (error) {

                console.error(
                    "Failed to mark conversation as read:",
                    error
                );

            }

            scrollToBottom();

        }
        catch (error) {

            console.error(
                "SignalR chat connection failed:",
                error
            );

            /*
             * Try again after a short delay.
             */
            setTimeout(
                startConnection,
                5000
            );
        }
    }

    /*
     * Scroll the message area to the bottom.
     */
    function scrollToBottom() {

        messagesContainer.scrollTop =
            messagesContainer.scrollHeight;
    }
    function increaseUnreadCount(conversationItem) {

        let badge =
            conversationItem.querySelector(
                "[data-unread-badge]"
            );

        if (!badge) {

            badge =
                document.createElement("span");

            badge.className =
                "unread-badge";

            badge.setAttribute(
                "data-unread-badge",
                ""
            );

            badge.textContent = "1";

            conversationItem.appendChild(
                badge
            );

            return;
        }

        let count =
            Number(badge.textContent) || 0;

        count++;

        badge.textContent =
            count.toString();
    }
    function formatConversationDate(lastMessageAt) {

        if (!lastMessageAt) {
            return "";
        }

        const date =
            new Date(lastMessageAt);

        return date.toLocaleDateString(
            [],
            {
                day: "2-digit",
                month: "short"
            }
        );
    }
    startConnection();

})();