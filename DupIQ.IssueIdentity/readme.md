DupIQ.IssueIdentity Namespace
=================
The DupIQ.IssueIdentity namespace helps track and analyze
issues by giving them a distinct identity and recognizing
when two different reports are for the same issue. Duplicate
and similar issue tracking helps to discover patterns of
common events we want to understand and respond to. It helps
reduce the number of things we have to keep track of by
recognizing when multiple occurrences are for the same problem
or issue.

IssueProfile
----------------
An IssueProfile identifies a distinct issue tracked in the
system. There may be multiple reports of the same issue,
tracked as IssueReport objects. Each IssueProfile object has
a unique ID, an example message usually taken from the first
report of the issue, and the time it was first seen.

IssueReport
----------------
An IssueReport is used to notify the system that we have seen
an issue. It includes a message describing what was seen, and
a time when it was reported. After being reported, each 
IssueReport object is assigned an issueId that determines which
IssueProfile it is most closely related to. If no previously reported
issue matches closely enough, a new IssueProfile is created.

RelatedIssueProfile
----------------
Sometimes we only want to search for existing issues that are
similar to something we have seen. DupIQ.IssueIdentity returns
a set of RelatedIssueProfile objects, which are the same as IssueProfile
objects with a Similarity score indicating how similar they are
to what we were searching on.

IssueRepository
==================
IssueRepository is the main class to interact with. It dispatches
requests to the implementations of the interfaces where the platform
specific work happens. It provides methods for reporting, searching for,
retrieving, and deleting issues and issue reports. Its constructor
takes implmentations of key interfaces used to provide access
to issues and issue reports on different platforms.

IIssueDbProvider
------------------
IIssueDbProvider implements interfaces that IssueRepository uses to store and manage IssueProfile and IssueReport
objects.

IssueIdentityManager
------------------
IssueIdentityManager provides business logic for determining whether an issue
is new or already known. IssueIdentity manager also provides storage services
necessary to establish if an identity is known or not. It takes in its constructor
a list of IIssueIdentityProviders which implement platform specific services
for determining unique issue identification.

<h3>IIssueIdentityProvider</h3>
Implements platform specific interfaces for establishing issue identity and
whether or not a given issue has been seen before. IssueIdentityManager goes
through its list of IIssueIdentityProviders in order, calling CanHandle on each
to determine whether that provider is able to provide a unique identity for
the reported issue. The first one to report True to CanHandle is the one that
assigns the identity for the reported issue. Some providers may need to store
per issue data necessary to track new and existing issues (e.g. an embeddings
search based provider may use a vector database to store prior embeddings for
known issues).

<h3>DupIQ.IssueIdentity.Providers Namespace</h3>
Platform and technology specific implementations of interfaces for storage
and retrieval of DupIQ.IssueIdentity objects reside within the DupIQ.IssueIdentity.Providers
namespace using the naming convention "DupIQ.IssueIdentity.Providers._technologyname_".
For example, storage and retrieval of data on Microsoft SQL Server is provider in
the DupIQ.IssueIdentity.Providers.Sql namespace assemblies.

ITenantManager
-----------------
IssueRepository uses an ITenantManager implementation to retrieve information
about Tenants and Projects necessary to access and manage the distinct
IssueProfiles and IssueReports owned by each Project.

Tenants and Projects
==================
Issues make sense within a given context. Different situation, different set of issues. The
DupIQ.IssueIdentity namespaces uses Tenants and Projects to separate different sets
of IssueReports and IssueProfiles so they may be tracked distinctly based on need. A Tenant
represents an organizational group of people that might work together on a variety of
problems or initiatives. A Project represents some initiative owned by a Tenant with a distinct
set of issues to track. Every Tenant has one or more Projects. IssuesProfiles and IssueReports
are stored, retrieved and managed based on the tenant the reside in. ITenantManager defines a set
of interfaces for creation, retrieval, management, and deletion of Tenants, Projects, and assigning
user roles to each. User authorization to use various methods and objects are stored and managed at
the Tenant and Project level.

_Current implementation only stores user authorization, it does not enforce it._
