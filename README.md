# DupIQ

DupIQ provides a set of tools that make managing test information easier.
Especially duplicate bug and test failure detection.

Core Assemblies
===============
IssueIdentity
---------------
DupIQ.IssueIdentity is the core API that exposes methods which help
manage, analyze, and identify duplicate issues even when text strings are
not exactly the same. The API is extensible, allowing implmentation on
top of different systems. The current implementation runs on Word2Vec, Pinecone,
and Microsoft SQL Server.

IssueIdentity.API
---------------
DupIQ.IssueIdentity.API is a REST service which provides easy access to
the functionality of the  DupIQ.IssueIdentity namespace. Current version
has support for managing multiple tenants and projects (used to separate
sets of duplicate issues based on different group and project needs), as
well as a user management database to assign user access to different projects
and tenants.

Security and authorization features are not implemented yet.

Extensions
===============
IssueIdentity.Providers.Sql
---------------
Provides implementation of the issue, tenant, and user storage in
Microsoft SQL Server.

IssueIdentity.Providers.Word2Vec_Pincone
---------------
Provides implementation of the issue profile matching and lookup
interfaces on a combination of Word2Vec (embedding creation) and
and Pinecone (vector database search and storage).
