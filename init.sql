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
INSERT INTO responsable (login, pwd) VALUES ("manager", "9eec3b1d166aad069128af7bef229b672b5688ec2b9a65052d51af5f00c5f0e0");
