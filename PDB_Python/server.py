
# code reduced from https://wiki.python.org/moin/BaseHttpServer

import time
import os
import http.server

# example of a python class
 
class MyHandler(http.server.BaseHTTPRequestHandler):
  def do_GET(s):
    """Respond to a GET request."""
    s.send_response(200)
    s.send_header(b"Content-type", "text/html")
    s.end_headers()
    s.wfile.write(b"<html><head><title>Title goes here.</title></head>")
    s.wfile.write(b"<body><p>This is a test.</p>")
    s.wfile.write(b"<p>You accessed path: %s</p>" % bytes(s.path))
    s.wfile.write(b"</body></html>")

host = os.getenv('IP', '0.0.0.0')
port = int(os.getenv('PORT', '8080'))
print(host, port)
httpd = http.server.HTTPServer((host, port), MyHandler)
httpd.serve_forever()

