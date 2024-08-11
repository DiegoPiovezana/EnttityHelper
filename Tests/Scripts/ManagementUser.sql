CREATE TABLE TB_SR_USERS__USERS(
    ID VARCHAR2(20),
    EMAIL VARCHAR2(50),
    LOGIN VARCHAR2(20),
    NAME VARCHAR2(500),
    ACTIVE NUMBER(1),
    DTCREATION TIMESTAMP,
    DTLASTLOGIN TIMESTAMP,
    DTACTIVATION TIMESTAMP,
    DTDEACTIVATION TIMESTAMP,
    DTALTERATION TIMESTAMP,
    DTREVISION TIMESTAMP,
    INTERNALUSER VARCHAR2(1),
    IDSUPERVISOR VARCHAR2(20),
    IDGROUP NUMBER,
    IDORIGIN NUMBER,
    IDCARRER NUMBER,
    IDSERVICECREATION NUMBER
);

CREATE TABLE TB_SR_USERS__USERS(
    ID VARCHAR2(100),
    EMAIL VARCHAR2(100),
    LOGIN VARCHAR2(100),
    NAME VARCHAR2(100),
    ACTIVE NUMBER(1),
    DTCREATION TIMESTAMP,
    DTLASTLOGIN TIMESTAMP,
    DTACTIVATION TIMESTAMP,
    DTDEACTIVATION TIMESTAMP,
    DTALTERATION TIMESTAMP,
    DTREVISION TIMESTAMP,
    INTERNALUSER VARCHAR2(100),
    IDSUPERVISOR VARCHAR2(100)
);

DROP TABLE TB_SR_USERS__USERS;

DELETE FROM TB_SR_USERS__USERS;

SELECT
    *
FROM
    TB_SR_USERS__USERS;

SELECT
    *
FROM
    TB_SR_USERS__USERS
WHERE
    (1 = 1);

INSERT INTO TB_SR_USERS__USERS (
    ID,
    EMAIL,
    LOGIN,
    NAME,
    ACTIVE,
    DTCREATION,
    DTLASTLOGIN,
    DTACTIVATION,
    DTDEACTIVATION,
    DTALTERATION,
    DTREVISION,
    INTERNALUSER,
    IDSUPERVISOR
) VALUES (
    'usuario.primeiro',
    'usuario.primeiro@gmail.com',
    'usuario.primeiro',
    'Usuário Teste 1 via banco',
    '1',
    '12/09/2023 00:17:56',
    '',
    '12/09/2023 00:17:56',
    '',
    '',
    '',
    'Y',
    ''
);

INSERT INTO TB_SR_USERS__USERS (
    ID,
    EMAIL,
    LOGIN,
    NAME,
    ACTIVE,
    DTCREATION,
    DTLASTLOGIN,
    DTACTIVATION,
    DTDEACTIVATION,
    DTALTERATION,
    DTREVISION,
    INTERNALUSER,
    IDSUPERVISOR
) VALUES (
    'usuario.segundo',
    'usuario.segundo@hotmail.com',
    '8085858555455454',
    'Usuário Teste 2 via banco',
    '1',
    '06/11/2023 17:10:05',
    '',
    '06/11/2023 17:10:05',
    '',
    '',
    '',
    'Y',
    'usuario.primeiro'
);

INSERT INTO TB_SR_USERS__USERS (
    ID,
    EMAIL,
    LOGIN,
    NAME,
    ACTIVE,
    DTCREATION,
    DTLASTLOGIN,
    DTACTIVATION,
    DTDEACTIVATION,
    DTALTERATION,
    DTREVISION,
    INTERNALUSER,
    IDSUPERVISOR
) VALUES (
    'usuario.terceiro',
    'usuario.terceiro@hotmail.com',
    '8085858555455454',
    'Usuário Teste 2 via banco',
    '1',
    '06/11/2023 17:10:05',
    '',
    '06/11/2023 17:10:05',
    '',
    '',
    '',
    'Y',
    'usuario.primeiro'
);

COMMIT;

--UPDATE TB_SR_USERS__USERS SET IDSUPERVISOR = 0 WHERE IDUSUARIO = 1;

--DELETE FROM TB_SR_USERS__USERS WHERE IdUsuario = 1 AND ROWNUM = 1;

--SELECT * FROM TB_SR_USERS__USERS WHERE IdUsuario = 1;

--DROP TABLE TB_SR_USERS__USERS;



-------------------- COPIAR TABELA ------------------

