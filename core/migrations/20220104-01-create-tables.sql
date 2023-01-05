CREATE TABLE IF NOT EXISTS Project (
    Id SERIAL PRIMARY KEY
    ,Name        TEXT NOT NULL
    ,Description TEXT NOT NULL
    ,CreatedAt TIMESTAMP DEFAULT current_timestamp
    ,UpdatedAt TIMESTAMP DEFAULT current_timestamp
    ,DeletedAt TIMESTAMP
);

CREATE TYPE TaskStatus AS ENUM ('To-do', 'In Progress', 'Done');

CREATE TABLE IF NOT EXISTS Task (
    Id SERIAL PRIMARY KEY
    ,Name TEXT NOT NULL
    ,Description TEXT NOT NULL
    ,Status TaskStatus NOT NULL
    ,ProjectId INTEGER NOT NULL
    ,CreatedAt TIMESTAMP DEFAULT current_timestamp
    ,UpdatedAt TIMESTAMP DEFAULT current_timestamp
    ,DeletedAt TIMESTAMP
    ,FOREIGN KEY (ProjectId) REFERENCES Project(Id)
);

CREATE TABLE IF NOT EXISTS WorkSpan (
    Id SERIAL PRIMARY KEY
    ,TaskId INTEGER
    ,Description TEXT NOT NULL
    ,StartDate TIMESTAMP
    ,EndDate TIMESTAMP
    ,CreatedAt TIMESTAMP DEFAULT current_timestamp
    ,UpdatedAt TIMESTAMP DEFAULT current_timestamp
    ,DeletedAt TIMESTAMP
    ,FOREIGN KEY (TaskId) REFERENCES Task(Id)
);