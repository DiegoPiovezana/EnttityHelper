
CREATE TABLE TB_SR_USERS__USERS(
Id VARCHAR2(20),
Email VARCHAR2(50),
Login VARCHAR2(20),
Name VARCHAR2(500),
Active NUMBER(1),
DtCreation TIMESTAMP,
DtLastLogin TIMESTAMP,
DtActivation TIMESTAMP,
DtDeactivation TIMESTAMP,
DtAlteration TIMESTAMP,
DtRevision TIMESTAMP,
InternalUser VARCHAR2(1),
IdSupervisor VARCHAR2(20),
IdGroup NUMBER,
IdOrigin NUMBER,
IdCarrer NUMBER,
IdServiceCreation NUMBER
);


CREATE TABLE TB_SR_USERS__USERS(
    Id VARCHAR2(100), 
    Email VARCHAR2(100), 
    Login VARCHAR2(100), 
    Name VARCHAR2(100), 
    Active NUMBER(1), 
    DtCreation TIMESTAMP, 
    DtLastLogin TIMESTAMP, 
    DtActivation TIMESTAMP, 
    DtDeactivation TIMESTAMP, 
    DtAlteration TIMESTAMP, 
    DtRevision TIMESTAMP, 
    InternalUser VARCHAR2(100), 
    IdSupervisor VARCHAR2(100)
    );

DROP TABLE TB_SR_USERS__USERS;
DELETE FROM TB_SR_USERS__USERS;


SELECT * FROM TB_SR_USERS__USERS;
SELECT * FROM TB_SR_USERS__USERS WHERE (1 = 1);

INSERT INTO TB_SR_USERS__USERS (Id, Email, Login, Name, Active, DtCreation, DtLastLogin, DtActivation, DtDeactivation, DtAlteration, DtRevision, InternalUser, IdSupervisor) VALUES ('usuario.primeiro', 'usuario.primeiro@gmail.com', 'usuario.primeiro', 'Usuário Teste 1 via banco', '1', '12/09/2023 00:17:56', '', '12/09/2023 00:17:56', '', '', '', 'Y', '');

INSERT INTO TB_SR_USERS__USERS (Id, Email, Login, Name, Active, DtCreation, DtLastLogin, DtActivation, DtDeactivation, DtAlteration, DtRevision, InternalUser, IdSupervisor) VALUES ('usuario.segundo', 'usuario.segundo@hotmail.com', '8085858555455454', 'Usuário Teste 2 via banco', '1',  '06/11/2023 17:10:05', '', '06/11/2023 17:10:05', '', '', '', 'Y', 'usuario.primeiro');

INSERT INTO TB_SR_USERS__USERS (Id, Email, Login, Name, Active, DtCreation, DtLastLogin, DtActivation, DtDeactivation, DtAlteration, DtRevision, InternalUser, IdSupervisor) VALUES ('usuario.terceiro', 'usuario.terceiro@hotmail.com', '8085858555455454', 'Usuário Teste 2 via banco', '1',  '06/11/2023 17:10:05', '', '06/11/2023 17:10:05', '', '', '', 'Y', 'usuario.primeiro');

COMMIT;

--UPDATE TB_SR_USERS__USERS SET IDSUPERVISOR = 0 WHERE IDUSUARIO = 1;

--DELETE FROM TB_SR_USERS__USERS WHERE IdUsuario = 1 AND ROWNUM = 1;

--SELECT * FROM TB_SR_USERS__USERS WHERE IdUsuario = 1;

--DROP TABLE TB_SR_USERS__USERS;



-------------------- COPIAR TABELA ------------------

CREATE TABLE TB_SR_USERS__USERS_TEMP AS SELECT * FROM TB_SR_USERS__USERS;
DROP TABLE TB_SR_USERS__USERS;
-- (CRIA TB_SR_USERS__USERS TABELA NO C#)

INSERT INTO TB_SR_USERS__USERS 
(Id, Email, Login, Name, Active, DtCreation, DtLastLogin, DtActivation, DtDeactivation, DtAlteration, DtRevision, InternalUser, IdSupervisor, IdCareer)
SELECT * FROM TB_SR_USERS__USERS_TEMP;

SELECT * FROM TB_SR_USERS__USERS;
SELECT * FROM TB_SR_USERS__USERS_TEMP;


-----------------------------------------------------


SELECT
    OWNER,
    OBJECT_NAME,
    OBJECT_TYPE
FROM
    ALL_OBJECTS
-- WHERE
--     OBJECT_NAME = 'TB_ENTITY_TEST'
;


---------------------

CREATE TABLE TB_SR_USERS__CAREERS (
    IdCareer NUMBER(10) PRIMARY KEY, 
    Name NVARCHAR2(200), 
    CareerLevel NUMBER, 
    Active NUMBER(1)
    );

DROP TABLE TB_SR_USERS__CAREERS;

-- TB_SR_USERS__CAREERS
SELECT * FROM TB_SR_USERS__CAREERS;



SELECT * FROM TB_SR_USERS__CAREERS WHERE (IdCareer='1');


-----------------------


SELECT * FROM TB_SR_USERS__GROUPS;
DROP TABLE TB_SR_USERS__GROUPS;






------------------------------

-- TB_SR_USERS__ORIGENS

CREATE TABLE TB_SR_USERS__ORIGENS(
IdOrigem NUMBER,
Identificacao VARCHAR2(100)(20),
Nome VARCHAR2(100)(500),
Ativo NUMBER(1)
);

INSERT INTO TB_SR_USERS__ORIGENS (IdOrigem, Identificacao, Nome, Ativo) VALUES ('0', '00000', 'Origem Teste via banco', '1');

SELECT * FROM TB_SR_USERS__ORIGENS;

SELECT * FROM TB_SR_USERS__USERS WHERE (IdOrigem=0);