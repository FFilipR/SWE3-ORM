select * from Persons;
select * from JuniorDevelopers;
select * from Mentors;
select * from skills;
select * from departments;
select * from jDevs_skills;

create table Persons (
	ID varchar(50) primary key,
	FirstName varchar(50),
	LastName varchar(50),
	Sex int,
	BDate timestamptz
)
create table Mentors (
	KPerson varchar(50) references Persons(ID) primary key,
	HDate timestamptz,
	Salary int
)
create table Skills (
	ID varchar(50) primary key,
	Name varchar(50),
	KMentor varchar(50),
	foreign key (KMentor) references Mentors (KPerson)
)

create table Departments (
	ID varchar(50) primary key,
	Name varchar(50),
	KMentor varchar(50),
	foreign key (KMentor) references Mentors (KPerson)
)

create table JuniorDevelopers (
	KPerson varchar(50) references Persons(ID) primary key,
	KDepartment varchar(50) references Departments(ID) foreign key,
	HDate timestamptz,
	Salary int
)

create table jDevs_skills(
	KjDev varchar(50),
	KSkill varchar(50),
	foreign key (KjDev) references JuniorDevelopers (KPerson),
	foreign key (KSkill) references Skills (ID)
)

SELECT Salary, HDate, ID, FirstName, LastName, BDate, Sex FROM Mentors INNER JOIN Persons ON ID = KPerson WHERE ID = 'm1'

