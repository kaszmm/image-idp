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