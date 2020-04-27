# Dot Net Core - Identity
#### Repository made for learnig purposes & code lookup
___
### Identity 1
In Identity 1 we'll take a look at how to register new users and login using the helper class UserManager from Microsoft.AspNetCore.Identity Namespace
###### Register
in the register method in the homecontroller we're using the `UserManager` to check if the user exists, if it does not we'll create a new user. We create a new user by using the `MyUser` class defined under the folder models and creating a new object.

Then we use the `UserManager` to create a new user. `CreateAsync` will create a user and hash the pasword parsed as the second parameter.

###### Login
In login there isnt alot of things going on, we're checking model states and doing some error handling. however within `ModelState.IsValid` we're creating a new `ClaimsIdentity` and appending `claims` to it. we then use the claims to sign in the user with the specified `ClaimsPrincipal`. I will also mention that we use the builtin `HttpContext` to login instead of `SignInManager` in this folder.

Claims took me a bit of time to get, so i created a repo named *DNC-Authenticatin* for reference for myself and others that might need it
___

### Identity 2

In Identity 2 we'll be using `EntityFrameworkCore` and its builtin tables for Identity. You can see the dependencies in `.csproj`

###### startup

in startup under condigureservices we add `AddDbContect` and reference the class made under `Data` folder.
###### MyUser

this time we use the MyUser class to inherit and thereby extend IdentityUser. By doing this we create additional rows to the table.

###### ApplicationDbContext

in this class we ovveride `OnModelCreating` and define the building blocks of the new properties added to the `MyUser` class.
We also define that the entity `Organization` will be in its own table.

###### Home Controller

No change from Identity 1

## Identity 3
