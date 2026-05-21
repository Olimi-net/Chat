Create table [Users]([Id] int PRIMARY KEY,[Login] nvarchar(50), [Password] nvarchar(50));

Create table [ChatLog]([Id] int IDENTITY(1,1) PRIMARY KEY, [Text] nvarchar(250), [UserId] int, [Date] datetime, CONSTRAINT fk_messages_user_id FOREIGN KEY (UserId) REFERENCES Users (Id));

Create table [ErrorLog]([Id] int IDENTITY(1,1) PRIMARY KEY, [Text] nvarchar(250), [Code] int, [Date] datetime);

Insert into [Users] ([Id], [Login], [Password]) Values(1,"Root", "");