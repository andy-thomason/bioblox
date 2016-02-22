# vim: tabstop=2 expandtab shiftwidth=2 softtabstop=2

import sys
import math
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

disable_cache = False
htdocs = os.path.expanduser('~/htdocs/')


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

atom_colours = {
  b'NZ  LYS': (1, 0, 0, 1),
  b'NH2 ARG': (1, 0, 0, 1),
  
  b'OE1 GLU': (0, 0, 1, 1),
  b'OE2 GLU': (0, 0, 1, 1),
  b'OD1 ASP': (0, 0, 1, 1),
  b'OD2 ASP': (0, 0, 1, 1),
  
  b'SG  CYS': (1, 1, 0, 1),
  
  b'OG  SER': (0, 1, 1, 1),
  b'OG2 THR': (0, 1, 1, 1),
  b'OD1 ASN': (0, 1, 1, 1),
  b'OE1 GLN': (0, 1, 1, 1),

  b'CB  ALA': (0, 0, 0, 1),
  b'CG2 VAL': (0, 0, 0, 1),
  b'CD1 ILE': (0, 0, 0, 1),
  b'CD2 LEU': (0, 0, 0, 1),
  b'CE  MET': (0, 0, 0, 1),
  b'CZ  PHE': (0, 0, 0, 1),
  b'OH  TYR': (0, 0, 0, 1),
  b'CH2 TRP': (0, 0, 0, 1),
}

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
    self.atom_colours = array.array('f')
    self.minxyz = (0, 0, 0)
    self.maxxyz = (0, 0, 0)
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

  def make_mesh(self, filename, resolution):
    bytes = thumbnail.make_mesh(self.pos, self.radii, self.atom_colours, resolution)
    if len(bytes):
      with open(filename, 'wb') as file:
        file.write(bytes)
      pass

  def add(self, mol, x, y):
    self.serial += mol.serial
    self.name += mol.name
    self.alt_loc += mol.alt_loc
    self.res += mol.res
    self.chain += mol.chain
    self.res_seq += mol.res_seq
    self.ins += mol.ins
    beg = len(self.pos)
    self.pos += mol.pos
    end = len(self.pos)
    pos = self.pos
    minx, miny, minz = self.minxyz
    maxx, maxy, maxz = self.maxxyz
    for i in range(beg, end, 3):
      pos[i] += x
      pos[i+1] += y
      minx = min(pos[i], minx)
      miny = min(pos[i+1], miny)
      minz = min(pos[i+2], minz)
      maxx = max(pos[i], maxx)
      maxy = max(pos[i+1], maxy)
      maxz = max(pos[i+2], maxz)
    self.radii += mol.radii
    self.minxyz = (minx, miny, minz)
    self.maxxyz = (maxx, maxy, maxz)


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

      id = line[13:20]
      colour = atom_colours[id] if id in atom_colours else (1, 1, 1, 1)
      #print(id, colour)

      mol.atom_colours.append(colour[0])
      mol.atom_colours.append(colour[1])
      mol.atom_colours.append(colour[2])
      mol.atom_colours.append(colour[3])

      #print("[" + line[12:20] + "] (" + str(colour) + ")")
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
    rfile = open(htdocs + '/data/names.txt', 'rb')
  except:
    req = urllib.request.Request('ftp://ftp.wwpdb.org/pub/pdb/derived_data/index/entries.idx')
    with urllib.request.urlopen(req) as req:
      x_pos = 50
      y_pos = 50
      with open(htdocs + '/data/names.txt', 'wb') as wfile:
        for line in req:
          splt = line.split(b'\t')
          if len(splt) >= 2 and splt[1][0:7] == b'COMPLEX':
            x_pos += 100
            if x_pos > 1000:
              y_pos += 100
              x_pos -= 1000
            name = splt[0].decode().lower()
            wfile.write(('%s,%f,%f\n' % (name, x_pos, y_pos)).encode())
    rfile = open(htdocs + '/data/names.txt', 'rb')


  for line in rfile:
    split = line[:-1].split(b',')
    names.append((split[0], float(split[1]), float(split[2])))

  print(names)
  for name, x, y in names:
    name = name.decode()
    local_name = htdocs + '/pdb/%s.pdb' % name
    try:
      os.stat(local_name)
    except:
      print('reading pdb %s' % ('http://www.rcsb.org/pdb/files/%s.pdb' % name))
      req = urllib.request.Request('http://www.rcsb.org/pdb/files/%s.pdb' % name)
      with urllib.request.urlopen(req) as req:
        print(req)
        pdb_file = req.read()
        with open(local_name, 'wb') as file:
          file.write(pdb_file)

  map_name = htdocs + '/data/map.png'
  try:
    os.stat(map_name)
  except:
    # build a megamolecule!  
    tot = PDB_molecule()
    for name, x, y in names:
      mols = get_mols(name)
      for mol in mols:
        tot.add(mol, x, y)
    tot.make_thumbnail(map_name, 2048, 2048)
    
