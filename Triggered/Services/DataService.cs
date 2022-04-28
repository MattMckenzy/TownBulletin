﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using Triggered.Models;

namespace Triggered.Services
{
    /// <summary>
    /// A singleton service that contains methods to persist dynamic <see cref="ExpandoObject"/> objects in the database.
    /// </summary>
    public class DataService
    {
        private readonly IDbContextFactory<TriggeredDbContext> _dbContextFactory;

        /// <summary>
        /// Default constructor with injected services.
        /// </summary>
        /// <param name="dbContextFactory">Injected <see cref="IDbContextFactory{TContext}"/> of <see cref="TriggeredDbContext"/>.</param>
        public DataService(IDbContextFactory<TriggeredDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Saves the given <see cref="ExpandoObject"/> under the given dot-syntax key.
        /// </summary>
        /// <param name="key">Dot-syntax key (i.e. "Twitch.Users.MattMckenzy").</param>
        /// <param name="expandoObject">The <see cref="ExpandoObject"/> to save under the given key.</param>
        public async Task SetObject(string key, ExpandoObject expandoObject)
        {
            TriggeredDbContext triggeredDbContext = await _dbContextFactory.CreateDbContextAsync();
            DataObject dataObject = new(key, key.Split(".", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length)
            {
                ExpandoObjectJson = JsonConvert.SerializeObject(expandoObject, Formatting.Indented)
            };

            if (triggeredDbContext.DataObjects.Contains(dataObject))
                triggeredDbContext.Update(dataObject);
            else
                triggeredDbContext.Add(dataObject);

            await triggeredDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the <see cref="ExpandoObject"/> stored at the given dot-syntax key, or null if it doesn't exist.
        /// </summary>
        /// <param name="key">The dot-syntax key used to retrieve the <see cref="ExpandoObject"/> (i.e. "Twitch.Users.MattMckenzy").</param>
        /// <returns>The <see cref="ExpandoObject"/>, or null if nothing was found at the given key.</returns>
        public async Task<ExpandoObject?> GetObject(string key)
        {
            TriggeredDbContext triggeredDbContext = await _dbContextFactory.CreateDbContextAsync();
            string? expandoObjectJson = triggeredDbContext.DataObjects.FirstOrDefault(obj => obj.Key.Equals(key))?.ExpandoObjectJson;
            if (!string.IsNullOrWhiteSpace(expandoObjectJson))
                return JsonConvert.DeserializeObject<ExpandoObject>(expandoObjectJson);
            else
                return null;
        }

        /// <summary>
        /// Retrieves all <see cref="ExpandoObject"/>s that are children of the given dot-syntax key.
        /// </summary>
        /// <param name="key">The dot-syntax key from which to retrieve children. (i.e. "Twitch.Users" retrieves "Twitch.Users.MattMckenzy" and "Twitch.Users.SirSquad").</param>
        /// <returns>An enumerable of children <see cref="ExpandoObject"/>.</returns>
        public async Task<IEnumerable<ExpandoObject?>> GetChildren(string key)
        {
            TriggeredDbContext triggeredDbContext = await _dbContextFactory.CreateDbContextAsync();
            IEnumerable<DataObject> dataObjects = triggeredDbContext.DataObjects
                .Where(dataObjects => dataObjects.Key.StartsWith(key) && dataObjects.Depth == key.Split(".", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length + 1);

            return dataObjects.Where(dataObject => !string.IsNullOrWhiteSpace(dataObject.ExpandoObjectJson))
                .Select(dataObject => JsonConvert.DeserializeObject<ExpandoObject>(dataObject.ExpandoObjectJson!));
        }

        /// <summary>
        /// Removes the <see cref="ExpandoObject"/> stored at the given dot-syntax key, or does nothing if it doesn't exist.
        /// </summary>
        /// <param name="key">The dot-syntax key used of the <see cref="ExpandoObject"/> to remove (i.e. "Twitch.Users.MattMckenzy").</param>
        public async Task RemoveObject(string key)
        {
            TriggeredDbContext triggeredDbContext = await _dbContextFactory.CreateDbContextAsync();
            DataObject? dataObject = triggeredDbContext.DataObjects.FirstOrDefault(obj => obj.Key.Equals(key));
            if (dataObject != null)
            {
                triggeredDbContext.DataObjects.Remove(dataObject);
                await triggeredDbContext.SaveChangesAsync();
            }

            return;
        }
    }
}