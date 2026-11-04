const ws = new WebSocket("ws://192.168.211.27:8090");
const log = document.getElementById("log");  

ws.onopen = () => onPlayerConnect();

ws.onmessage = (event) => processData(event.data);

ws.onclose = () => log.innerHTML += "<div>Conexión cerrada</div>";

function onPlayerConnect() {
    const PlayerString = (dataJSON.Player.substring(7) + ":8090/").trim();

    playerSprites.set(PlayerString, playerSprites.size + 1);
    
}

function enviarMsg(inputMessage) {
	const input = inputMessage;
	ws.send(input);
	input.value = "";
}

function processData(inputData) {
	const dataJSON = JSON.parse(inputData);

    const WSString = ws.url.substring(5).trim();
    const PlayerString = (dataJSON.Player.substring(7) + ":8090/").trim();

    if(WSString == PlayerString) {
        console.log("Yes");
    } else {
        console.log("No");
    }
}