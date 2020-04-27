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
___
## Identity 3

In Identity 3 we're taking a look at secure user management & email confirmation with tokens

###### startup

Most noticably and importantly we have changed `services.AddIdentityCore` to `AddIdentity`, This means we dont have to use `AddAuthentication` as `AddIdentity` already has that built in. This require us to add multiple methods such as `AddDefaultTokenProviders` and `AddEntityFrameworkStores` specified with the context. We also add out own password validator named `DoesNotContainPasswordValidator` with a non generic user type.

Last we also need to add dependency injection.

###### HomeController

In home controller we have modified the login method to use `SignInManager`, however `SignInManager` removes alot of the useful options provided by `HttpContext.SignInManager`. We have also added lockout functionallity to prevent bruteforce attacks.

We have also added confirm email, forgot password, reset password and TwoFactorAuthentication methods.


###### DoesNoTContainPasswordValidator

does infact contain password validator. In the method `ValidateAsync` we check against use cases we do not approve of and thereby return appropriate `IdentityError`s 

###### NOTE

We create a new class `EmailConfirmationtoken` to give us the option to use different timespans on `TokenLifespan`. Example: It may take a user longer to confirm their email than if they were to use 2FA which is why we want different timespans. This is a part of *securing token services*

___

## Identity 4

Identity 4 Is more or less the same as Identity 3, however there are some minor changes

###### HomeController

In the homecontroller we changed `SignInManager` back to `HttpContext.SignInManager` to get more options, especially regarding claims.
We then use the method `Store2FA` to generate claims regarding the 2FA.

we use the `TwoFactorAsync` methods to handle the signin if it is enabled.
we then use `UserManager.GeneratetwoFactorTokenAsync` to generate a 2FA token which we will send by email, or in this case generate to a text file to validate against. We then validate the token in `TwoFactorAsync` using the `TwoFactorModel` and `UserManager.VerifyTwoFactorTokenAsync`


###### TwoFactorModel

We use the homecontroller to check if 2FA is enabled, it is not by default, but i set it manually in my database using queries.
We then use the model to transport the `Token` string.


___

**Final note:** *If this repo is subject to misinformation, please create an issue and/or a pull request*





