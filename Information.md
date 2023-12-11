## Authorization happens using two sort of ways
- Role based
- Attribute based

### Role based access control (RBAC)
- In this we need to provide some role for each sort of scope, and combining and adding more into scope will result into lot of over head of creating new roles, for each specific scopes.
- Example:
```c#
    // Decorator
    [Authorize(Roles="someRole,someOtherRole")
        
```

### Attribute based access control (ABAC)
- In this we can set rules that we can call as policy, and we can use that policy to control the authorization, rather than creating different role for different requirements
- Example:
```c#
  // Decorator
  [Authorize(Policy="somePolicy")]
```

- ABAC is recommended over RBAC cause of its flexibility to allow multiple rules in single time


### Fun Fact:
- IDP provides two tokens when user gets authenticated, one is ID Token (used for authentication) and
  and another is Access Token (used for authorization).

### Validations for token:
- **Id token**:
  1. **Signature** of token should be valid( basically nobody tampered the token its validates)
  2. **Nonce** of token, nonce in token should be same as nonce sent by client when making authentication request
  3. **Issuer** claim should match, so that we know for sure that token provided by idp is same that we requested to.
     - This will happen behind the scene, during the validating the id token,
       the client will hit the discovery endpoint and get the "iss" from it and checks the iss should match with id token
  4. **Audience** claim should match
  5. **Expiration time**
  6. **at_hash**, basically client also checks the id token and its access token should be linked and not different

- **Access Token**
  - Validation of access token is beyond scope of oidc, but validation generally happens
    based on this criteria
  1. **Signature**
  2. **Issuer**
  3. **Expiration**

### Access token vs Reference Token
- Access token contains all the sensitive data regarding the user and its claims and scope,
  so when access token gets leaked its all information is also compromised
- **Access token** are self contained token (JWT) and it doesnt require to call the idp
 everytime it needs to validate the token, so if token is leaked and token revocation is hard as
 the api will not make idp call to validate the token
- **Reference token** are just a reference id which is used to get access token saved in Idp server,
 if reference token gets leaked it can easily be revoked, as api uses the reference token to make call to 
 idp and then gets access token.
- If **reference token** gets leaked , the sensitive info cannot be leaked cause reference token is
 just id which provides the location in which the token is stored in idp server.
- Also **reference token** reduces the overhead of encrypting ans storing the access token in client app.
- **Reference token** takes more time to validate the access token, as it needs to request to idp on each request call.

### Password Hashing
- we are using .net inbuilt password hasher `IPasswordHasher`,
 this hashes the password with salt and do key stretching(hashing the password multiple times) as well

### Integrating third party identity providers
- This allows users to login into our idp by using there already logged in account of facebook, google etc..
- In our Idp we have integrated facebook oidc flow in which our idp works as client
 for the facebook's idp.
- https://developers.facebook.com/apps/297798405963664/permissions/?use_case_enum=FB_LOGIN&show_verification_toast=1
 for setting up new apps in developers facebook page.

### Federation Identity
- It means a single identity is shared across multiple identity servers, and generally 
 this identities have a unique key that can be used to linked them across multiple identities.
- Read this https://www.okta.com/identity-101/what-is-federated-identity/

### User Provisioning
- It means user or Iam can provision to creating/modifying/deleting the user account.

### Some definitions
- `IIdentityServerInteractionService`: This service is used by IdentityServer to interact with its protocol layer.
    It allows you to get information about the current authorization request, 
    consent requests, and to deal with login and consent responses.
-  `IAuthenticationSchemeProvider`: This service in ASP.NET Core is used to retrieve the list of registered
    authentication schemes (like local login, Facebook, Google, etc.).
    It helps to dynamically load the list of available external providers configured in the system.
- `IIdentityProviderStore`: This is an IdentityServer service that allows for dynamically 
   enabling/disabling identity providers (external login providers like Facebook).