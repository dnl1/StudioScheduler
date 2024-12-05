-- Drop Rooms Table
DROP TABLE IF EXISTS "Rooms" CASCADE;

-- Drop Studios Table
DROP TABLE IF EXISTS "Studios" CASCADE;
drop TABLE IF EXISTS "Scheduler" cascade;

select * from "Studios"
-- Create Studios Table
CREATE TABLE "Studios" (
    "StudioID" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Address" TEXT NOT NULL,
    "ContactNumber" VARCHAR(20) NOT NULL
);

-- Create Rooms Table
CREATE TABLE "Rooms" (
    "RoomID" SERIAL PRIMARY KEY,
    "StudioID" INT REFERENCES "Studios"("StudioID") ON DELETE CASCADE,
    "RoomName" VARCHAR(50) NOT NULL
);

-- Create Scheduler Table
CREATE TABLE "Scheduler" (
    "ScheduleID" SERIAL PRIMARY KEY,
    "RoomID" INT REFERENCES "Rooms"("RoomID") ON DELETE cascade not NULL,
    "BandName" VARCHAR(100) NOT NULL,
    "ContactName" VARCHAR(50) NOT NULL,
    "MobileNumber" VARCHAR(15) NOT NULL,
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL
);

INSERT INTO "Studios" ("Name", "Address", "ContactNumber")
VALUES 
    ('Warzone Canal 1', 'Av. Senador Pinheiro Machado, 369A - Vila Belmiro, Santos - SP, 11075-001', '+55 13 97407-6478'),
    ('Warzone Canal 3', 'R. Braz Cubas, 372A - Vila Matias, Santos - SP, 11075-200', '+55 13 97407-6478');

insert into "Rooms" ("StudioID", "RoomName") 
values (1, 'Sala 1 - Warzone Canal 1'), (2, 'Sala 1 - Warzone Canal 3')

INSERT INTO "Scheduler" ("RoomID", "BandName", "ContactName", "MobileNumber", "StartDate", "EndDate")
VALUES (1, 'Undercolin', 'Danilo Gomes', '11946010528', '2024-12-15 10:00:00', '2024-12-15 14:00:00');
    