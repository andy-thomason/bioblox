# vim: tabstop=2 expandtab shiftwidth=2 softtabstop=2

import sys
import array
import io
import re
import urllib.request
from PIL import Image
import time
import os
import http.server
import ssl

# our extensions
import thumbnail


"""
 ATOM AND CONECT LINE FORMATS
COLUMNS        DATA TYPE       CONTENTS                            
--------------------------------------------------------------------------------
1 -  6         Record name     "ATOM  "                                            
7 - 11         Integer         Atom serial number.                   
13 - 16        Atom            Atom name.                            
17             Character       Alternate location indicator.         
18 - 20        Residue name    Residue name.                         
22             Character       Chain identifier.                     
23 - 26        Integer         Residue sequence number.              
27             AChar           Code for insertion of residues.       
31 - 38        Real(8.3)       Orthogonal coordinates for X in Angstroms.                       
39 - 46        Real(8.3)       Orthogonal coordinates for Y in Angstroms.                            
47 - 54        Real(8.3)       Orthogonal coordinates for Z in Angstroms.                            
55 - 60        Real(6.2)       Occupancy.                            
61 - 66        Real(6.2)       Temperature factor (Default = 0.0).                   
73 - 76        LString(4)      Segment identifier, left-justified.   
77 - 78        LString(2)      Element symbol, right-justified.      
79 - 80        LString(2)      Charge on the atom. 
COLUMNS       DATA TYPE       FIELD         DEFINITION
-------------------------------------------------------
1 -  6        Record name     "CONECT"
7 - 11        Integer         serial        Atom serial number
12 - 16       Integer         serial        Serial number of bonded atom
17 - 21       Integer         serial        Serial number of bonded atom
22 - 26       Integer         serial        Serial number of bonded atom
27 - 31       Integer         serial        Serial number of bonded atom

"""

# Reference: glMol / A. Bondi, J. Phys. Chem., 1964, 68, 441.
radii = {
  b" H" :  1.2, b"LI" :  1.82, b"NA" :  2.27, b" K" :  2.75, b" C" :  1.7, b" N" :  1.55, b" O" :  1.52,
  b" F" :  1.47, b" P" :  1.80, b" S" :  1.80, b"CL" :  1.75, b"BR" :  1.85, b"SE" :  1.90,
  b"ZN" :  1.39, b"CU" :  1.4, b"NI" :  1.63,
};

class PDB_molecule:
  def __init__(self):
    self.serial = array.array('I')
    self.name = []
    self.alt_loc = []
    self.res = []
    self.chain = array.array('B')
    self.res_seq = []
    self.ins = []
    self.pos = array.array('f')
    self.radii = array.array('f')
    #self.occ = []
    #self.tf = []
    #self.seg_id = []
    #self.esym = []
    #self.charge = []

  def make_thumbnail(self, filename, width, height):
    image = bytearray(width*height*3)
    thumbnail.make_thumbnail(image, self.pos, self.radii, self.chain, width, height)
    img = Image.frombytes('RGB', (width, height), bytes(image));
    img.save(filename)
    #svg_file.write('<svg version="1.1" baseProfile="full" width="300" height="200" xmlns="http://www.w3.org/2000/svg">\n')
    #svg_file.write('  <circle cx="50" cy="50" r="40" stroke="black" stroke-width="3" fill="red" />\n')
    #svg_file.write('</svg>\n')

  def add(self, mol):
    self.serial += mol.serial
    self.name += mol.name
    self.alt_loc += mol.alt_loc
    self.res += mol.res
    self.chain += mol.chain
    self.res_seq += mol.res_seq
    self.ins += mol.ins
    self.pos += mol.pos
    self.radii += mol.radii


def parse_pdb(file):
  mol = PDB_molecule()
  result = []
  start = True

  for line in file:
    if line[0:3] == b'TER':
      mol = PDB_molecule()
      start = True
      
    elif line[0:6] == b'ATOM  ':
      if start:
        result.append(mol)
        start = False
      mol.serial.append( int(line[6:11]) )
      mol.name.append(line[12:16])
      mol.alt_loc.append(line[16])
      mol.res.append(line[17:20])
      mol.chain.append(line[21])
      mol.res_seq.append(int(line[22:26]))
      mol.ins.append(line[26])
      mol.pos.append(float(line[30:38]) )
      mol.pos.append(float(line[38:46]) )
      mol.pos.append(float(line[46:54]) )
      mol.radii.append(radii[line[76:78]])
      #mol.occ.append(line[54:60])
      #mol.tf.append(line[60:66])
      #mol.seg_id.append(line[72:76])
      #mol.esym.append(line[76:78])
      #mol.charge.append(line[78:80])

      #print("(%s) ) (%s) ) (%s) ) %f %f %f" % (serial, name, res, x, y, z))
      #break
  return result

