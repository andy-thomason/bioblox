
# build molecules in examples.

import os
import re
import sys

print("Usage: build_all.py list|build|caonlyi|bsonly|sconly")


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
  ('1EXB.pdb', 'A', 'E', 3),
  ('1BVN.pdb', 'P', 'T', 3),
  ('1BUH.pdb', 'A', 'B', 3),
  ('1AY7.pdb', 'A', 'B', 3),
  ('1AVX.pdb', 'A', 'B', 3),
  ('1ATN.pdb', 'A', 'D', 3),
  ('1ACB.pdb', 'E', 'I', 3),
  ('1GRN.pdb', 'A', 'B', 3),
  ('1EMV.pdb', 'A', 'B', 3),
  ('1FSS.pdb', 'A', 'B', 3),
  ('4KC3.pdb', 'A', 'B', 3),
  ('2PTC.pdb', 'E', 'I', 3),
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

if len(sys.argv) == 2 and (sys.argv[1] == 'build' or sys.argv[1] == 'bsonly'):
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


if len(sys.argv) == 2 and sys.argv[1] == 'sconly':
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

