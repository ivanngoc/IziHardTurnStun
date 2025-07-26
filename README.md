# CLI
create certs: 
openssl req -x509 -newkey rsa:4096 -keyout turn_server_pkey.pem -out turn_server_cert.pem -days 365 -nodes -subj "/CN=izihardgames.com"


# create db schema
https://github.com/coturn/coturn/blob/master/docs/PostgreSQL.md
https://github.com/coturn/coturn/blob/master/turndb/schema.sql
turnadmin -a \
  -u admin \
  -p your_password \
  -r myrealm \
  -e "host=192.168.0.3 port=5432 dbname=coturn_db user=postgres password=postgres"


turnadmin -k -u admin -r myrealm -p admin
0xc375eef6af72685543809f7a523a8b61


INSERT INTO admin_user (name, realm, password, password_hash)
VALUES (
  'admin',
  'myrealm',
  'mypassword',
  'sha256:0xc375eef6af72685543809f7a523a8b61'
);


OR if no password_hash column
INSERT INTO admin_user (name, realm, password)
VALUES (
  'admin',
  'myrealm',
  'mypassword'
);