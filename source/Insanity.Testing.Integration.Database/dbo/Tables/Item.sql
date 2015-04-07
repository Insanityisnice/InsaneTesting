CREATE TABLE [dbo].[Item]
(
	[Id] INT NOT NULL PRIMARY KEY, 
	[OrderId] INT NOT NULL, 
	[Description] NVARCHAR(255) NOT NULL, 
	CONSTRAINT [FK_Item_Order] FOREIGN KEY ([OrderId]) REFERENCES [Order]([Id])
)
