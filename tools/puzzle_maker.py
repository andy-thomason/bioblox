import sys

atom = 1
for mol in range(0, 2):
  for x in range(-10, 10+1):
    for y in range(-20, 20+1):
      for z in range(-2, 2+1):
        fx = x * 3
        fy = y * 3
        fz = z * 3
        if y >= 0 and (x - 5) * (x - 5) + (y - 1) * (y - 1) > 15:
          if mol == 0:
            print("ATOM  %5d  N   ILE E  16     %7.3f %7.3f %7.3f  1.00 21.86           N " % (atom, fx, fy, fz))
            atom += 1
        else:
          if mol == 1:
            print("ATOM  %5d  N   ILE E  16     %7.3f %7.3f %7.3f  1.00 21.86           N " % (atom, fx, fy, fz))
            atom += 1
  print("TER     ");

