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
DELETE FROM TB_USER;

SELECT * FROM TB_USER ORDER BY ID;


COMMIT

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

                   
INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('1', 'Diego Piovezana', '@DiegoPiovezana', '11/08/2024 16:17:20', '1')
RETURNING Id INTO :Result;  


COMMIT

ROLLBACK


-- Set auto increment
DROP SEQUENCE SEQUENCE_USER;
CREATE SEQUENCE SEQUENCE_USER;
-- CREATE SEQUENCE SEQUENCE_USER
-- START WITH 1
-- INCREMENT BY 1;

-- ID respeitando possibilidade de ja vir o ID a partir do C#
-- CREATE OR REPLACE TRIGGER TRIGGER_USER
-- BEFORE INSERT ON TB_USER
-- FOR EACH ROW
-- BEGIN
--     IF :NEW.id IS NULL THEN
--         :NEW.id := SEQUENCE_USER.NEXTVAL;
--     END IF;
-- END;

-- ID do sequencial, independente do que vir do C#
CREATE OR REPLACE TRIGGER TRIGGER_USER
BEFORE INSERT ON TB_USER
FOR EACH ROW
BEGIN
    SELECT SEQUENCE_USER.NEXTVAL
    INTO :NEW.ID
    FROM DUAL;
END;

INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('1', 'Diego Piovezana', '@DiegoPiovezana', '10/08/2024 17:45:03', '1'); -- error
INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('0', 'Diego Piovezana', '@DiegoPiovezana', '10/08/2024 17:45:03', '1'); -- ok
INSERT INTO TB_USER (Name, GitHub, DtCreation, IdCareer) VALUES ('Diego Piovezana', '@DiegoPiovezana', '10/08/2024 17:45:03', '1'); -- ok
INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('200', 'Diego Piovezana', '@DiegoPiovezana', '10/08/2024 17:45:03', '1'); -- ok


INSERT INTO TB_USER (Id, Name, GitHub, DtCreation, IdCareer) VALUES ('0', 'Diego Piovezana One', '@DiegoPiovezana', '15/08/2024 01:18:58', '1')
RETURNING Id INTO :Result

SELECT * FROM TB_USER ORDER BY ID;
DELETE FROM TB_USER;

SELECT * FROM TB_CAREERS;

INSERT INTO TB_CAREERS (IdCareer, Name, CareerLevel, Active) VALUES ('1', 'Pleno', '2', '1') RETURNING Id INTO :Result;


------------- Many Insertions with MxN -------------

SELECT * FROM TB_USER ORDER BY ID;
SELECT * FROM TB_GROUP_USERS;
SELECT * FROM TB_GROUP_USERSTOGROUPS;


DROP SEQUENCE SEQUENCE_GROUP_USERS;
CREATE SEQUENCE SEQUENCE_GROUP_USERS START WITH 3;

CREATE OR REPLACE TRIGGER TRIGGER_GROUP_USERS
BEFORE INSERT ON TB_GROUP_USERS
FOR EACH ROW
BEGIN
    SELECT SEQUENCE_GROUP_USERS.NEXTVAL
    INTO :NEW.ID
    FROM DUAL;
END;




DELETE FROM TB_GROUP_USERSTOGROUPS;
DELETE FROM TB_GROUP_USERS;
DELETE FROM TB_USER;

COMMIT

SELECT * FROM TB_USER ORDER BY ID;
DROP TABLE TB_GROUP_USERSTOGROUPS;
DROP TABLE TB_USER;
DROP TABLE TB_GROUP_USERS; 


SELECT COUNT(*) FROM TESTTABLE;
SELECT * FROM TESTTABLE;


CREATE TABLE TB_TEST(
    c_1 VARCHAR2(1000),
    c_select VARCHAR2(1000)
)

DROP TABLE TB_TEST;

---------------------------------------------------------
-- load CSV


-- OracleException: ORA-01653: não é possível estender a tabela SYSTEM.TESTTABLE_BIGCSV em 1024 no tablespace SYSTEM

