const psiToKpa = 6.89476;
const kpaToPsi = 1 / psiToKpa;
const unknownRepresentation = "-";

const serverUrlInput = document.getElementById("server-url");
const connectButton = document.getElementById("connect-disconnect");

const currentFuel = document.getElementById("current-fuel")
const currentCompound = document.getElementById("current-compound");
const currentSet = document.getElementById("current-set");
const currentFl = document.getElementById("current-fl");
const currentFr = document.getElementById("current-fr");
const currentRl = document.getElementById("current-rl");
const currentRr = document.getElementById("current-rr");

const currentMenuFuel = document.getElementById("current-menu-fuel")
const currentMenuCompound = document.getElementById("current-menu-compound");
const currentMenuSet = document.getElementById("current-menu-set");
const currentMenuFl = document.getElementById("current-menu-fl");
const currentMenuFr = document.getElementById("current-menu-fr");
const currentMenuRl = document.getElementById("current-menu-rl");
const currentMenuRr = document.getElementById("current-menu-rr");
const copyButton = document.getElementById("copy")

const applyFuel = document.getElementById("apply-fuel")
const applyCompound = document.getElementById("apply-compound");
const applySet = document.getElementById("apply-set");
const applyFl = document.getElementById("apply-fl");
const applyFr = document.getElementById("apply-fr");
const applyRl = document.getElementById("apply-rl");
const applyRr = document.getElementById("apply-rr");
const applyButton = document.getElementById("apply");

const historyTable = document.getElementById("history");

var ws;

(function() {
    var autoconnect;
    const params = new URLSearchParams(window.location.search)
    if (params.has('connect')) {
        autoconnect = params.get('connect');
    } else if (window.location.pathname.startsWith("/ui/")) {
        autoconnect = `ws://${window.location.host}/`
    }
    if (autoconnect) {
      serverUrlInput.value = autoconnect;
      connect();
    }
})();

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
    currentFuel.innerText = telemetry.FuelLeftL.toFixed(2);
    currentCompound.innerText = safeCompoundConversion(telemetry.FrontTires.Compound);
    currentSet.innerText = telemetry.TireSet;
    currentFl.innerText = (telemetry.FrontTires.Left.PressureKpa * kpaToPsi).toFixed(1);
    currentFr.innerText = (telemetry.FrontTires.Right.PressureKpa * kpaToPsi).toFixed(1);
    currentRl.innerText = (telemetry.RearTires.Left.PressureKpa * kpaToPsi).toFixed(1);
    currentRr.innerText = (telemetry.RearTires.Right.PressureKpa * kpaToPsi).toFixed(1);

    currentMenuFuel.innerText = telemetry.PitMenu.FuelToAddL;
    currentMenuCompound.innerText = safeCompoundConversion(telemetry.PitMenu.FrontTires.Compound)
    currentMenuSet.innerText = telemetry.PitMenu.TireSet;
    currentMenuFl.innerText = (telemetry.PitMenu.FrontTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    currentMenuFr.innerText = (telemetry.PitMenu.FrontTires.RightPressureKpa * kpaToPsi).toFixed(1);
    currentMenuRl.innerText = (telemetry.PitMenu.RearTires.LeftPressureKpa * kpaToPsi).toFixed(1);
    currentMenuRr.innerText = (telemetry.PitMenu.RearTires.RightPressureKpa * kpaToPsi).toFixed(1);
    copyButton.disabled = false;
  } catch(e) {
    console.log("Unable to fill telemetry", telemetry, e);
  }
}

function safeCompoundConversion(compound) {
  return (compound && compound != "Unknown") ? compound : unknownRepresentation;
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
  const compound = applyCompound.value;
  const pitStrategyRequest = {
    "FuelToAddL": parseInt(applyFuel.value),
    "TireSet": parseInt(applySet.value),
    "FrontTires": {
      "Compound": compound,
      "LeftPressureKpa": parseFloat(applyFl.value) * psiToKpa,
      "RightPressureKpa": parseFloat(applyFr.value) * psiToKpa
    },
    "RearTires": {
      "Compound": compound,
      "LeftPressureKpa": parseFloat(applyRl.value) * psiToKpa,
      "RightPressureKpa": parseFloat(applyRr.value) * psiToKpa
    }
  };
  ws.send(JSON.stringify({"PitStrategyRequest": pitStrategyRequest}));
  logPitStrategyRequest(pitStrategyRequest);
}

function copyStrategy() {
  applyFuel.value = currentMenuFuel.innerText;
  applySet.value = currentMenuSet.innerText;
  const compound = currentMenuCompound.innerText;
  if (unknownRepresentation != compound)
    applyCompound.value = compound;
  applyFl.value = currentMenuFl.innerText;
  applyFr.value = currentMenuFr.innerText;
  applyRl.value = currentMenuRl.innerText;
  applyRr.value = currentMenuRr.innerText;
}