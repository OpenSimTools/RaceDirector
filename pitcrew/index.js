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
    connectButton.innerText = "Connect";
    connectButton.disabled = true;
    serverUrlInput.disabled = true;

    ws = new WebSocket(serverUrlInput.value);

    ws.onopen = function () {
      connectButton.innerText = "Disconnect";
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
      connectButton.innerText = "Connect";
      connectButton.className = "btn btn-primary"
      connectButton.disabled = false
      serverUrlInput.disabled = false;
      applyButton.disabled = true;
    };
  } catch(e) {
    console.log("Failed to connect", e);
    connectButton.innerText = "Connect";
    connectButton.className = "btn btn-primary"
    connectButton.disabled = false
    serverUrlInput.disabled = false;
  }
}

function disconnect() {
  ws.close();
}

function fillTelemetry(telemetry) {
  try {
    document.getElementById("current-fuel").innerText = telemetry.FuelLeftL.toFixed(2);
    document.getElementById("current-compound").innerText = telemetry.FrontTires.Compound;
    document.getElementById("current-set").innerText = telemetry.TireSet;
    document.getElementById("current-fl").innerText = (telemetry.FrontTires.Left.PressureKpa * kpaToPsi).toFixed(1);
    document.getElementById("current-fr").innerText = (telemetry.FrontTires.Right.PressureKpa * kpaToPsi).toFixed(1);
    document.getElementById("current-rl").innerText = (telemetry.RearTires.Left.PressureKpa * kpaToPsi).toFixed(1);
    document.getElementById("current-rr").innerText = (telemetry.RearTires.Right.PressureKpa * kpaToPsi).toFixed(1);
    
    document.getElementById("current-menu-fuel").innerText = telemetry.PitMenu.FuelToAddL;
    document.getElementById("current-menu-compound").innerText = telemetry.PitMenu.FrontTires.Compound;
    document.getElementById("current-menu-set").innerText = telemetry.PitMenu.TireSet;
    document.getElementById("current-menu-fl").innerText = (telemetry.PitMenu.FrontTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    document.getElementById("current-menu-fr").innerText = (telemetry.PitMenu.FrontTires.RightPressureKpa * kpaToPsi).toFixed(1);
    document.getElementById("current-menu-rl").innerText = (telemetry.PitMenu.RearTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    document.getElementById("current-menu-rr").innerText = (telemetry.PitMenu.RearTires.RightPressureKpa * kpaToPsi).toFixed(1);
  } catch(e) {
    console.log("Unable to fill telemetry", telemetry, e);
  }
}

function logPitStrategyRequest(pitStrategyRequest) {
  var row = historyTable.insertRow();
  try {
    row.insertCell().innerText = "-";
    row.insertCell().innerText = pitStrategyRequest.FuelToAddL;
    row.insertCell().innerText = pitStrategyRequest.FrontTires.Compound;
    const tireSetCell = row.insertCell();
    if (pitStrategyRequest.TireSet)
      tireSetCell.innerText = pitStrategyRequest.TireSet;
    row.insertCell().innerText = (pitStrategyRequest.FrontTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    row.insertCell().innerText = (pitStrategyRequest.FrontTires.RightPressureKpa * kpaToPsi).toFixed(1);
    row.insertCell().innerText = (pitStrategyRequest.RearTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    row.insertCell().innerText = (pitStrategyRequest.RearTires.RightPressureKpa * kpaToPsi).toFixed(1);
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