SELECT COUNT(*) FROM TESTTABLE;
SELECT * FROM TESTTABLE;


SELECT COUNT(*) FROM TESTTABLE_TXT;
SELECT * FROM TESTTABLE_TXT;


SELECT COUNT(*) FROM TestTableCsv_RangeRows;
SELECT * FROM TestTableCsv_RangeRows;



SELECT COUNT(*) FROM TESTTABLE_BIGCSV;

SELECT * FROM TESTTABLE_BIGCSV
WHERE ROWNUM <= 3;

SELECT * FROM TESTTABLE_BIGCSV;

DROP TABLE TESTTABLE_BIGCSV;


SELECT * FROM TestTableCsv_RangeRows;


------------

-- username/password@[//]host[:port][/service_name]
DISCONNECT
CONNECT system/oracle@localhost:1521/xe

-- DROP TABLE TestTable1M_Csv;
SELECT COUNT(*) FROM TestTable1M_Csv;
SELECT * FROM TestTable1M_Csv;

DISCONNECT
CONNECT system/oracle@localhost:49262/orclcdb

-- DROP TABLE TEST_LINKSELECT_CSV;
SELECT COUNT(*) FROM TEST_LINKSELECT_CSV;
SELECT * FROM TEST_LINKSELECT_CSV;




------------


SELECT * FROM TB_CAREERS;
SELECT * FROM TB_GROUP_USERS;
SELECT * FROM TB_USER;

DEFINE ID_TEST = '106';
DELETE FROM TB_USER WHERE TO_CHAR(ID) LIKE '%&ID_TEST%';
DELETE FROM TB_GROUP_USERS WHERE TO_CHAR(ID) LIKE '%&ID_TEST%';
DELETE FROM TB_CAREERS WHERE TO_CHAR(IDCAREER) LIKE '%&ID_TEST%';

COMMIT


-------------
-- Test includes

SELECT
    ID_TB_GROUP_USERS,
    ID_TB_USER
FROM
    TB_GROUP_USERSTOGROUPS
WHERE
    ID_TB_GROUP_USERS = NUMBER
    AND ID_TB_USER = NUMBER;




---------------------------------------------------------
-- Get paginated

SELECT * FROM v$version; -- Oracle Database 11g Express Edition Release 11.2.0.2.0 - 64bit Production

-- DROP TABLE TestTableCsv_RangeRows2;
-- DROP TABLE TestTableCsv_RangeRowsBig;

SELECT COUNT(*) FROM TestTableCsv_RangeRowsBig;
SELECT * FROM TestTableCsv_RangeRowsBig OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;
SELECT * FROM (SELECT * FROM (SELECT * FROM TestTableCsv_RangeRowsBig  )) WHERE ROWNUM <= 100 AND ROWNUM > 50;     -- 0
SELECT COUNT(*) FROM (SELECT * FROM (SELECT * FROM TestTableCsv_RangeRowsBig  )) WHERE ROWNUM <= 100;              -- 98
SELECT COUNT(*) FROM (SELECT * FROM (SELECT * FROM TestTableCsv_RangeRowsBig  )) WHERE ROWNUM > 50;                -- 0
SELECT COUNT(*) FROM TestTableCsv_RangeRowsBig WHERE ROWNUM > 50;                                                  -- 0
SELECT * FROM (SELECT * FROM TestTableCsv_RangeRowsBig) WHERE ROWNUM <= 100 AND ROWNUM > 50;
SELECT * FROM (SELECT * FROM TestTableCsv_RangeRowsBig) WHERE ROWNUM <= 20;


SELECT 
ROWNUM, 
T.* 
FROM TestTableCsv_RangeRowsBig T 
-- WHERE ROWNUM > 50
;  

SELECT * FROM 
(
SELECT ROWNUM AS N, T.* 
FROM TestTableCsv_RangeRowsBig T 
-- WHERE ROWNUM > 50
)
-- WHERE ROWNUM > 50 -- 0
WHERE N > 50   -- OK
;  

SELECT COUNT(*) FROM (SELECT * FROM (SELECT * FROM TestTableCsv_RangeRowsBig  )) WHERE ROWNUM > 50;

SELECT *
FROM (
    SELECT TestTableCsv_RangeRowsBig.*, ROW_NUMBER() OVER (ORDER BY your_column) AS rownum_alias
    FROM TestTableCsv_RangeRowsBig
)
WHERE rownum_alias >= 51;


SELECT * FROM (
    SELECT * FROM TestTableCsv_RangeRowsBig
    --ORDER BY {sortColumn} {sortDirection}
) 
WHERE ROWNUM <= 50 
AND ROWNUM > 50;


SELECT *
FROM (
    SELECT TestTableCsv_RangeRowsBig.*, ROWNUM AS rownum_alias
    FROM TestTableCsv_RangeRowsBig
    WHERE ROWNUM >= 51
)
WHERE rownum_alias >= 51;


SELECT * 
        FROM (
            SELECT inner_query.*, ROWNUM AS row_number
            FROM (SELECT * FROM TestTableCsv_RangeRowsBig) inner_query
            WHERE ROWNUM <= 100
        )
        WHERE row_number >= 50;



---------- Original query

                WITH UserGroupSummary AS (
                    SELECT 
                        u.Id AS UserId,
                        u.Name AS UserName,
                        g.Id AS GroupId,
                        g.Name AS GroupName,
                        c.Name AS CareerName,
                        COUNT(ug.ID_TB_USER) AS UserCountInGroup
                    FROM TB_USER u
                    JOIN TB_GROUP_USERStoGROUPS ug ON u.Id = ug.ID_TB_USER
                    JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
                    JOIN TB_CAREERS c ON u.IdCareer = c.IdCareer
                    GROUP BY u.Id, u.Name, g.Id, g.Name, c.Name
                )

                SELECT 
                    UserId,
                    UserName,
                    GroupId,
                    GroupName,
                    CareerName,
                    UserCountInGroup
                FROM UserGroupSummary

                UNION ALL

                SELECT 
                    NULL AS UserId, 
                    NULL AS UserName, 
                    g.Id AS GroupId, 
                    g.Name AS GroupName, 
                    NULL AS CareerName, 
                    COUNT(ug.ID_TB_USER) AS UserCountInGroup
                FROM TB_GROUP_USERStoGROUPS ug
                JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
                GROUP BY g.Id, g.Name

                ORDER BY GroupId, UserId NULLS LAST


-- Query com WITH e JOIN - 27 registros
WITH UserGroupSummary AS (
    SELECT 
        u.Id AS UserId,
        u.Name AS UserName,
        g.Id AS GroupId,
        g.Name AS GroupName
    FROM TB_USERS u
    JOIN TB_GROUP_USERStoGROUPS ug ON u.Id = ug.ID_TB_USERS
    JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
)
SELECT * FROM UserGroupSummary 
WHERE TO_CHAR(UserId) LIKE '206%'
ORDER BY GroupName, UserName

--  Query com UNION - 8 registros
SELECT Id, Name 
FROM TB_USERS
WHERE Id < 20605
AND TO_CHAR(Id) LIKE '206%'
UNION ALL
SELECT Id, Name 
FROM TB_GROUP_USERS
WHERE Id < 20605
AND TO_CHAR(Id) LIKE '206%'
ORDER BY Name

-- Query simples sem cláusulas adicionais - 20 registros
SELECT Id, Name FROM TB_USERS  WHERE TO_CHAR(Id) LIKE '206%'

-- Query com subquery - 6 registros
SELECT u.Id, u.Name
FROM TB_USERS u
WHERE u.Id IN (SELECT ug.ID_TB_USERS FROM TB_GROUP_USERStoGROUPS ug WHERE ug.ID_TB_GROUP_USERS = 20601)




---------- Paginated query

                SELECT /*+ FIRST_ROWS(10) */ * FROM ( SELECT inner_query.*, ROWNUM AS rnum FROM ( 
                WITH UserGroupSummary AS (
                    SELECT 
                        u.Id AS UserId,
                        u.Name AS UserName,
                        g.Id AS GroupId,
                        g.Name AS GroupName,
                        c.Name AS CareerName,
                        COUNT(ug.ID_TB_USER) AS UserCountInGroup
                    FROM TB_USER u
                    JOIN TB_GROUP_USERStoGROUPS ug ON u.Id = ug.ID_TB_USER
                    JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
                    JOIN TB_CAREERS c ON u.IdCareer = c.IdCareer
                    GROUP BY u.Id, u.Name, g.Id, g.Name, c.Name
                )

                SELECT 
                    UserId,
                    UserName,
                    GroupId,
                    GroupName,
                    CareerName,
                    UserCountInGroup
                FROM UserGroupSummary
                WHERE TO_CHAR(UserId) LIKE '205%'

                UNION ALL

                SELECT 
                    NULL AS UserId, 
                    NULL AS UserName, 
                    g.Id AS GroupId, 
                    g.Name AS GroupName, 
                    NULL AS CareerName, 
                    COUNT(ug.ID_TB_USER) AS UserCountInGroup
                FROM TB_GROUP_USERStoGROUPS ug
                JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
                WHERE TO_CHAR(g.Id) LIKE '205%'
                GROUP BY g.Id, g.Name

                -- ORDER BY GroupId, UserId NULLS LAST ORDER BY 1) inner_query WHERE ROWNUM <= 10 ) WHERE rnum > 0
                -- ORDER BY GroupId, UserId NULLS LAST) inner_query WHERE ROWNUM <= 10 ) WHERE rnum > 0
               ) inner_query WHERE ROWNUM <= 10 ) WHERE rnum > 0