names = []

# the entries.txt file is a tab separated list of names and references of PDB entries.
def download_entries():
  try:
    rfile = open('data/names.txt', 'rb')
  except:
    req = urllib.request.Request('ftp://ftp.wwpdb.org/pub/pdb/derived_data/index/entries.idx')
    with urllib.request.urlopen(req) as req:
      with open('data/names.txt', 'wb') as wfile:
        for line in req:
          splt = line.split(b'\t')
          if len(splt) >= 2 and splt[1][0:7] == b'COMPLEX':
            wfile.write(splt[0].lower() + b'\n')
    rfile = open('data/names.txt', 'rb')
  for line in rfile:
    names.append(line[:-1])
  print(names)



def build_resources(pdb):
  print('reading %s' % ('http://www.rcsb.org/pdb/files/%s.pdb' % pdb))
  req = urllib.request.Request('http://www.rcsb.org/pdb/files/%s.pdb' % pdb)
  with urllib.request.urlopen(req) as req:
    pdb_file = req.read()
    mols = parse_pdb(io.BytesIO(pdb_file))

  width = 1024
  height = 1024
  i = 0
  tot = PDB_molecule()
  for mol in mols:
    #mol.make_thumbnail('%s.%d.png' % (pdb, i), width, height)
    i = i + 1
    tot.add(mol)

  tot.make_thumbnail('thumbnails/%s.png' % (pdb), width, height)
  # pdb = '4hhb';
  # pdb = '5cdo';
  # pdb = '2ptc';
  # pdb = '1e79';

thumbnails_png_re = re.compile('^/thumbnails/(\\w+)\.png$')
mesh_txt_re = re.compile('^/mesh/(\\w+)\.txt$')

class MyHandler(http.server.BaseHTTPRequestHandler):
  def do_GET(s):
    print(s.path)
    if s.path == '/':
      s.send_response(200)
      s.send_header(b"Content-type", "text/html")
      s.end_headers()
      s.wfile.write(b"<html>\n<head>\n<title>Bioblox data server</title>\n</head>")
      s.wfile.write(b"<body>\n")
      s.wfile.write(b"<h1>Bioblox data server</h1>\n")
      s.wfile.write(b"<h3>Example urls:</h3>\n")
      s.wfile.write(b"<p><a href='/data/names.txt'>/data/names.txt</p>\n")
      s.wfile.write(b"<p><a href='/thumbnails/2ptc.png'>/thumbnails/2ptc.png</p>\n")
      s.wfile.write(b"<p><a href='/mesh/2ptc.txt'>/mesh/2ptc.txt</p>\n")
      s.wfile.write(b"<p><a href='/mesh/2ptc.1.vertics'>/mesh/2ptc.1.vertics</p>\n")
      s.wfile.write(b"<p><a href='/mesh/2ptc.1.colors'>/mesh/2ptc.1.colors</p>\n")
      s.wfile.write(b"</body></html>\n")
    elif s.path[0:12] == '/thumbnails/' or s.path[0:6] == '/mesh/' or s.path[0:6] == '/data/':
      s.send_response(200)
      if s.path[-4:] == '.png':
        s.send_header(b"Content-type", "image/png")
      else:
        s.send_header(b"Content-type", "text/plain")
      s.end_headers()
      try:
        with open(s.path[1:], 'rb') as rf:
          s.wfile.write(rf.read())
      except:
        m = thumbnails_png_re.match(s.path)
        if m:
          build_resources(str(m.group(1)))
        pass
      with open(s.path[1:], 'rb') as rf:
        s.wfile.write(rf.read())
    else:
      s.send_response(404)
      s.end_headers()
        

def main(argv):
  # example of a python class
  try:
    os.mkdir('data')
  except:
    pass
  try:
    os.mkdir('thumbnails')
  except:
    pass
  try:
    os.mkdir('mesh')
  except:
    pass

  download_entries()
  
  host = os.getenv('IP', '0.0.0.0')
  port = int(os.getenv('PORT', '8080'))
  print(host, port)
  httpd = http.server.HTTPServer((host, port), MyHandler)
  #httpd.socket = ssl.wrap_socket(httpd.socket)
  print("serving")
  httpd.serve_forever()

if __name__ == "__main__":
  main(sys.argv)

