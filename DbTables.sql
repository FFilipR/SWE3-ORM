
select * from JuniorDevelopers;
select * from Mentors;
select * from skills;
select * from departments;
select * from jDevs_skills;



drop table jDevs_skills;
drop table JuniorDevelopers;
drop table Departments;
drop table Skills;
drop table Mentors;
drop table locking;
drop INDEX Uq_Locking;

CREATE TABLE Locking (LClass varchar(50) NOT NULL, LObject varchar(50) NOT NULL, LTime timestamptz NOT NULL, LOwner varchar(50))
CREATE UNIQUE INDEX UX_Locking ON Locking (LClass, LObject)


create table if not exists Mentors(
	ID varchar(50) primary key,
	FirstName varchar(50),
	LastName varchar(50),
	BDate timestamptz,
	Sex int,
	Salary int,
	HDate timestamptz
)

create table Skills (
	ID varchar(50) primary key,
	Name varchar(50),
	KMentor varchar(50),
	foreign key (KMentor) references Mentors (ID)
)

create table Departments (
	ID varchar(50) primary key,
	Name varchar(50),
	KMentor varchar(50),
	foreign key (KMentor) references Mentors (ID)
)

create table JuniorDevelopers (
	ID varchar(50) primary key,
	FirstName varchar(50),
	LastName varchar(50),
	BDate timestamptz,
	Sex int,
	Salary int,
	HDate timestamptz,
	KSkill varchar(50),
	KDepartment varchar(50),
	foreign key (KSkill) references Skills (ID),
	foreign key (KDepartment) references Departments (ID)
)

create table jDevs_skills(
	KjDev varchar(50),
	KSkill varchar(50),
	foreign key (KjDev) references JuniorDevelopers (ID),
	foreign key (KSkill) references Skills (ID)
)

SELECT Salary, HDate, ID, FirstName, LastName, BDate, Sex FROM Mentors WHERE ID = 1;

INSERT INTO Mentors(Salary, HDate, ID, FirstName, LastName, BDate, Sex) VALUES ('4500', '02-Jan-99 12:00:00 AM', 'm1', 'Larry', 'Bird', '10-Sep-69 12:00:00 AM', '0') ON CONFLICT (ID) DO UPDATE SET Salary = '4500', HDate = '02-Jan-99 12:00:00 AM', FirstName = 'Larry', LastName = 'Bird', BDate = '10-Sep-69 12:00:00 AM', Sex = '0'

DELETE FROM Mentors WHERE ID = 'm1';

SELECT ID, Name, KMentor FROM Departments WHERE KMentor = 'm1'



INSERT INTO JuniorDevelopers (Salary, HDate, KDepartment, ID, FirstName, LastName, BDate, Sex) VALUES ('2000', '28-Feb-22 12:00:00 AM', null , 'jd3', 'Filip', 'Filipovic', '6-July-98 12:00:00 AM', '0') ON CONFLICT (ID) DO UPDATE SET Salary = '2000', HDate = '28-Feb-22 12:00:00 AM', KDepartment = null, FirstName = 'Filip', LastName = 'Filipovic', BDate = '6-July-98 12:00:00 AM', Sex = '0'

SELECT Salary, HDate, KDepartment, ID, FirstName, LastName, BDate, Sex FROM JuniorDevelopers WHERE ID IN (SELECT KjDev FROM jDevs_skills WHERE KSkill = 's1')


show max_connections;
SELECT * FROM pg_stat_activity;
SELECT COUNT(*) from pg_stat_activity;
select min_val, max_val from pg_settings where name='max_connections';
alter system set max_connections = 200;




SELECT Salary, HDate, KDepartment, ID, FirstName, LastName, BDate, Sex FROM JuniorDevelopers WHERE (Salary > '2200')

Create Table 