def get_mols(name):
  if type(name) is bytes: name = name.decode()
  local_name = htdocs + '/pdb/%s.pdb' % name
  print('reading %s' % local_name)
  with open(local_name, 'rb') as file:
    pdb_file = file.read()
    mols = parse_pdb(io.BytesIO(pdb_file))
  return mols

def build_thumbnail(pdb, size):
  mols = get_mols(pdb)
  i = 0
  tot = PDB_molecule()
  for mol in mols:
    i = i + 1
    tot.add(mol, 0, 0)
  tot.make_thumbnail(htdocs + '/thumbnails/%s.%d.png' % (pdb, size), size, size)

def build_mesh(pdb, index, resolution):
  print("build_mesh res=%d" % resolution)
  mols = get_mols(pdb)
  if index < len(mols):
    filename = htdocs + 'mesh/%s.%d.%d.bin' % (pdb, index, resolution)
    mols[index].make_mesh(filename, resolution)

thumbnails_png_re = re.compile('^/thumbnails/(\\w+)\.([0-9]+)\.png$')
mesh_re = re.compile('^/mesh/(\\w+)\.([0-9]+)\.([0-9]+)\.bin$')
data_re = re.compile('^/data/(\\w+)\..*$')
pdb_re = re.compile('^/pdb/(\\w+)\.pdb$')

class MyHandler(http.server.BaseHTTPRequestHandler):
  def serve_file(self, prefix, path):
    try:
      with open(prefix + path, 'rb') as rf:
        print("serving %s" % (prefix+path));
        self.send_response(200)
        if path[-4:] == '.bin':
          self.send_header(b"Content-type", "application/octet-stream")
        elif path[-4:] == '.png':
          self.send_header(b"Content-type", "image/png")
        else:
          self.send_header(b"Content-type", "text/plain")
        self.end_headers()
        self.wfile.write(rf.read())
        return True
    except:
      print("failed to find %s" % (prefix+path));
    return False

  def make_on_demand(self, path):
    print('making ' + path + ' on demand')
    
    match_thumbnail = thumbnails_png_re.match(path)
    match_mesh = mesh_re.match(path)
    #match_data = data_re.match(path)
    #match_pdb = pdb_re.match(path)

    if match_thumbnail:
      pdb = str(match_thumbnail.group(1))
      size = int(match_thumbnail.group(2))
      build_thumbnail(pdb, size)
    elif match_mesh:
      pdb = str(match_mesh.group(1))
      index = int(match_mesh.group(2))
      resolution = int(match_mesh.group(3))
      build_mesh(pdb, index, resolution)

  def do_GET(self):
    print("GET %s" % self.path)
    
    path = '/index.html' if self.path == '/' else self.path; 
    
    if '..' in path:
      self.send_response(404)
      self.end_headers()
      return

    if self.serve_file(htdocs, path):
      return
    
    if self.serve_file('htdocs/', path):
      return

    self.make_on_demand(path)

    if self.serve_file(htdocs, path):
      return

    print('failed after make on demand')
    self.send_response(404)
    self.end_headers()
    return

def make_dir(path):
  try:
    os.makedirs(path)
  except:
    pass

def main(argv):
  make_dir(htdocs + '/data')
  make_dir(htdocs + '/pdb')
  make_dir(htdocs + '/thumbnails')
  make_dir(htdocs + '/mesh')

  download_entries()
  
  host = os.getenv('IP', '0.0.0.0')
  port = int(os.getenv('PORT', '443'))
  print("serving on %s:%d" % (host, port))
  httpd = http.server.HTTPServer((host, port), MyHandler)
  
  #http://pankajmalhotra.com/Simple-HTTPS-Server-In-Python-Using-Self-Signed-Certs/
  if port == 443:
    httpd.socket = ssl.wrap_socket(httpd.socket, certfile='bioblox.key', server_side=True)

  httpd.serve_forever()

if __name__ == "__main__":
  main(sys.argv)

