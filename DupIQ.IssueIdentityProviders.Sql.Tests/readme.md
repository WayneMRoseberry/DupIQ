This test suite covers both the general sql provider
business logic and the helper methods that directly
connect to the sql database. An instance of the
dupiq.issueidentity.providers.sql databases needs to
be installed, initialized, and running for these tests
to run.

The server information is specified a user secrets file with
the following settings:
```
{
  "SqlIOHelperConfig": {
    "ServerName": "<your server>",
    "DatabaseName": "<your database>",
    "AccountName": "<account with write access to database>",
    "Password": "<password for the account>"
  }
}
```