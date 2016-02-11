openssl genrsa -des3 -out server.key 1024
openssl req -new -key server.key -out server.csr
openssl x509 -req -days 1024 -in server.csr -signkey server.key -out server.crt
cat server.crt server.key > server.pem
rm server.crt
rm server.key


#openssl genrsa -out key.pem
#openssl req -new -key key.pem -out csr.pem
#openssl x509 -req -days 9999 -in csr.pem -signkey key.pem -out cert.pem
#rm csr.pem
