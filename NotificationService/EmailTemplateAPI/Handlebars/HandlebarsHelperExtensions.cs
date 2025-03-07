using HandlebarsDotNet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace EmailTemplateAPI.Handlebars
{
    public static class HandlebarsHelperExtensions
    {
        private static bool _helpersRegistered = false;
        private static readonly object _lock = new object();
        private static Localization _localization = new Localization();

        /// <summary>
        /// Registers all Handlebars helpers and partials
        /// </summary>
        public static void RegisterAllHelpersAndPartials(IHostEnvironment hostEnvironment, ILogger logger)
        {
            // Use double-checked locking to ensure thread safety
            if (!_helpersRegistered)
            {
                lock (_lock)
                {
                    if (!_helpersRegistered)
                    {
                        RegisterCustomHelpers(logger);
                        RegisterPartials(hostEnvironment, logger);
                        _helpersRegistered = true;
                        
                        // Log success
                        logger.LogInformation("All Handlebars helpers and partials registered successfully");
                    }
                }
            }
        }

        /// <summary>
        /// Registers custom Handlebars helpers
        /// </summary>
        private static void RegisterCustomHelpers(ILogger logger)
        {
            try
            {
                // Register localization helper
                HandlebarsDotNet.Handlebars.RegisterHelper("localize", (output, context, arguments) => 
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

                    string localizedMessage = _localization.FormatMessage(key, language, args);
                    output.WriteSafeString(localizedMessage);
                });
                
                // Register a helper for the current year
                HandlebarsDotNet.Handlebars.RegisterHelper("currentYear", (writer, context, parameters) => {
                    writer.WriteSafeString(DateTime.Now.Year.ToString());
                });
                
                logger.LogInformation("Handlebars custom helpers registered successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error registering Handlebars custom helpers");
                throw; // Rethrow to fail startup if helpers can't be registered
            }
        }

        /// <summary>
        /// Registers Handlebars partials from the Handlebars/Partials directory
        /// </summary>
        private static void RegisterPartials(IHostEnvironment hostEnvironment, ILogger logger)
        {
            try
            {
                string partialsDirectory = Path.Combine(hostEnvironment.ContentRootPath, "Handlebars", "Partials");
                logger.LogInformation("Looking for partials in: {PartialsDirectory}", partialsDirectory);
                
                if (Directory.Exists(partialsDirectory))
                {
                    var files = Directory.GetFiles(partialsDirectory, "*.hbs");
                    logger.LogInformation("Found {Count} partial files", files.Length);
                    
                    foreach (var file in files)
                    {
                        string partialName = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                        string partialContent = File.ReadAllText(file);
                        HandlebarsDotNet.Handlebars.RegisterTemplate(partialName, partialContent);
                        logger.LogInformation("Registered partial: {PartialName} from file {FileName}", partialName, Path.GetFileName(file));
                    }
                }
                else
                {
                    logger.LogWarning("Partials directory not found: {Directory}", partialsDirectory);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error registering Handlebars partials");
                throw; // Rethrow to fail startup if partials can't be registered
            }
        }

        /// <summary>
        /// Compiles a Handlebars template with the given data
        /// </summary>
        public static string CompileTemplate(string templateText, object data)
        {
            var template = HandlebarsDotNet.Handlebars.Compile(templateText);
            return template(data);
        }
        
        /// <summary>
        /// Compiles both HTML and plain text templates for an email
        /// </summary>
        public static (string html, string text) CompileEmailTemplates(string htmlTemplatePath, string textTemplatePath, object data, ILogger logger)
        {
            try
            {
                // Compile HTML template
                string htmlTemplateText = File.ReadAllText(htmlTemplatePath);
                string htmlContent = CompileTemplate(htmlTemplateText, data);
                
                // Compile text template
                string textTemplateText = File.ReadAllText(textTemplatePath);
                string textContent = CompileTemplate(textTemplateText, data);
                
                return (htmlContent, textContent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error compiling email templates: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Lists all registered partials for debugging purposes
        /// </summary>
        public static void LogRegisteredPartials(ILogger logger)
        {
            try
            {
                // This is a workaround since Handlebars.NET doesn't expose a way to list registered partials
                logger.LogInformation("Handlebars partials have been registered. If you're seeing partial resolution errors, check case sensitivity.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error listing registered partials");
            }
        }
    }
} 