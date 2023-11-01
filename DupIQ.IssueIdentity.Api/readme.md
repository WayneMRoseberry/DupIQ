DupIQ.IssueIdentity.Api REST API
================================
This project builds a REST API for DUPIQ.IssueIdentity.Api on top of 
ASP.NET CORE. It uses DupIQ.IssueIdentity.Providers.Sql for issue, tenant, and user
storage, and DupIQ.IssueIdentity.Providers.Word2Vec_Pinecone for
issue identity assignment. It uses Swagger for API experimentation in development
mode.

TenantId and ProjectId Query Parameters
-------------------------------
Almost every API call will require a TenantId and ProjectId. This is because
all IssueProfile and IssueReport objects are assigned to a specific Project (and all
projects are owned by a Tenant). The TenantId and ProjectId are supplied as query parameters, for example:
```
GET http://<mydupiqserver>/IssueReports/Report?TenantId=<yourtenantid>&ProjectId=<yourprojectid>&message=message_text
```

This also means that before using any of the DupIQ.IssueIdentity APIs, there needs
to be a set of calls made to create at least one Tenant and at least one Project in
order to store any of the IssueReports and IssueProfiles.

No Security Yet
--------------------------------
_This current version does not implement security. It permits anonymous access
to the web service, and all users can call to any of the APIs._

REST API Methods
================================

Admin
--------------------------------
_This set of methods was implemented for testing purposes early on
and will eventually be deprecated as matching capabilities are added
to other methods._

IssueProfiles
--------------------------------
Provides a set of methods for creating, retrieving, searching for
IssueProfile objects.

IssueReports
--------------------------------
Provides a set of methods for reporting, retrieving, and deleting IssueReport objects.

Tenants
--------------------------------
Provides a set of methods for creating, retrieving, modifying, deleting Tenants, Projects and
User roles on each.

Users
--------------------------------
Provides a set of methods for creating, retrieving, modifying, deleting users.
_User management features only store information about users. The current implementation
does not use the User storage features for anything._