---------- Count query



WITH UserGroupSummary AS (
    SELECT 
        u.Id AS UserId,
        u.Name AS UserName,
        g.Id AS GroupId,
        g.Name AS GroupName,
        c.Name AS CareerName,
        COUNT(ug.ID_TB_USER) AS UserCountInGroup
    FROM TB_USER u
    JOIN TB_GROUP_USERStoGROUPS ug ON u.Id = ug.ID_TB_USER
    JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
    JOIN TB_CAREERS c ON u.IdCareer = c.IdCareer
    GROUP BY u.Id, u.Name, g.Id, g.Name, c.Name
)
SELECT COUNT(1) FROM (
    SELECT 1
    FROM UserGroupSummary

    UNION ALL

    SELECT 1
    FROM TB_GROUP_USERStoGROUPS ug
    JOIN TB_GROUP_USERS g ON ug.ID_TB_GROUP_USERS = g.Id
    GROUP BY g.Id, g.Name
) CountQuery



------------
-- special entity

DROP TABLE TB_GROUP_USERSTOGROUPS;
DROP TABLE TB_GROUP_USERS;
DROP TABLE TB_USERS;
-- DROP TABLE TestTableCsv_UTF8;
DELETE FROM TB_TICKET;

CREATE TABLE TB_TICKET (IdLog NUMBER PRIMARY KEY, DateCreate TIMESTAMP, IdUser NUMBER, Obs NVARCHAR2(1000), Previous NVARCHAR2(1000), After NVARCHAR2(1000))
DROP TABLE TB_TICKET;

