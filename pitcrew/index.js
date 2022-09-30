var ws;
var connectButton = document.getElementById("connect-button");
var serverUrlInput = document.getElementById("server-url");

function connectOrDisconnect() {
  if (ws != null && ws.readyState == WebSocket.OPEN) {
    disconnect();
  } else {
    connect();
  }
}

function connect() {
  try {
    connectButton.innerHTML = "Connecting";
    connectButton.className = "btn btn-secondary"
    serverUrlInput.disabled = true

    ws = new WebSocket(document.getElementById("server-url").value);

    ws.onopen = function () {
      connectButton.innerHTML = "Disconnect";
      connectButton.className = "btn btn-warning"
      serverUrlInput.disabled = true
    };
    ws.onmessage = function (event) {
      try {
        const payload = JSON.parse(event.data);
        if (payload.Telemetry)
          fillTelemetry(payload.Telemetry);
        if (payload.PitStrategyRequest)
          logPitStrategyRequest(payload.PitStrategyRequest);
      } catch(e) {
        console.log("Failed to parse message", event.data, e);
      }
    };
    ws.onclose = function () {
      connectButton.innerHTML = "Connect";
      connectButton.className = "btn btn-primary"
      serverUrlInput.disabled = false;
    };
  } catch(e) {
    console.log("Failed to connect", e);
    connectButton.innerHTML = "Connect";
    connectButton.className = "btn btn-primary"
    serverUrlInput.disabled = false;
  }
}

function disconnect() {
  ws.close();
}

function fillTelemetry(telemetry) {
  console.log("Telemetry", telemetry);
}

function logPitStrategyRequest(pitStrategyRequest) {
  console.log("PitStrategyRequest", pitStrategyRequest);
}