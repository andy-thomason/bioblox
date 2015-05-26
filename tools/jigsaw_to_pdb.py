
#ATOM      1  N   ILE E  16      18.871  65.715  12.731  1.00 21.86           N  

atom = 1
for line in open(r"C:\Users\Andy\Google Drive\BioBlox\JigSawFive.ma"):
    if ".rp" in line:
        pos = line.split()[4:7]
        print("ATOM  %5d  N   ILE E  16     %7.3f %7.3f %7.3f  1.00 21.86           N " % (atom, float(pos[0]),float(pos[1]), float(pos[2])))
        atom = atom + 1

