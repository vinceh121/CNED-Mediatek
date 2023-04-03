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
	login VARCHAR(64) NOT NULL,
	pwd VARCHAR(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS absences (
	datedebut DATETIME NOT NULL,
	datefin DATETIME,
	idpersonnel INT NOT NULL,
	idmotif INT NOT NULL,
	FOREIGN KEY (idpersonnel) REFERENCES personnel (idpersonnel),
	FOREIGN KEY (idmotif) REFERENCES motif (idmotif),
	PRIMARY KEY (datedebut)
);

-- insertion of defaults

INSERT INTO service (nom) VALUES ("Administratif"),("Médiation culturelle"),("Prêt");
INSERT INTO motif (libelle) VALUES ("Vacances"),("Maladie"),("Motif familial"),("Congé parental");

