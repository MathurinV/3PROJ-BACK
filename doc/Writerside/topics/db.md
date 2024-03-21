# db

The database service is created using postgres version 16.2. It is used to store users' related data and data protection keys 

<tabs>
<tab title="Database schema">
    Here is the Mermaid diagram for the database:
    <code-block lang="mermaid">
        classDiagram
        direction BT
        class DataProtectionKeys {
           text FriendlyName
           text Xml
           integer Id
        }
        class Expenses {
           uuid GroupId
           numeric Amount
           varchar(255) Description
           timestamp with time zone CreatedAt
           uuid CreatedById
           integer JustificationExtension
           uuid Id
        }
        class GroupMessages {
           uuid SenderId
           uuid GroupId
           text Content
           timestamp with time zone SentAt
           uuid Id
        }
        class Groups {
           varchar(50) Name
           varchar(50) Description
           varchar(50) Image
           uuid OwnerId
           uuid Id
        }
        class Invitations {
           timestamp with time zone InvitedAt
           uuid GroupId
           uuid UserId
        }
        class Messages {
           uuid SenderId
           uuid ReceiverId
           text Content
           timestamp with time zone SentAt
           uuid Id
        }
        class RoleClaims {
           uuid RoleId
           text ClaimType
           text ClaimValue
           integer Id
        }
        class Roles {
           varchar(256) Name
           varchar(256) NormalizedName
           text ConcurrencyStamp
           uuid Id
        }
        class UserClaims {
           uuid UserId
           text ClaimType
           text ClaimValue
           integer Id
        }
        class UserExpenses {
           numeric Amount
           timestamp with time zone PaidAt
           uuid ExpenseId
           uuid UserId
        }
        class UserGroups {
           timestamp with time zone JoinedAt
           uuid UserId
           uuid GroupId
        }
        class UserLogins {
           text ProviderDisplayName
           uuid UserId
           text LoginProvider
           text ProviderKey
        }
        class UserRoles {
           uuid UserId
           uuid RoleId
        }
        class UserTokens {
           text Value
           uuid UserId
           text LoginProvider
           text Name
        }
        class Users {
           numeric Balance
           integer AvatarExtension
           varchar(256) UserName
           varchar(256) NormalizedUserName
           varchar(256) Email
           varchar(256) NormalizedEmail
           boolean EmailConfirmed
           text PasswordHash
           text SecurityStamp
           text ConcurrencyStamp
           text PhoneNumber
           boolean PhoneNumberConfirmed
           boolean TwoFactorEnabled
           timestamp with time zone LockoutEnd
           boolean LockoutEnabled
           integer AccessFailedCount
           uuid Id
        }
        Expenses  -->  Groups : GroupId
        Expenses  -->  Users : CreatedById
        GroupMessages  -->  Groups : GroupId
        GroupMessages  -->  Users : SenderId
        Groups  -->  Users : OwnerId
        Invitations  -->  Groups : GroupId
        Invitations  -->  Users : UserId
        Messages  -->  Users : SenderId
        Messages  -->  Users : ReceiverId
        RoleClaims  -->  Roles : RoleId
        UserClaims  -->  Users : UserId
        UserExpenses  -->  Expenses : ExpenseId
        UserExpenses  -->  Users : UserId
        UserGroups  -->  Groups : GroupId
        UserGroups  -->  Users : UserId
        UserLogins  -->  Users : UserId
        UserRoles  -->  Roles : RoleId
        UserRoles  -->  Users : UserId
        UserTokens  -->  Users : UserId
    </code-block>
</tab>
<tab title="Configuration">
    <tabs>
        <tab title="DbContext">
            <list>
                <li>
                    The DbContext is configured using a code-first approach, with minor annotations.
                </li>
                <li>
                    <a href="https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-8.0">Data Protection</a> is added to the DbContext to persist its access keys. Data protection keys use a <emphasis>AES_256_GCM</emphasis> encryption algorithm and a  <emphasis>HMACSHA256</emphasis> validation algorithm.
                </li>
                <li>
                    The postgres service uses split queries: Better performance, but special attention needs to be made so no concurrent database calls are made.
                </li>
            </list>
        </tab>
        <tab title="Identity">
            <list>
                <li>
                    The Identity DbContext is configured to use Guid as the default Id type. This ensures every entity in the database has a unique Id.
                </li>
                <li>
                    The database uses the default entity framework stores, and token providers.
                </li>
                <li>
                    Only 2 roles are defined for users:
                    <tabs>
                        <tab title="User">
                            Have basic right in the api. He can fetch his own profile, view the groups he is in, upload an avatar, and change his account information.
                        </tab>
                        <tab title="Admin">
                            Have all the rights of the User role, plus additional authorizations, like being able to fetch the entire database, view sensible information...
                        </tab>
                    </tabs>
                </li>
            </list>
        </tab>
    </tabs>
</tab>
</tabs>