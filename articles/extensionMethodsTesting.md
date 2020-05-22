## Introduction

Probably every single one of us writes at least one extension method. They are extremely useful as they enable us to add functionality to existing types without creating derived types, recompiling existing types, and even modifying them. Even though they are static methods they can be called in the same way as if they were instance methods. Typical scenarios when we would like to use them are:

- adding functionality to collections
- adding functionalities to Domain Entities or Data Transfer Objects
- adding functionalities to predefined system types

When we read guidelines of extension methods we usually see much information:

- how to use them
- in which namespace they should be put
- how to be prepared for changing contracts of an extended type

However one aspect of extension methods is often omitted - testability. One thing is the testability extension method on the unit level. The second thing is the testability of the code which is using an extension method. In the following paragraphs I will explain to you what impact can extension method has on the testability of your code on a unit level.