CREATE TABLE TB_SR_USERS__USERS_TEMP AS
    SELECT
        *
    FROM
        TB_SR_USERS__USERS;

DROP TABLE TB_SR_USERS__USERS;

-- (CRIA TB_SR_USERS__USERS TABELA NO C#)

INSERT INTO TB_SR_USERS__USERS (
    ID,
    EMAIL,
    LOGIN,
    NAME,
    ACTIVE,
    DTCREATION,
    DTLASTLOGIN,
    DTACTIVATION,
    DTDEACTIVATION,
    DTALTERATION,
    DTREVISION,
    INTERNALUSER,
    IDSUPERVISOR,
    IDCAREER
)
    SELECT
        *
    FROM
        TB_SR_USERS__USERS_TEMP;

SELECT
    *
FROM
    TB_SR_USERS__USERS;

SELECT
    *
FROM
    TB_SR_USERS__USERS_TEMP;

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
    IDCAREER NUMBER(10) PRIMARY KEY,
    NAME NVARCHAR2(200),
    CAREERLEVEL NUMBER,
    ACTIVE NUMBER(1)
);

DROP TABLE TB_SR_USERS__CAREERS;

-- TB_SR_USERS__CAREERS
SELECT
    *
FROM
    TB_SR_USERS__CAREERS;

SELECT
    *
FROM
    TB_SR_USERS__CAREERS
WHERE
    (IDCAREER='1');

-----------------------


SELECT
    *
FROM
    TB_SR_USERS__GROUPS;

DROP TABLE TB_SR_USERS__GROUPS;

------------------------------

-- TB_SR_USERS__ORIGENS

CREATE TABLE TB_SR_USERS__ORIGENS(
    IDORIGEM NUMBER,
    IDENTIFICACAO VARCHAR2(100)(20),
    NOME VARCHAR2(100)(500),
    ATIVO NUMBER(1)
);

INSERT INTO TB_SR_USERS__ORIGENS (
    IDORIGEM,
    IDENTIFICACAO,
    NOME,
    ATIVO
) VALUES (
    '0',
    '00000',
    'Origem Teste via banco',
    '1'
);

SELECT
    *
FROM
    TB_SR_USERS__ORIGENS;

SELECT
    *
FROM
    TB_SR_USERS__USERS
WHERE
    (IDORIGEM=0);

---------------------------------------------------------

SELECT
    *
FROM
    TB_USER;

SELECT
    *
FROM
    TB_CAREERS;

CREATE TABLE TB_USER_IDGROUPS (
    ID_TB_USER INT,
    ID_IDGROUPS INT,
    PRIMARY KEY (ID_TB_USER, ID_IDGROUPS),
    FOREIGN KEY (ID_TB_USER) REFERENCES (ID_TB_USER),
    FOREIGN KEY (ID_IDGROUPS) REFERENCES (ID_IDGROUPS)
);

DROP TABLE TB_USER_IDGROUPS;


CREATE TABLE UsuarioGrupo (
    UsuarioId INT,
    GrupoId INT,
    CONSTRAINT FK_UsuarioGrupo_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId),
    CONSTRAINT FK_UsuarioGrupo_Grupo FOREIGN KEY (GrupoId) REFERENCES Grupo(GrupoId),
    CONSTRAINT PK_UsuarioGrupo PRIMARY KEY (UsuarioId, GrupoId)
);



CREATE TABLE Test(
    ID INT PRIMARY KEY,
    NAME VARCHAR(50),
    SALT BLOB
);


INSERT INTO Test (
    ID,
    NAME,
    SALT
) VALUES (
    2,
    'Teste 1',
    hextoraw('0x01')
);


ALTER TABLE Usuario_Grupo
ADD CONSTRAINT FK_Usuario_Grupo_Grupo
FOREIGN KEY (ID_Grupo)
REFERENCES Grupo(ID);

SELECT * FROM SHEET8;

SELECT * FROM TEST_LINKSELECT;


SELECT 
    object_name AS nome_tabela,
    created AS data_criacao
FROM 
    all_objects
WHERE 
    object_type = 'TABLE'
    AND created >= SYSDATE - INTERVAL '10' MINUTE;


---------------------------------------------------------------------------------------------------

-- MxN


-- PK em uso
SELECT
    uc.constraint_name AS nome_constraint,
    uc.table_name AS tabela_referenciante,
    ucc.column_name AS coluna_referenciante
FROM
    all_cons_columns ucc
JOIN
    all_constraints uc ON ucc.constraint_name = uc.constraint_name
