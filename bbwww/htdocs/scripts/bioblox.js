
// this object contains information to communticate to the server
to_server = {}

// this object contains information from the server
from_server = {}

function do_get(request) {
  var str = JSON.stringify(to_server);
  //request.setRequestHeader("Content-length", str.length);
  request.open("PUT", "/data", true);
  request.send(str);
}

function keydown(event) {
  var key = event.keyCode || event.which;
  to_server["key" + key] = true;
  console.log(to_server);
}

function keyup(event) {
  var key = event.keyCode || event.which;
  delete to_server["key" + key];
  console.log(to_server);
}

function run(e) {
  canvas = document.getElementById("canvas");
  console.log("canvas=" + canvas);

  var ctx = canvas.getContext("2d");

  var request = new XMLHttpRequest();
  request.responseType = "json";
  request.onreadystatechange = function () {
    if (request.readyState == 4 && request.status == 200) {
      //if (request.response) display(ctx, request.response);
      console.log(request.response);
      do_get(request);
    }
  }
  do_get(request);
}

