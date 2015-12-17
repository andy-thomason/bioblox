# vim: tabstop=2 expandtab shiftwidth=2 softtabstop=2

import urllib2
import array
from PIL import Image
from PIL import PSDraw

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
  " H" :  1.2, "LI" :  1.82, "NA" :  2.27, " K" :  2.75, " C" :  1.7, " N" :  1.55, " O" :  1.52,
  " F" :  1.47, " P" :  1.80, " S" :  1.80, "CL" :  1.75, "BR" :  1.85, "SE" :  1.90,
  "ZN" :  1.39, "CU" :  1.4, "NI" :  1.63,
};

class PDB_molecule:
  def __init__(self):
    self.serial = array.array('I')
    self.name = []
    self.alt_loc = []
    self.res = []
    self.chain = []
    self.res_seq = []
    self.ins = []
    self.pos = array.array('f')
    #self.occ = []
    #self.tf = []
    #self.seg_id = []
    #self.esym = []
    #self.charge = []

def parse_pdb(file):
  mol = PDB_molecule()
  result = []
  start = True

  for line in file:
    if line[0:3] == 'TER':
      mol = PDB_molecule()
      
    elif line[0:6] == 'ATOM  ':
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
      mol.pos.append(radii[line[76:78]])
      #mol.occ.append(line[54:60])
      #mol.tf.append(line[60:66])
      #mol.seg_id.append(line[72:76])
      #mol.esym.append(line[76:78])
      #mol.charge.append(line[78:80])

      #print("(%s) ) (%s) ) (%s) ) %f %f %f" % (serial, name, res, x, y, z))
      #break
  return result

  def draw(self, svg_file):
    svg_file.write('<svg version="1.1" baseProfile="full" width="300" height="200" xmlns="http://www.w3.org/2000/svg">\n')
    svg_file.write('  <circle cx="50" cy="50" r="40" stroke="black" stroke-width="3" fill="red" />\n')
    svg_file.write('</svg>\n')


def main():
  pdb_file = urllib2.urlopen('http://www.rcsb.org/pdb/files/4hhb.pdb')
  mols = parse_pdb(pdb_file)
  print(mols[0].chain)
  #mol.draw(open('1.svg', 'wb'))
  

if __name__ == "__main__":
  main()

