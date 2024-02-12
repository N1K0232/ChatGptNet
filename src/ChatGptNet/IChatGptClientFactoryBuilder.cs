﻿using Microsoft.Extensions.DependencyInjection;

namespace ChatGptNet;

/// <summary>
/// Represents a builder for configuring ChatGPT client factory.
/// </summary>
public interface IChatGptClientFactoryBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where ChatGPT services are registered.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the <see cref="IHttpClientBuilder"/> used to configure the <see cref="HttpClient"/> used by ChatGPT.
    /// </summary>
    IHttpClientBuilder HttpClientBuilder { get; }
}
