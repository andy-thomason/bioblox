
# build molecules in examples.

import os
import re
import sys

print("Usage: build_all.py list|build")


pdbdir = '.' #os.path.dirname(sys.argv[0])

pdb_files = [f for f in os.listdir(pdbdir) if f[-4:] == '.pdb']

exe_file = 'molecules.exe'

if len(sys.argv) == 2 and sys.argv[1] == 'list':
  chains_re = re.compile("chains: ([A-Z]+)")
  # list the chains
  for f in pdb_files:
    ex = "%s %s --list-chains" % (exe_file, pdbdir + '/' + f)
    #print(ex)
    lines = [line for line in os.popen(ex)]

    for line in lines:
      m = chains_re.match(line)
      if m:
        chains = m.group(1)
        #if len(chains) == 2:
        print("  ('%s', '%s', '%s', 1)," %( f, chains[:-1], chains[-1]))
        
  sys.exit()

chains = [
  ('1ACB.pdb', 'E', 'I', 1),
  ('1ATN.pdb', 'A', 'D', 1),
  ('1AVX.pdb', 'A', 'B', 1),
  ('1AY7.pdb', 'A', 'B', 1),
  ('1BUH.pdb', 'A', 'B', 1),
  ('1BVN.pdb', 'P', 'T', 1),
  ('1CGI.pdb', 'E', 'I', 1),
  ('1CLV.pdb', 'A', 'I', 1),
  ('1D6R.pdb', 'A', 'I', 1),
  ('1DFJ.pdb', 'E', 'I', 1),
  ('1E96.pdb', 'A', 'B', 1),
  ('1EMV.pdb', 'A', 'B', 1),
  ('1EXB.pdb', 'A', 'E', 1),
  ('1F34.pdb', 'A', 'B', 1),
  ('1FC2.pdb', 'C', 'D', 1),
  ('1FLE.pdb', 'E', 'I', 1),
  ('1FQ1.pdb', 'A', 'B', 1),
  ('1FSS.pdb', 'A', 'B', 1),
  ('1GRN.pdb', 'A', 'B', 1),
  ('1H1V.pdb', 'A', 'G', 1),
  ('1HE8.pdb', 'A', 'B', 1),
  ('1IRA.pdb', 'X', 'Y', 1),
  ('1J2J.pdb', 'A', 'B', 1),
  ('1JIW.pdb', 'I', 'P', 1),
  ('1JTD.pdb', 'A', 'B', 1),
  ('1KXP.pdb', 'A', 'D', 1),
  ('1M10.pdb', 'A', 'B', 1),
  ('1NW9.pdb', 'A', 'B', 1),
  ('1OC0.pdb', 'A', 'B', 1),
  ('1OHZ.pdb', 'A', 'B', 1),
  ('1OPH.pdb', 'A', 'B', 1),
  ('1PPE.pdb', 'E', 'I', 1),
  ('1R0R.pdb', 'E', 'I', 1),
  ('1R8S.pdb', 'A', 'E', 1),
  ('1RKE.pdb', 'A', 'B', 1),
  ('1T6B.pdb', 'X', 'Y', 1),
  ('1TMQ.pdb', 'A', 'B', 1),
  ('1UDI.pdb', 'E', 'I', 1),
  ('1US7.pdb', 'A', 'B', 1),
  ('1WQ1.pdb', 'G', 'R', 1),
  ('1Y64.pdb', 'A', 'B', 1),
  ('1YVB.pdb', 'A', 'I', 1),
  ('1Z5Y.pdb', 'D', 'E', 1),
  ('1ZHH.pdb', 'A', 'B', 1),
  ('1ZHI.pdb', 'A', 'B', 1),
  ('1ZLI.pdb', 'A', 'B', 1),
  ('2A9K.pdb', 'A', 'B', 1),
  ('2AYO.pdb', 'A', 'B', 1),
  ('2B42.pdb', 'A', 'B', 1),
  ('2BTF.pdb', 'A', 'P', 1),
  ('2C0L.pdb', 'A', 'B', 1),
  ('2FJU.pdb', 'A', 'B', 1),
  ('2G77.pdb', 'A', 'B', 1),
  ('2GAF.pdb', 'A', 'D', 1),
  ('2HLE.pdb', 'A', 'B', 1),
  ('2HRK.pdb', 'A', 'B', 1),
  ('2NZ8.pdb', 'A', 'B', 1),
  ('2O3B.pdb', 'A', 'B', 1),
  ('2OOB.pdb', 'A', 'B', 1),
  ('2OT3.pdb', 'A', 'B', 1),
  ('2OUL.pdb', 'A', 'B', 1),
  ('2OZA.pdb', 'A', 'B', 1),
  ('2PTC.pdb', 'E', 'I', 1),
  ('2SNI.pdb', 'E', 'I', 1),
  ('2UUY.pdb', 'A', 'B', 1),
  ('2VDB.pdb', 'A', 'B', 1),
  ('2Z0E.pdb', 'A', 'B', 1),
  ('3DAW.pdb', 'A', 'B', 1),
  ('3F1P.pdb', 'A', 'B', 1),
  ('3FN1.pdb', 'A', 'B', 1),
  ('3SGQ.pdb', 'E', 'I', 1),
  ('4FZA.pdb', 'A', 'B', 1),
  ('4H03.pdb', 'A', 'B', 1),
  ('4KC3.pdb', 'A', 'B', 1),
  ('4M76.pdb', 'A', 'B', 1),
  ('7CEI.pdb', 'A', 'B', 1),
]

if len(sys.argv) == 2 and sys.argv[1] == 'build':
  # thumbnail meshes
  for f, ch1, ch2, lod in chains:
    ex = "%s %s se --chains %s%s --ply --lod 0" % (exe_file, pdbdir + '/' + f, ch1, ch2)
    print(ex)
    for line in os.popen(ex):
      print(line)

  # LOD 1 solvent excluded models
  for f, ch1, ch2, lod in chains:
    ex = "%s %s se --chains %s --ply --lod %d" % (exe_file, pdbdir + '/' + f, ch1, lod)
    print(ex)
    for line in os.popen(ex):
      print(line)

    ex = "%s %s se --chains %s --ply --lod %d" % (exe_file, pdbdir + '/' + f, ch2, lod)
    print(ex)
    for line in os.popen(ex):
      print(line)

  # ball and stick models
  for f, ch1, ch2, lod in chains:
    ex = "%s %s bs --chains %s --ply --lod 1" % (exe_file, pdbdir + '/' + f, ch1)
    print(ex)
    for line in os.popen(ex):
      print(line)

    ex = "%s %s bs --chains %s --ply --lod 1" % (exe_file, pdbdir + '/' + f, ch2)
    print(ex)
    for line in os.popen(ex):
      print(line)

    ex = "%s %s ca --chains %s --ply --lod 1" % (exe_file, pdbdir + '/' + f, ch1)
    print(ex)
    for line in os.popen(ex):
      print(line)

    ex = "%s %s ca --chains %s --ply --lod 1" % (exe_file, pdbdir + '/' + f, ch2)
    print(ex)
    for line in os.popen(ex):
      print(line)

