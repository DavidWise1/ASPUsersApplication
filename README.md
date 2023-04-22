# ASPUsersApplication

DB is located in DateBase/sqlUsers.bak 


db installation:

On SQL Server Management Studio

Right click Databases on left pane (Object Explorer)

Click Restore Database...

Choose Device, click ..., and add the noted .bak file

Click OK, then OK again

Update Web.config connecton string.

currently its default : connectionString="Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;"
