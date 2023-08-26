begin transaction;

create table Pallete(
	Id uniqueidentifier not null,
	Width int not null,
	Height int not null,
	Length int not null,
	Weigth int not null,
	ProductionDate datetime not null
);

create table Box(
	Id uniqueidentifier not null,
	Width int not null,
	Height int not null,
	Length int not null,
	Weigth int not null,
	ProductionDate datetime not null,
	PalleteId uniqueidentifier null
);

commit;