DROP SEQUENCE SEQUENCE_TICKET;

-- Criando a sequência para o ID
CREATE SEQUENCE SEQUENCE_TICKET
START WITH 1
INCREMENT BY 1;

SELECT SEQUENCE_TICKET.NEXTVAL FROM DUAL; 

-- Criando o trigger para definir o ID antes da inserção
CREATE OR REPLACE TRIGGER TRIGGER_TICKET
BEFORE INSERT ON TB_TICKET
FOR EACH ROW
BEGIN    
    -- IF :NEW.IdLog IS NULL THEN -- Verifica se o ID não foi fornecido, e atribui o próximo valor da sequência
        :NEW.IdLog := SEQUENCE_TICKET.NEXTVAL;
    -- END IF;
END;

CREATE OR REPLACE TRIGGER TRIGGER_TICKET
BEFORE INSERT ON TB_TICKET
FOR EACH ROW
BEGIN
:NEW.IdLog := SEQUENCE_TICKET.NEXTVAL;
END;


-- Selecionando todos os registros da tabela TB_TICKET para verificação
SELECT * FROM TB_TICKET;

SELECT * FROM TB_USERS;




--------------------------------------------------------------------------------------------
-- Read

SELECT * FROM TestTableCsvHeader2;
SELECT * FROM TestTableCsvHeader1;
SELECT * FROM TestTable1M_Csv;
SELECT * FROM TestTable50K_Csv;
SELECT * FROM TEST_LINKSELECT_CSV;
SELECT * FROM TestTable_Txt;
SELECT * FROM TestTable_BigCsv;
SELECT * FROM TestTableCsv_RangeRows;
SELECT * FROM TestTableCsv_RangeRowsBig;
SELECT * FROM TestTable;
SELECT * FROM TABLEX;
SELECT * FROM TB_ENTITY_TEST;
SELECT * FROM TB_CAREERS;
SELECT * FROM TB_GROUP_USERSTOGROUPS;
SELECT * FROM TB_GROUP_USERS;
SELECT * FROM TB_USER;
SELECT * FROM TestTableCsv_UTF8;
SELECT * FROM TB_TICKET;

