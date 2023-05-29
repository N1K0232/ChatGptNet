﻿using ChatGptNet.Models;
using ChatGptNet.ServiceConfigurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatGptNet;

/// <summary>
/// Provides extension methods for adding ChatGPT support in NET applications.
/// </summary>
public static class ChatGptServiceCollectionExtensions
{
    /// <summary>
    /// Registers a <see cref="ChatGptClient"/> instance with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="builder">The <see cref="ChatGptOptionsBuilder"/> to configure options.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>This method automatically adds a <see cref="MemoryCache"/> that is used to save chat messages for completion.</remarks>
    /// <seealso cref="ChatGptOptionsBuilder"/>
    /// <seealso cref="MemoryCacheServiceCollectionExtensions.AddMemoryCache(IServiceCollection)"/>
    public static IServiceCollection AddChatGpt(this IServiceCollection services, Action<ChatGptOptionsBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(builder);

        var options = new ChatGptOptionsBuilder();
        builder.Invoke(options);

        ArgumentNullException.ThrowIfNull(options.ServiceConfiguration);

        SetMissingDefaults(options);
        services.AddSingleton(options.Build());

        AddChatGptCore(services);

        return services;
    }

    /// <summary>
    /// Registers a <see cref="ChatGptClient"/> instance reading configuration from the specified <see cref="IConfiguration"/> source.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
    /// <param name="sectionName">The name of the configuration section that holds ChatGPT settings (default: ChatGPT).</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>This method automatically adds a <see cref="MemoryCache"/> that is used to save chat messages for completion.</remarks>
    /// <seealso cref="ChatGptOptions"/>
    /// <seealso cref="IConfiguration"/>
    /// <seealso cref="MemoryCacheServiceCollectionExtensions.AddMemoryCache(IServiceCollection)"/>
    public static IServiceCollection AddChatGpt(this IServiceCollection services, IConfiguration configuration, string sectionName = "ChatGPT")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new ChatGptOptionsBuilder();
        var configurationSection = configuration.GetSection(sectionName);
        configurationSection.Bind(options);

        // Creates the service configuration (OpenAI or Azure) according to the configuration settings.
        options.ServiceConfiguration = ChatGptServiceConfiguration.Create(configurationSection);

        SetMissingDefaults(options);
        services.AddSingleton(options.Build());

        AddChatGptCore(services);

        return services;
    }

    /// <summary>
    /// Registers a <see cref="ChatGptClient"/> instance using dynamic options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="builder">The <see cref="ChatGptOptionsBuilder"/> to configure options.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>Use this this method if it is necessary to dynamically set options (for example, using other services via dependency injection).
    /// This method automatically adds a <see cref="MemoryCache"/> that is used to save chat messages for completion.
    /// </remarks>
    /// <seealso cref="ChatGptOptions"/>
    /// <seealso cref="IServiceProvider"/>
    /// <seealso cref="MemoryCacheServiceCollectionExtensions.AddMemoryCache(IServiceCollection)"/>
    public static IServiceCollection AddChatGpt(this IServiceCollection services, Action<IServiceProvider, ChatGptOptionsBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(builder);

        services.AddScoped(provider =>
        {
            var options = new ChatGptOptionsBuilder();
            builder.Invoke(provider, options);

            ArgumentNullException.ThrowIfNull(options.ServiceConfiguration);

            SetMissingDefaults(options);
            return options.Build();
        });

        AddChatGptCore(services);

        return services;
    }

    private static void AddChatGptCore(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient<IChatGptClient, ChatGptClient>();
    }

    private static void SetMissingDefaults(ChatGptOptionsBuilder options)
    {
        // If the provider is OpenAI and no default model has been specified, uses gpt-3.5-turbo by default.
        if (options.ServiceConfiguration is OpenAIChatGptServiceConfiguration && string.IsNullOrWhiteSpace(options.DefaultModel))
        {
            options.DefaultModel = OpenAIChatGptModels.Gpt35Turbo;
        }
    }
}
