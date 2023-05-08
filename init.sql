CREATE TABLE IF NOT EXISTS service (
	idservice INT NOT NULL AUTO_INCREMENT,
	nom TEXT NOT NULL UNIQUE,
	PRIMARY KEY (idservice)
);

CREATE TABLE IF NOT EXISTS personnel (
	idpersonnel INT NOT NULL AUTO_INCREMENT,
	nom TINYTEXT,
	prenom TINYTEXT,
	tel TINYTEXT UNIQUE,
	mail TINYTEXT UNIQUE,
	idservice INT NOT NULL REFERENCES service(idservice),
	PRIMARY KEY (idpersonnel)
);

CREATE TABLE IF NOT EXISTS motif (
	idmotif INT NOT NULL AUTO_INCREMENT,
	libelle TEXT NOT NULL UNIQUE,
	PRIMARY KEY (idmotif)
);

CREATE TABLE IF NOT EXISTS responsable (
	login VARCHAR(64) UNIQUE NOT NULL,
	pwd VARCHAR(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS absences (
	idabsence INT NOT NULL AUTO_INCREMENT,
	datedebut DATETIME NOT NULL,
	datefin DATETIME NOT NULL,
	idpersonnel INT NOT NULL,
	idmotif INT NOT NULL,
	FOREIGN KEY (idpersonnel) REFERENCES personnel (idpersonnel)
	ON DELETE CASCADE,
	FOREIGN KEY (idmotif) REFERENCES motif (idmotif),
	PRIMARY KEY (idabsence),
	CONSTRAINT UC_ABSPERSO UNIQUE (idabsence, idpersonnel)
);

-- insertion of defaults

INSERT INTO service (nom) VALUES ("Administratif"),("Médiation culturelle"),("Prêt");
INSERT INTO motif (libelle) VALUES ("Vacances"),("Maladie"),("Motif familial"),("Congé parental");

-- default admin user
-- default password: Tj7WDuvIHXUb
-- password hash format: salt$b64hash
-- salt is a pseudo-random 19 char string
-- hash is SHA256 encoded in base64
INSERT INTO responsable (login, pwd) VALUES ("manager", "Xq0UDOyJjEdQZ5fzvWB$DWgq2V9kgYXDXt5Cf4rSyBHydTlxvoSDvVCAUx/COVo=");