-- Origin
DISCONNECT
CONNECT system/oracle@localhost:1521/xe
SELECT COUNT(*) FROM TestTable50K_Csv; -- 49999

-- Destiny
DISCONNECT
CONNECT system/oracle@localhost:49262/orclcdb
SELECT COUNT(*) FROM TEST_LINKSELECT_CSV; -- 49999


--------------------------------------------------------------------------------------------
-- Cleanning

DISCONNECT
CONNECT system/oracle@localhost:1521/xe

DISCONNECT
CONNECT system/oracle@localhost:49262/orclcdb

DROP TABLE TestTableCsvHeader2;
DROP TABLE TestTableCsvHeader1;
DROP TABLE TestTable1M_Csv;
DROP TABLE TestTable50K_Csv;
DROP TABLE TEST_LINKSELECT_CSV;
DROP TABLE TestTable_Txt;
DROP TABLE TestTable_BigCsv;
DROP TABLE TestTableCsv_RangeRows;
DROP TABLE TestTableCsv_RangeRowsBig;
DROP TABLE TestTable;
DROP TABLE TABLEX;
DROP TABLE TB_ENTITY_TEST;
DROP TABLE TB_CAREERS;
DROP TABLE TB_GROUP_USERSTOGROUPS;
DROP TABLE TB_GROUP_USERS;
DROP TABLE TB_USERS;
DROP TABLE TestTableCsv_UTF8;
DROP TABLE TB_TICKET;




--------------------------------------------------------------------------------------------

-- Consulta para exibir o nome das tabelas e a quantidade de registros estimada
SELECT 
    t.table_name AS "Table Name",
    t.tablespace_name AS "Tablespace",
    t.num_rows AS "Estimated Rows",
    t.blocks AS "Blocks",
    t.avg_row_len AS "Average Row Length (Bytes)"
FROM 
    all_tables t
ORDER BY 
    t.num_rows DESC NULLS LAST;

-- Atualiza count
EXEC DBMS_STATS.GATHER_SCHEMA_STATS(OWNNAME => 'NOME_DO_ESQUEMA');

SELECT * FROM SOURCE$;






--------------------------------------------------------------------------------------------

-- Check space
SELECT TABLESPACE_NAME, BYTES/1024/1024 AS FREE_MB
FROM DBA_FREE_SPACE
WHERE TABLESPACE_NAME = 'SYSTEM';

