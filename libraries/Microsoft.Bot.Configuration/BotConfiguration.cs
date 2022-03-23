// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration.Encryption;
using Microsoft.Bot.Connector.Client.Models;

namespace Microsoft.Bot.Configuration
{
    /// <summary>
    /// BotConfiguration represents configuration information for a bot.
    /// </summary>
    /// <remarks>It is typically loaded from a .bot file on disk.
    /// This class implements methods for encrypting and manipulating the in memory representation of the configuration.</remarks>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public class BotConfiguration
    {
        private const string SECRETKEY = "secretKey";

        /// <summary>
        /// Gets or sets name of the bot.
        /// </summary>
        /// <value>The name of the bot.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the bot.
        /// </summary>
        /// <value>The description for the bot.</value>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets padlock - Used to validate that the secret is consistent for all encrypted fields.
        /// </summary>
        /// <value>The padlock.</value>
        [JsonPropertyName("padlock")]
        public string Padlock { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "2.0";

        /// <summary>
        /// Gets or sets connected services.
        /// </summary>
        /// <value>The list of connected services.</value>
        [JsonPropertyName("services")]
        [JsonConverter(typeof(BotServiceConverter))]
#pragma warning disable CA2227 // Collection properties should be read only (this class is obsolete, we won't fix it)
        public List<ConnectedService> Services { get; set; } = new List<ConnectedService>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets properties that are not otherwise defined.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData]
#pragma warning disable CA2227 // Collection properties should be read only (this class is obsolete, we won't fix it)
        public Dictionary<string, JsonElement> Properties { get; set; } = new Dictionary<string, JsonElement>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the location of the configuration.
        /// </summary>
        [JsonIgnore]
        private string Location { get; set; }

        /// <summary>
        /// Load the bot configuration by looking in a folder and loading the first .bot file in the folder.
        /// </summary>
        /// <param name="folder">Folder to look for bot files. </param>
        /// <param name="secret">Secret to use to encrypt keys. </param>
        /// <returns><see cref="Task"/> of <see cref="BotConfiguration"/>.</returns>
        public static async Task<BotConfiguration> LoadFromFolderAsync(string folder, string secret = null)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            var file = Directory.GetFiles(folder, "*.bot", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (file != null)
            {
                return await BotConfiguration.LoadAsync(file, secret).ConfigureAwait(false);
            }

            throw new FileNotFoundException($"Error: no bot file found in {folder}. Choose a different location or use msbot init to create a.bot file.");
        }

        /// <summary>
        /// Load the bot configuration by looking in a folder and loading the first .bot file in the folder.
        /// </summary>
        /// <param name="folder">Folder to look for bot files. </param>
        /// <param name="secret">Secret to use to encrypt keys. </param>
        /// <returns><see cref="BotConfiguration"/>.</returns>
        public static BotConfiguration LoadFromFolder(string folder, string secret = null)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            return BotConfiguration.LoadFromFolderAsync(folder, secret).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load the configuration from a .bot file.
        /// </summary>
        /// <param name="file">Path to bot file. </param>
        /// <param name="secret">Secret to use to decrypt the file on disk. </param>
        /// <returns><see cref="Task"/> of <see cref="BotConfiguration"/>.</returns>
        public static async Task<BotConfiguration> LoadAsync(string file, string secret = null)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            var json = string.Empty;
            using (var stream = File.OpenText(file))
            {
                json = await stream.ReadToEndAsync().ConfigureAwait(false);
            }

            var bot = JsonSerializer.Deserialize<BotConfiguration>(json);
            bot.Location = file;
            bot.MigrateData();

            var hasSecret = bot.Padlock?.Length > 0;
            if (hasSecret)
            {
                bot.Decrypt(secret);
            }

            return bot;
        }