WHERE
    uc.constraint_type = 'R' -- Apenas chaves estrangeiras
    AND uc.r_owner = (SELECT owner FROM all_tables WHERE table_name = 'TB_USER') -- Proprietário da tabela TB_USER
    AND uc.r_constraint_name = (SELECT constraint_name FROM all_constraints WHERE constraint_type = 'P' AND table_name = 'TB_USER'); -- Nome da constraint da chave primária da tabela TB_USER








---------------------
-- Create table

CREATE TABLE TB_USERToGroup 
(
ID_Id1 INT, 
ID_Id2 INT, 
PRIMARY KEY (ID_Id1, ID_Id2), 
FOREIGN KEY (ID_Id1) REFERENCES TB_USER(Id),
FOREIGN KEY (ID_Id2) REFERENCES TB_GROUP_USERS(Id)
);

DROP TABLE TB_USERToGroup;
-- DELETE FROM TB_GROUP_USERS;

select * from TB_USER;
select * from TB_GROUP_USERS;
-- select * from TB_USERToGroup;
select * from TB_USERtoTB_GROUP_USERS;

COMMIT;
-- -- Nomes das tabelas MxN
-- TB_USERtoTB_GROUP_USERS -- Se menor que 30 caracteres
-- TB_USERtoGROUP          -- Se anterior maior que 30 caracteres

DELETE FROM TB_USER;
INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('1', 'Diego Piovezana', '@DiegoPiovezana', '05/05/2024 03:21:14', '1');
DELETE FROM TB_USERtoTB_GROUP_USERS;
INSERT INTO TB_USERtoTB_GROUP_USERS (ID_ID1,ID_ID2) VALUES ('1','1');
INSERT INTO TB_USERtoTB_GROUP_USERS (ID_ID1,ID_ID2) VALUES ('1','2');
DROP TABLE TB_USERtoTB_GROUP_USERS;
DROP TABLE TB_USER;
ALTER TABLE TB_USER DISABLE CONSTRAINT ID;

ROLLBACK;


---------------------
-- Get
SELECT * FROM TB_USERtoTB_GROUP_USERS;

SELECT * FROM TB_GROUP_USERSTOGROUPS;

SELECT * FROM TB_USER;

SELECT * FROM TB_GROUP_USERS;

SELECT * FROM TB_USERtoGROUP;

SELECT * FROM TB_USERtoGROUP WHERE (ID_TB_USER='1');

SELECT * FROM TB_CAREERS;



----------------------
-- UPDATE







-----------------------------------------------------

select * from TB_CAREERS;
select * from TB_USER;





--------------------------------------------------------------
-- Many to Many

-- ENTITY 1 - Group
-- Type: ................... "Group";
-- Table: .................. "TB_GROUP_USERS";
-- NameProp: ............... "Users".

-- ENTITY 2 - User
-- Type: ................... "User"; 
-- Table: .................. "TB_USER";
-- NameProp: ............... "Groups".


-- TB_GROUP_USERStoGROUPS
-- TB_GROUP_USERStoUSERS


SELECT * FROM TB_USERtoTB_GROUP_USERS;

SELECT * FROM TB_GROUP_USERSTOGROUPS;

SELECT * FROM TB_USER;

SELECT * FROM TB_GROUP_USERS;

SELECT * FROM TB_USERtoGROUP;


--------------------------------------
-- Improve inserts

DROP TABLE TB_USER;

SELECT * FROM TB_USER;

INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('1', 'Diego Piovezana', '@DiegoPiovezana', '10/08/2024 17:45:03', '1')
RETURNING Id;



SET SERVEROUTPUT ON;

DECLARE
    v_id TB_USER.Id%TYPE;
BEGIN
    INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('2', 'Diego Piovezana', '@DiegoPiovezana', '10/08/2024 17:45:03', '1')
    RETURNING Id INTO v_id;

    -- Output the value to an Oracle parameter
    :InsertedId := v_id;
    DBMS_OUTPUT.PUT_LINE('ID: ' || v_id);
END;


SET SERVEROUTPUT ON;
                        
DECLARE
    v_id TB_USER.Id%TYPE;
BEGIN
    INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('1', 'Diego Piovezana', '@DiegoPiovezana', '11/08/2024 16:17:20', '1')
    RETURNING Id INTO v_id;
    
    -- :InsertedId := v_id;
    OPEN :result FOR SELECT v_id FROM DUAL;
END;




COMMIT

ROLLBACK;