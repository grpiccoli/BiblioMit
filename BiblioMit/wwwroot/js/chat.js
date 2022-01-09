const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();
connection.on("ReceiveMessage", (user, message) => {
    const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    const encodedMsg = user + ": " + msg;
    const li = document.createElement("li");
    $(li).addClass("self");
    li.textContent = encodedMsg;
    $("#messagesList").append(li);
});
connection.start().catch((err) => console.error(err.toString()));
if ($("#sendButton").length !== 0) {
    $("#sendButton").click((event) => {
        const user = $("#userInput").val();
        const message = $("#messageInput").val();
        connection.invoke("SendMessage", user, message).catch((err) => console.error(err.toString()));
        event.preventDefault();
    });
}
var element = $('.floating-chat');
var myStorage = localStorage;
if (!myStorage.getItem('chatID')) {
    myStorage.setItem('chatID', createUUID());
}
setTimeout((_) => {
    element.addClass('enter');
}, 1000);
element.click(openElement);
function openElement() {
    var messages = element.find('.messages');
    var textInput = element.find('.text-box');
    element.find('>i').hide();
    element.addClass('expand');
    element.find('.chat').addClass('enter');
    textInput.keydown(onMetaAndEnter).prop("disabled", false).focus();
    element.off('click', openElement);
    element.find('.header button').click(closeElement);
    element.find('#sendMessage').click(sendNewMessage);
    messages.scrollTop(messages.prop("scrollHeight"));
}
function closeElement() {
    element.find('.chat').removeClass('enter').hide();
    element.find('>i').show();
    element.removeClass('expand');
    element.find('.header button').off('click', closeElement);
    element.find('#sendMessage').off('click', sendNewMessage);
    element.find('.text-box').off('keydown', onMetaAndEnter).prop("disabled", true).blur();
    setTimeout((_) => {
        element.find('.chat').removeClass('enter').show();
        element.click(openElement);
    }, 500);
}
function createUUID() {
    var s = [];
    var hexDigits = "0123456789abcdef";
    for (var i = 0; i < 36; i++) {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1);
    }
    s[14] = "4";
    s[19] = hexDigits.substr(s[19] & 0x3 | 0x8, 1);
    s[8] = s[13] = s[18] = s[23] = "-";
    var uuid = s.join("");
    return uuid;
}
function sendNewMessage() {
    var userInput = $('.text-box');
    var newMessage = userInput.html().replace(/\<div\>|\<br.*?\>/ig, '\n').replace(/\<\/div\>/g, '').trim().replace(/\n/g, '<br>');
    if (!newMessage)
        return;
    var messagesContainer = $('.messages');
    messagesContainer.append([
        '<li class="self">',
        newMessage,
        '</li>'
    ].join(''));
    userInput.html('');
    userInput.focus();
    messagesContainer.finish().animate({
        scrollTop: messagesContainer.prop("scrollHeight")
    }, 250);
}
function onMetaAndEnter(event) {
    if ((event.metaKey || event.ctrlKey) && event.keyCode === 13) {
        sendNewMessage();
    }
}
//# sourceMappingURL=chat.js.map