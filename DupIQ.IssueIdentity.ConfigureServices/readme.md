Create the database in SQL Server first. Assign an account appropriate permissions to
create tables and stored procedures. That account is specified in the "AccountName"
value in the user secrets as per below.
 
 User secrets are stored in the following format:

```
{
  "SqlIOHelperConfig": {
    "ConnectionString": "",
    "ServerName": "<servername>",
    "DatabaseName": "<databasename>",
    "AccountName": "<sql server account>",
    "Password": "<password to sql account>"
}
```