-- Check path dbf
SELECT FILE_NAME, TABLESPACE_NAME
FROM DBA_DATA_FILES
WHERE TABLESPACE_NAME = 'SYSTEM';


ALTER DATABASE DATAFILE '/u01/app/oracle/oradata/XE/system.dbf' RESIZE 10240M; -- Reduz 10 GB de espaco em disco C











--------------------------------------------------------------------------------------------


-- Verifica a saúde do banco de dados Oracle
WITH 
-- Seção 1: CPU usada por sessão
CPU_USAGE AS (
    SELECT 
        'CPU Usage' AS "Section",
        s.sid AS "Session ID",
        s.serial# AS "Serial Number",
        p.spid AS "Process ID",
        s.username AS "Username",
        'N/A' AS "Status",
        'N/A' AS "Metric",
        t.value / 100 AS "Value"
    FROM 
        v$session s
    JOIN 
        v$process p ON s.paddr = p.addr
    JOIN 
        v$sesstat t ON s.sid = t.sid
    JOIN 
        v$statname n ON t.statistic# = n.statistic#
    WHERE 
        n.name = 'CPU used by this session'
),

-- Seção 2: Uso de memória PGA por processo
MEMORY_USAGE AS (
    SELECT 
        'Memory Usage' AS "Section",
        NULL AS "Session ID",
        NULL AS "Serial Number",
        p.spid AS "Process ID",
        p.program AS "Username",
        'N/A' AS "Status",
        'PGA Used MB' AS "Metric",
        p.pga_used_mem / (1024 * 1024) AS "Value"
    FROM 
        v$process p
    UNION ALL
    SELECT 
        'Memory Usage',
        NULL,
        NULL,
        p.spid,
        p.program,
        'N/A',
        'PGA Allocated MB',
        p.pga_alloc_mem / (1024 * 1024)
    FROM 
        v$process p
    UNION ALL
    SELECT 
        'Memory Usage',
        NULL,
        NULL,
        p.spid,
        p.program,
        'N/A',
        'PGA Max MB',
        p.pga_max_mem / (1024 * 1024)
    FROM 
        v$process p
),

-- Seção 3: Sessões ativas
ACTIVE_SESSIONS AS (
    SELECT 
        'Active Sessions' AS "Section",
        s.sid AS "Session ID",
        s.serial# AS "Serial Number",
        NULL AS "Process ID",
        s.username AS "Username",
        s.status AS "Status",
        'N/A' AS "Metric",
        NULL AS "Value"
    FROM 
        v$session s
    WHERE 
        s.status = 'ACTIVE'
),

-- Seção 4: Eventos de espera
WAIT_EVENTS AS (
    SELECT 
        'Wait Events' AS "Section",
        NULL AS "Session ID",
        NULL AS "Serial Number",
        NULL AS "Process ID",
        'N/A' AS "Username",
        'N/A' AS "Status",
        event AS "Metric",
        time_waited / 100 AS "Value"
    FROM 
        v$system_event
    WHERE 
        event NOT IN ('SQL*Net message from client', 'SQL*Net message to client')
),

-- Seção 5: Leituras e Escritas no banco
IO_STATS AS (
    SELECT 
        'I/O Statistics' AS "Section",
        NULL AS "Session ID",
        NULL AS "Serial Number",
        NULL AS "Process ID",
        'N/A' AS "Username",
        'N/A' AS "Status",
        name AS "Metric",
        value AS "Value"
    FROM 
        v$sysstat
    WHERE 
        name IN ('physical reads', 'physical writes')
)

-- Query final: Combina todos os dados para exibir uma visão geral da saúde do banco de dados
SELECT * FROM CPU_USAGE
UNION ALL
SELECT * FROM MEMORY_USAGE
UNION ALL
SELECT * FROM ACTIVE_SESSIONS
UNION ALL
SELECT * FROM WAIT_EVENTS
UNION ALL
SELECT * FROM IO_STATS;





