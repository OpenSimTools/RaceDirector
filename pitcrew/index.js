const psiToKpa = 6.89476;
const kpaToPsi = 1 / psiToKpa;

const serverUrlInput = document.getElementById("server-url");
const connectButton = document.getElementById("connect-disconnect");
const applyButton = document.getElementById("apply");
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
    connectButton.innerHTML = "Connect";
    connectButton.disabled = true;
    serverUrlInput.disabled = true;

    ws = new WebSocket(document.getElementById("server-url").value);

    ws.onopen = function () {
      connectButton.innerHTML = "Disconnect";
      connectButton.className = "btn btn-warning";
      connectButton.disabled = false;
      serverUrlInput.disabled = true;
      applyButton.disabled = false;
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
      connectButton.disabled = false
      serverUrlInput.disabled = false;
      applyButton.disabled = true;
    };
  } catch(e) {
    console.log("Failed to connect", e);
    connectButton.innerHTML = "Connect";
    connectButton.className = "btn btn-primary"
    connectButton.disabled = false
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

function applyPitStrategy() {
  const compound = document.getElementById("apply-compound").value;
  const pitStrategyRequest = {
    "FuelToAddL": parseInt(document.getElementById("apply-fuel").value),
    "TireSet": parseInt(document.getElementById("apply-set").value),
    "FrontTires": {
      "Compound": compound,
      "LeftPressureKpa": parseFloat(document.getElementById("apply-fl").value) * psiToKpa,
      "RightPressureKpa": parseFloat(document.getElementById("apply-fr").value) * psiToKpa
    },
    "RearTires": {
      "Compound": compound,
      "LeftPressureKpa": parseFloat(document.getElementById("apply-rl").value) * psiToKpa,
      "RightPressureKpa": parseFloat(document.getElementById("apply-rr").value) * psiToKpa
    }
  };
  ws.send(JSON.stringify({"PitStrategyRequest": pitStrategyRequest}));
  logPitStrategyRequest(pitStrategyRequest);
}