# Results!

[![.NET Build](https://github.com/tombatron/Tombatron.Results/actions/workflows/build.yml/badge.svg)](https://github.com/tombatron/Tombatron.Results/actions/workflows/build.yml)

## Overview

What you have here is a basic implementation of the ["Results pattern"](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern) which by itself isn't. No, this one has a Roslyn analyzer that was inspired by the Rust language, who's `Option` type has requirements that if not met will throw compile time errors. 

Is that useful in a .NET-based application? I have no idea, but I thought it would be fun to build so here we are. 

## Installation

Installation is a snap. I recommend using the `Tombatron.Results.All` meta-package since it bundles everything together. 

The following are the related published packages. 


|Package Name                                                                                | Description                                                                                        |
|--------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------|
| [Tombatron.Results](https://www.nuget.org/packages/Tombatron.Results/)                     | This package contains the results pattern implementation.                                          |
| [Tombatron.Results.Analyzers](https://www.nuget.org/packages/Tombatron.Results.Analyzers/) | This package contains the Roslyn analyzer for the **Tombatron.Results** package.                   |
| [Tombatron.Results.All](https://www.nuget.org/packages/Tombatron.Results.All/)             | This is a meta-package that contains both **Tombatron.Results** and **Tombatron.Results.Analyzers. |

## Usage

### Tombatron.Results

Usage of the simplistic result pattern implementation is... simple!

Let's say that you have a method that returns a `string`, the method signature might look something like:

```csharp
public string MyExampleMethod()
```

Instead of directly returning a `string` type directly, we'll wrap our result with the generic `Result<T>` class, so now our method signature will look like:

```csharp
public Result<string> MyExampleMethod()
```

Next, we need to create an instance of `Result<T>` or in this case `Result<string>`. `Result<T>` is an abstract class, so we can't directly create an instance of it, so instead we'll leverage one of the two static factory methods that exist on the `Result<T>` type to create our result. 

The first method is `Result<T>.Ok(T Value)`. This method will return a concrete instance of the `Ok<T>` class (which inherits from the `Result<T>` class). You simply pass in the value you want to wrap as the sole parameter and you're good to go. 

The next method is `Result<T>.Error(string message)`. You'll use this method to communicate issues that occurred within a method. This static factory method will return a concrete instance of `Error<T>`.



The following examples demonstrate different ways of handling a method that reads from a file and returns the content.

```csharp
// No special handling, just throw an exception!
public string ExampleMethod()
{
    return File.ReadAllText(@"/home/doesnt_exist.txt");
}

public void Main()
{
    try
    {
        var fileContents = ExampleMethod();

        // Do something with `fileContents`.
    }
    catch (FileNotFoundException)
    {
        // Produce a message to the user stating that the file didn't exist. 
    }
}
```

The above isn't really ideal since we're effectively using exception handling for flow control. You can do your own deep dive on why you should avoid that. 

```csharp
// Avoid the usage of exceptions and return `null` if the file doesn't exist. 
public string? ExampleMethod()
{
    var fileName = @"/home/doesnt_exist.txt";

    if (File.Exists(fileName))
    {
        return File.ReadAllText(fileName);
    } 
    else
    {
        return null;
    }
}

public void Main()
{
    var fileContents = ExampleMethod();

    if (fileContents is null)
    {
        // Do something with `fileContents`.
    }
    else
    {
        // Produce a message to the user stating that the file didn't exist.
    }
}
```

This is a little better since we're not using exceptions for flow control, but now we're attributing special meaning to a `null` value which in the long run is likely to cause maintainability issues. But, .NET is getting better at handling nullability so if you stay disciplined this can work for you. 

```csharp
// Use the "Results pattern" to return a value or indicate failure. 
public Result<string> ExampleMethod()
{
    var fileName = @"/home/doesnt_exist.txt";

    if (File.Exists(fileName))
    {
        return Result<string>.Error("The file didn't exist.");
    }
    else 
    {
        return Result<string>.Ok(File.ReadAllText(fileName));
    }
}

public void Main()
{
    var fileContents = ExampleMethod();

    if (fileContents is Ok<string> ok)
    {
        // Do something with `fileContents`.
    }

    if (fileContents is Error<string> error)
    {
        // Produce a message to the user stating that the file didn't exist. 
    }
}
```

This is what we're going with since it doesn't use exceptions for flow control, or attribute special meaning to a `null` value.

### Tombatron.Results.Analyzer




## Building

Building this project should be no more difficult than cloning this repository and issuing the `dotnet build` command.

Something to be aware of is that the `Tombatron.Results.Analyzers.Samples` project is excluded from building with the rest of the solution. This is because that project contains demonstrations of the Roslyn analyzer causing a compiler error. When building that project seperately, you'll see a demonstration of what happens when 