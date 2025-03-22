// <copyright file="HandlebarsHelperExtensions.cs" company="Visma IMS A/S">
// Copyright (c) Visma IMS A/S. All rights reserved.
// Unauthorized reproduction of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
// </copyright>

namespace Visma.Ims.EmailTemplateAPI.Handlebars;

using HandlebarsDotNet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using Visma.Ims.Common.Abstractions.Logging;

/// <summary>
/// Extension methods for Handlebars helpers and partials.
/// </summary>
public static class HandlebarsHelperExtensions
{
    private static readonly object LockObject = new object();
    private static bool helpersRegistered = false;
    private static Localization localization = new Localization();

    /// <summary>
    /// Registers all Handlebars helpers and partials
    /// </summary>
    /// <param name="hostEnvironment">The host environment.</param>
    /// <param name="logger">The logger.</param>
    public static void RegisterAllHelpersAndPartials(IHostEnvironment hostEnvironment, ILogFactory logger)
    {
        // Use double-checked locking to ensure thread safety
        if (!helpersRegistered)
        {
            lock (LockObject)
            {
                if (!helpersRegistered)
                {
                    RegisterCustomHelpers(logger);
                    RegisterPartials(hostEnvironment, logger);
                    helpersRegistered = true;

                    // Log success
                    logger.Log().Information("All Handlebars helpers and partials registered successfully");
                }
            }
        }
    }

    /// <summary>
    /// Compiles a Handlebars template with the given data
    /// </summary>
    /// <param name="templateText">The template text.</param>
    /// <param name="data">The data.</param>
    /// <returns>The compiled template.</returns>
    public static string CompileTemplate(string templateText, object data)
    {
        var template = Handlebars.Compile(templateText);
        return template(data);
    }

    /// <summary>
    /// Registers custom Handlebars helpers
    /// </summary>
    /// <param name="logger">The logger.</param>
    private static void RegisterCustomHelpers(ILogFactory logger)
    {
        try
        {
            // Register localization helper
            Handlebars.RegisterHelper("localize", (output, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    throw new ArgumentException("Localization requires at least two arguments: key and language.");
                }

                var key = arguments[0].ToString();
                var language = arguments[1].ToString();
                var args = new object[arguments.Length - 2];

                for (int i = 2; i < arguments.Length; i++)
                {
                    args[i - 2] = arguments[i];
                }

                string localizedMessage = localization.FormatMessage(key, language, args);
                output.WriteSafeString(localizedMessage);
            });

            // Register a helper for the current year
            Handlebars.RegisterHelper("currentYear", (writer, context, parameters) =>
            {
                writer.WriteSafeString(DateTime.Now.Year.ToString());
            });

            logger.Log().Information("Handlebars custom helpers registered successfully");
        }
        catch (Exception ex)
        {
            logger.Log().Error(ex, "Error registering Handlebars custom helpers");
            throw; // Rethrow to fail startup if helpers can't be registered
        }
    }

    /// <summary>
    /// Registers Handlebars partials from the Handlebars/Partials directory
    /// </summary>
    /// <param name="hostEnvironment">The host environment.</param>
    /// <param name="logger">The logger.</param>
    private static void RegisterPartials(IHostEnvironment hostEnvironment, ILogFactory logger)
    {
        try
        {
            string partialsDirectory = Path.Combine(hostEnvironment.ContentRootPath, "Handlebars", "Partials");
            logger.Log().Information("Looking for partials in: {PartialsDirectory}", partialsDirectory);

            if (Directory.Exists(partialsDirectory))
            {
                var files = Directory.GetFiles(partialsDirectory, "*.hbs");
                logger.Log().Information("Found {Count} partial files", files.Length);

                foreach (var file in files)
                {
                    string partialName = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                    string partialContent = File.ReadAllText(file);
                    Handlebars.RegisterTemplate(partialName, partialContent);
                    logger.Log().Information("Registered partial: {PartialName} from file {FileName}", partialName, Path.GetFileName(file));
                }
            }
            else
            {
                logger.Log().Warning("Partials directory not found: {Directory}", partialsDirectory);
            }
        }
        catch (Exception ex)
        {
            logger.Log().Error(ex, "Error registering Handlebars partials");
            throw; // Rethrow to fail startup if partials can't be registered
        }
    }
}
