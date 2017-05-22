
# build molecules in examples.

import os
import re
import sys

print("Usage: build_vr.py list|build|caonlyi|bsonly")


pdbdir = '.' #os.path.dirname(sys.argv[0])

pdb_files = [f for f in os.listdir(pdbdir) if f[-4:] == '.pdb']

exe_file = 'gilgamol.exe'

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
  ('2PTC.pdb', 'E', 'I', 1),
  # ('4KC3.pdb', 'A', 'B', 2),
  # ('1FSS.pdb', 'A', 'B', 2),
  # ('1EMV.pdb', 'A', 'B', 2),
  # ('1GRN.pdb', 'A', 'B', 2),
  # ('1ACB.pdb', 'E', 'I', 2),
  # ('1ATN.pdb', 'A', 'D', 2),
  # ('1AVX.pdb', 'A', 'B', 2),
  # ('1AY7.pdb', 'A', 'B', 2),
  # ('1BUH.pdb', 'A', 'B', 2),
  # ('1BVN.pdb', 'P', 'T', 2),
  # ('1EXB.pdb', 'A', 'E', 2),
]

if len(sys.argv) == 2 and sys.argv[1] == 'build':
  # LOD 1 solvent excluded models
  for f, ch1, ch2, lod in chains:
    ex = "%s %s vr --chains %s --ply --lod %d" % (exe_file, pdbdir + '/' + f, ch1, lod)
    print(ex)
    for line in os.popen(ex):
      print(line)

    ex = "%s %s vr --chains %s --ply --lod %d" % (exe_file, pdbdir + '/' + f, ch2, lod)
    print(ex)
    for line in os.popen(ex):
      print(line)


"""
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


if len(sys.argv) == 2 and (sys.argv[1] == 'build' or sys.argv[1] == 'caonly'):
  for f, ch1, ch2, lod in chains:
    ex = "%s %s ca --chains %s --ply --lod 1" % (exe_file, pdbdir + '/' + f, ch1)
    print(ex)
    for line in os.popen(ex):
      print(line)

    ex = "%s %s ca --chains %s --ply --lod 1" % (exe_file, pdbdir + '/' + f, ch2)
    print(ex)
    for line in os.popen(ex):
      print(line)
"""

