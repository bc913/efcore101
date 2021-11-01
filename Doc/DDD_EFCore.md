# DDD and EF Core
## Domain classes 
- Use public getters and private setters for the properties. C# creates the corresponding backing fields implicitly.
- EF Core, by default, binds to properties backing fields. Uses reflection to find those backing fields and assigns values to them.
- C# creates parameterless public constructor by default if no other ctor is defined. Always define one with the required arguments.
    1. MAke sure the function argument names are the `lowerCamelCase` versions of the class property names.
