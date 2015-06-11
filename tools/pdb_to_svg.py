import sys

print(r'<?xml version="1.0" encoding="UTF-8" standalone="no"?>')
print(r'<svg xmlns="http://www.w3.org/2000/svg" width="1000" height="1000">')

offx = -15
offy = -70
scale = 40

colour = '#f00'
for line in open(sys.argv[1]):
  if line[0:6] == "ATOM  ":
    #ATOM     32  CE1 TYR E  20      20.179  53.737  13.661  1.00 28.18           C  
    #print('[' + line[30:38] + ']')
    n = int(line[6:11])
    x = (offx + float(line[30:38])) * scale + 500
    y = (offy + float(line[38:46])) * scale + 500
    z = float(line[46:54])
    r = scale * 1.5
    #if (n >= 1749-3 and n <= 1749+3) or (n >= 1259-3 and n <= 1259+3):
    #if (n >= 1741 and n <= 1749) or (n >= 1254 and n <= 1259):
    print(r'<circle cx="%f" cy="%f" r="%f" fill="none" stroke="%s" stroke-width="0.5"/>' % (x, y, r, colour))
    print(r'<text x="%f" y="%f" fill="%s">%d</text>' % (x, y, colour, n))
  elif line[0:6] == "TER   ":
    colour = '#00f'

print(r'</svg>')
