const psiToKpa = 6.89476;
const kpaToPsi = 1 / psiToKpa;

const connectButton = document.getElementById("connect-button");
const serverUrlInput = document.getElementById("server-url");
const historyTable = document.getElementById("history");

var ws;

const params = new URLSearchParams(window.location.search)
if (params.has('connect')) {
  serverUrlInput.value = params.get('connect');
  connect();
}

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
  var row = historyTable.insertRow();
  try {
    row.insertCell().innerHTML = "-";
    row.insertCell().innerHTML = pitStrategyRequest.FuelToAddL;
    row.insertCell().innerHTML = pitStrategyRequest.FrontTires.Compound;
    const tireSetCell = row.insertCell();
    if (pitStrategyRequest.TireSet)
      tireSetCell.innerHTML = pitStrategyRequest.TireSet;
    row.insertCell().innerHTML = (pitStrategyRequest.FrontTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    row.insertCell().innerHTML = (pitStrategyRequest.FrontTires.RightPressureKpa * kpaToPsi).toFixed(1);
    row.insertCell().innerHTML = (pitStrategyRequest.RearTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    row.insertCell().innerHTML = (pitStrategyRequest.RearTires.RightPressureKpa * kpaToPsi).toFixed(1);
  } catch(e) {
    console.log(e);
  }
}