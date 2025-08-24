document.addEventListener("DOMContentLoaded", () => {
    const chatBadge = document.getElementById("chat-badge");
    const chatWindow = document.getElementById("chat-window");
    const chatClose = document.getElementById("chat-close");
    const chatSend = document.getElementById("chat-send");
    const chatInput = document.getElementById("chat-text");
    const chatMessages = document.getElementById("chat-messages");
    let user;
    // SignalR Hub 連線
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7104/chathub")
        .build();

    connection.start().then(function () {
        console.log("Hub 連線完成");
        user = "";
        userName = "";
        connection.invoke("CustomerServiceLogin", user, "123")
            .then(() => console.log("客服登入成功"))
            .catch(err => console.error("客服登入失敗", err));
    }).catch(function (err) {
        console.log("Hub 連線失敗");
    });
    // 接收訊息
    connection.on("UpdContent", function (message) {
        appendMessage("使用者-" + userName+":",message);
    });
    connection.on("system", function (message) {
        appendMessage(message,"");
    });
    connection.on("user", function (newuser) {
        user = newuser;
    });
    connection.on("clearUser", function (newuser) {
        user = "";
        userName = "";
    });
    connection.on("userName", function (newuser) {
        userName = newuser;

    });

    // 點擊懸浮球開關聊天窗
    chatBadge.addEventListener("click", () => {
        chatWindow.style.display = chatWindow.style.display === "flex" ? "none" : "flex";
    });

    // 關閉按鈕
    chatClose.addEventListener("click", () => chatWindow.style.display = "none");

    // 發送訊息
    chatSend.addEventListener("click", sendMessage);
    chatInput.addEventListener("keydown", (e) => { if (e.key === "Enter") sendMessage(); });

    function sendMessage() {
        const msg = chatInput.value.trim();
        if (!user) {
            appendMessage("", "尚未連線使用者");
            return;
        }
        if (!msg) return;
        appendMessage("我:", msg);
        chatInput.value = "";

        // 發送到 Hub
        connection.invoke("SendMessageToUser", user, msg)
            .catch(err => console.error(err.toString()));
    }

    function appendMessage(sender, msg) {
        const div = document.createElement("div");
        div.innerHTML = `<b>${sender}</b> ${msg}`;
        chatMessages.appendChild(div);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
});
