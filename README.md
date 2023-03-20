# ChatGPT for .NET

[![Lint Code Base](https://github.com/marcominerva/ChatGptNet/actions/workflows/linter.yml/badge.svg)](https://github.com/marcominerva/ChatGptNet/actions/workflows/linter.yml)
[![CodeQL](https://github.com/marcominerva/ChatGptNet/actions/workflows/codeql.yml/badge.svg)](https://github.com/marcominerva/ChatGptNet/actions/workflows/codeql.yml)
[![NuGet](https://img.shields.io/nuget/v/ChatGptNet.svg?style=flat-square)](https://www.nuget.org/packages/ChatGptNet)
[![Nuget](https://img.shields.io/nuget/dt/ChatGptNet)](https://www.nuget.org/packages/ChatGptNet)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/marcominerva/ChatGptNet/blob/master/LICENSE)

A ChatGPT integration library for .NET

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/ChatGptNet). Just search for *ChatGptNet* in the **Package Manager GUI** or run the following command in the **.NET CLI**:

    dotnet add package ChatGptNet

**Configuration**

Register ChatGPT service at application startup:

    builder.Services.AddChatGpt(options =>
    {
        options.ApiKey = "";
        options.Organization = null;    // Optional
        options.DefaultModel = ChatGptModels.Gpt35Turbo;  // Default: ChatGptModels.Gpt35Turbo
        options.MessageLimit = 16;  // Default: 10
        options.MessageExpiration = TimeSpan.FromMinutes(5);    // Default: 1 hour
    });

The API Key can be obtained in the [User settings](https://platform.openai.com/account/api-keys) page of your OpenAI account. For users who belong to multiple organizations, you can also specify which organization is used. Usage from these API requests will count against the specified organization's subscription quota.

With the *DefaultModel* property, you can specify the default model that will be used for chat completion, unless you pass an explicit value in the **AskAsync** method.

> **Note**
The *ChatGptModels.Gpt4* model is currently in a limited beta and only accessible to those who have been granted access. You can find more information in the [models documentation page](https://platform.openai.com/docs/models/gpt-4) of the [OpenAI site](https://openai.com/).

ChatGPT is aimed to support conversational scenarios: user can talk to ChatGPT without specifying the full context for every interaction. However, conversation history isn't managed by OpenAI, so it's up to us to retain the current state. **ChatGptNet** handles this requirement using a [MemoryCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.memorycache) that stores messages for each conversation. The behavior can be set using the following properties:

* *MessageLimit*: specifies how many messages for each conversation must be saved. When this limit is reached, oldest messages are automatically removed.
* *MessageExpiration*: specifies the time interval used to maintain messages in cache, regardless their count.

The **AddChatGpt** method has an overload that accepts an [IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider) as argument. It can be used, for example, if we're in a Web API and we need to support scenarios in which every user has a different API key that can be retrieved accessing a database via Dependency Injection.

**Usage**

The library can be used in any .NET application built with .NET 6.0 or later. For example, we can create a Minimal API in this way:

    app.MapPost("/api/ask", async (Request request, IChatGptClient chatGptClient) =>
    {
        var response = await chatGptClient.AskAsync(request.ConversationId, request.Message);
        return TypedResults.Ok(response);
    })
    .WithOpenApi();

    // ...

    public record class Request(Guid ConversationId, string Message);

If we just want to retrieve the response message, we can call the **GetMessage** method:

    var message = response.GetMessage();

**Handling a conversation**

The **AskAsync** method has an overload (the one shown in the example above) that requires a *conversationId* parameter. If we pass an empty value, a random one is generated and returned.
We can pass this value in subsequent invocations of **AskAsync** so that the library automatically retrieves previous messages of the current conversation (according to *MessageLimit* and *MessageExpiration* settings) and send them to ChatGPT.

**Changing the assistant's behavior**

ChatGPT supports messages with the *system* role to influence how the assistant should behave. For example, we can tell to ChatGPT something like that:

- You are an helpful assistant
- Answer like Shakespeare
- Give me only wrong answers
- Answer in rhyme

**ChatGptNet** provides this feature using the **SetupAsync** method:

    var conversationId await = chatGptClient.SetupAsync("Answer in rhyme");

If we use the same *conversationId* when calling  **AskAsync**, then the *system* message will be automatically sent along with every request, so that the assistant will know how to behave.

> **Note**
The *system* message does not count for messages limit number.

**Deleting a conversation**

Conversation history is automatically deleted when expiration time (specified by *MessageExpiration* property) is reached. However, if necessary it is possible to immediately clear the history:

    await chatGptClient.DeleteConversationAsync(conversationId);

**Contribute**

The project is constantly evolving. Contributions are welcome. Feel free to file issues and pull requests on the repo and we'll address them as we can. 