        /// <summary>
        /// Load the configuration from a .bot file.
        /// </summary>
        /// <param name="file">Path to bot file. </param>
        /// <param name="secret">Secret to use to decrypt the file on disk. </param>
        /// <returns><see cref="BotConfiguration"/>.</returns>
        public static BotConfiguration Load(string file, string secret = null)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file));
            }

            return BotConfiguration.LoadAsync(file, secret).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generate a new key suitable for encrypting.
        /// </summary>
        /// <returns>key to use with <see cref="Encrypt(string)"/> method. </returns>
        public static string GenerateKey() => EncryptUtilities.GenerateKey();

        /// <summary>
        /// Save the file with secret.
        /// </summary>
        /// <param name="secret">Secret for encryption. </param>
        /// <returns><see cref="Task"/>.</returns>
        public Task SaveAsync(string secret = null) => this.SaveAsAsync(this.Location, secret);

        /// <summary>
        /// Save the file with secret.
        /// </summary>
        /// <param name="secret">Secret for encryption. </param>
        public void Save(string secret = null) => this.SaveAsync(secret).GetAwaiter().GetResult();

        /// <summary>
        /// Save the configuration to a .bot file.
        /// </summary>
        /// <param name="path">Path to bot file.</param>
        /// <param name="secret">Secret for encrypting the file keys.</param>
        /// <returns>Task. </returns>
        public async Task SaveAsAsync(string path = null, string secret = null)
        {
            // Validate state: Either path needs to be provided or Location needs to be set
            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(this.Location))
            {
                // If location is not set, we expect the path to be provided
#pragma warning disable CA2208 // Instantiate argument exceptions correctly (this class is obsolete, we won't fix it)
                throw new ArgumentException(nameof(path));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            if (!string.IsNullOrEmpty(secret))
            {
                this.ValidateSecret(secret);
            }

            var hasSecret = this.Padlock?.Length > 0;

            // Make sure that all dispatch serviceIds still match services that are in the bot
            foreach (var dispatchService in this.Services.Where(s => s.Type == ServiceTypes.Dispatch).Cast<DispatchService>())
            {
                dispatchService.ServiceIds = dispatchService.ServiceIds
                        .Where(serviceId => this.Services.Any(s => s.Id == serviceId))
                        .ToList();
            }

            if (hasSecret)
            {
                // Make sure fields are encrypted before serialization
                this.Encrypt(secret);
            }

            // Save it to disk
            using (var file = File.Open(path ?? this.Location, FileMode.Create))
            {
                using (var textWriter = new StreamWriter(file))
                {
                    await textWriter.WriteLineAsync(JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })).ConfigureAwait(false);
                }
            }

            if (hasSecret)
            {
                // Make sure all in memory fields are decrypted again for continued operations
                this.Decrypt(secret);
            }
        }

        /// <summary>
        /// Save the configuration to a .bot file.
        /// </summary>
        /// <param name="path">Path to bot file.</param>
        /// <param name="secret">Secret for encrypting the file keys.</param>
        public void SaveAs(string path, string secret = null)
        {
            // Validate state: Either path needs to be provided or Location needs to be set
            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(this.Location))
            {
                // If location is not set, we expect the path to be provided
#pragma warning disable CA2208 // Instantiate argument exceptions correctly (this class is obsolete, we won't fix it)
                throw new ArgumentException(nameof(path));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            SaveAsAsync(path, secret).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Clear secret.
        /// </summary>
        public void ClearSecret() => this.Padlock = string.Empty;

        /// <summary>
        /// Connect a service to the bot file.
        /// </summary>
        /// <param name="newService"><see cref="ConnectedService"/> to add.</param>
        public void ConnectService(ConnectedService newService)
        {
            if (newService == null)
            {
                throw new ArgumentNullException(nameof(newService));
            }

            if (string.IsNullOrEmpty(newService.Id))
            {
                int maxValue = 0;
                foreach (var service in this.Services)
                {
                    if (int.TryParse(service.Id, out int id) && id > maxValue)
                    {
                        maxValue = id;
                    }
                }

#pragma warning disable CA1305 // Specify IFormatProvider (this class is obsolete, we won't fix it)
                newService.Id = (++maxValue).ToString();
#pragma warning restore CA1305 // Specify IFormatProvider
            }
            else if (this.Services.Where(s => s.Type == newService.Type && s.Id == newService.Id).Any())
            {
                throw new InvalidOperationException($"service with {newService.Id} is already connected");
            }

            this.Services.Add(newService);
        }

        /// <summary>
        /// Encrypt all values in the in memory config.
        /// </summary>
        /// <param name="secret">Secret to encrypt.</param>
        public void Encrypt(string secret)
        {
            this.ValidateSecret(secret);

            foreach (var service in this.Services)
            {
                service.Encrypt(secret);
            }
        }

        /// <summary>
        /// Decrypt all values in the in memory config.
        /// </summary>
        /// <param name="secret">Secret to encrypt.</param>
        public void Decrypt(string secret)
        {
            this.ValidateSecret(secret);

            foreach (var service in this.Services)
            {
                service.Decrypt(secret);
            }
        }

        /// <summary>
        /// Find a service by its name or ID.
        /// </summary>
        /// <param name="nameOrId">The name or service ID to find.</param>
        /// <returns>The <see cref="ConnectedService"/>; or null if the service isn't found.</returns>
        public ConnectedService FindServiceByNameOrId(string nameOrId)
        {
            if (string.IsNullOrEmpty(nameOrId))
            {
                throw new ArgumentNullException(nameof(nameOrId));
            }

            return this.Services.FirstOrDefault(s => s.Id == nameOrId || s.Name == nameOrId);
        }

        /// <summary>
        /// Find a specific type of service by its name or ID.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="nameOrId">The name or service ID to find.</param>
        /// <returns>The <see cref="ConnectedService"/>; or null if the service isn't found.</returns>
        public T FindServiceByNameOrId<T>(string nameOrId)
            where T : ConnectedService
        {
            if (string.IsNullOrEmpty(nameOrId))
            {
                throw new ArgumentNullException(nameof(nameOrId));
            }

            return (T)this.Services.FirstOrDefault(s =>
                s is T && (s.Id == nameOrId || s.Name == nameOrId));
        }

        /// <summary>
        /// Find a service by ID.
        /// </summary>
        /// <param name="id">The ID of the service to find.</param>
        /// <returns>The <see cref="ConnectedService"/>; or null if the service isn't found.</returns>
        public ConnectedService FindService(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return this.Services.FirstOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Remove a service by its name or ID.
        /// </summary>
        /// <param name="nameOrId">The name or service ID.</param>
        /// <returns>The <see cref="ConnectedService"/> that was found and removed.</returns>
        /// <exception cref="Exception">No such service was found.</exception>
        public ConnectedService DisconnectServiceByNameOrId(string nameOrId)
        {
            if (string.IsNullOrEmpty(nameOrId))
            {
                throw new ArgumentNullException(nameof(nameOrId));
            }

            var service = this.FindServiceByNameOrId(nameOrId);
            if (service == null)
            {
                throw new ArgumentException($"a service with id or name of[{nameOrId}] was not found");
            }

            this.Services.Remove(service);
            return service;
        }

        /// <summary>
        /// Remove a specific type of service by its name or ID.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="nameOrId">The name or service ID.</param>
        /// <returns>The <see cref="ConnectedService"/> that was found and removed.</returns>
        /// <exception cref="Exception">No such service was found.</exception>
        public T DisconnectServiceByNameOrId<T>(string nameOrId)
            where T : ConnectedService
        {
            if (string.IsNullOrEmpty(nameOrId))
            {
                throw new ArgumentNullException(nameof(nameOrId));
            }

            var service = this.FindServiceByNameOrId<T>(nameOrId);
            if (service == null)
            {
                throw new ArgumentException($"a service with id or name of[{nameOrId}] was not found");
            }

            this.Services.Remove(service);
            return service;
        }

        /// <summary>
        /// Remove a service by its ID.
        /// </summary>
        /// <param name="id">The ID of the service.</param>
        public void DisconnectService(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var service = this.FindService(id);
            if (service != null)
            {
                this.Services.Remove(service);
            }
        }

        /// <summary>
        /// Make sure secret is correct by decrypting the secretKey with it.
        /// </summary>
        /// <param name="secret">Secret to use.</param>
        protected void ValidateSecret(string secret)
        {
            if (secret?.Length == null)
            {
                throw new InvalidOperationException("You are attempting to perform an operation which needs access to the secret and --secret is missing");
            }

            try
            {
                if (this.Padlock?.Length == 0)
                {
                    // If no key, create a guid and enrypt that to use as secret validator.
                    this.Padlock = Guid.NewGuid().ToString("n").Encrypt(secret);
                }
                else
                {
                    // This will throw exception if invalid secret.
                    this.Padlock.Decrypt(secret);
                }
            }
            catch
            {
                throw new InvalidOperationException("You are attempting to perform an operation which needs access to the secret and --secret is incorrect.");
            }
        }

        /// <summary>
        /// migrate old records to new records.
        /// </summary>
        protected virtual void MigrateData()
        {
            // migrate old secretKey
            var secretKey = Properties[SECRETKEY].GetString();
            if (secretKey != null)
            {
                if (this.Padlock == null)
                {
                    this.Padlock = secretKey;
                }

                this.Properties.Remove(SECRETKEY);
            }

            foreach (var service in this.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.Bot:
                        {
                            var botService = (BotService)service;

                            // old bot service records may not have the appId on the bot, but we probably have it already on an endpoint
                            if (string.IsNullOrEmpty(botService.AppId))
                            {
                                botService.AppId = this.Services.Where(s => s.Type == ServiceTypes.Endpoint).Cast<EndpointService>()
                                    .Where(ep => !string.IsNullOrEmpty(ep.AppId))
                                    .Select(ep => ep.AppId)
                                    .FirstOrDefault();
                            }
                        }

                        break;

                    default:
                        break;
                }
            }

            // this is now a 2.0 version of the schema
            this.Version = "2.0";
        }

        /// <summary>
        /// Converter for strongly typed connected services.
        /// </summary>
#pragma warning disable CA1812 // Internal class that is apparently never instantiated (this class is obsolete, we won't fix this)
        internal class BotServiceConverter : JsonConverter<List<ConnectedService>>
#pragma warning restore CA1812 // Internal class that is apparently never instantiated
        {
            public override List<ConnectedService> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var services = new List<ConnectedService>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var service = JsonSerializer.Deserialize<ConnectedService>(ref reader, SerializationConfig.DefaultDeserializeOptions);

                        switch (service.Type)
                        {
                            case ServiceTypes.Bot:
                                services.Add(service.ToObject<BotService>());
                                break;
                            case ServiceTypes.AppInsights:
                                services.Add(service.ToObject<AppInsightsService>());
                                break;
                            case ServiceTypes.BlobStorage:
                                services.Add(service.ToObject<BlobStorageService>());
                                break;
                            case ServiceTypes.CosmosDB:
                                services.Add(service.ToObject<CosmosDbService>());
                                break;
                            case ServiceTypes.Dispatch:
                                services.Add(service.ToObject<DispatchService>());
                                break;
                            case ServiceTypes.Endpoint:
                                services.Add(service.ToObject<EndpointService>());
                                break;
                            case ServiceTypes.File:
                                services.Add(service.ToObject<FileService>());
                                break;
                            case ServiceTypes.Luis:
                                services.Add(service.ToObject<LuisService>());
                                break;
                            case ServiceTypes.QnA:
                                services.Add(service.ToObject<QnAMakerService>());
                                break;
                            case ServiceTypes.Generic:
                                services.Add(service.ToObject<GenericService>());
                                break;
                            default:
                                System.Diagnostics.Trace.TraceWarning($"Unknown service type {service.Type}");
                                services.Add(service);
                                break;
                        }
                    }
                }

                return services;
            }

            public override void Write(Utf8JsonWriter writer, List<ConnectedService> value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}
