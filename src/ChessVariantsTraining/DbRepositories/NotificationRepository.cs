using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public class NotificationRepository : INotificationRepository
    {
        MongoSettings settings;
        IMongoCollection<Notification> notificationCollection;

        public NotificationRepository(IOptions<Settings> appSettings)
        {
            settings = appSettings.Value.Mongo;

            GetCollection();
        }

        void GetCollection()
        {
            MongoClient client = new MongoClient(settings.MongoConnectionString);

            notificationCollection = client.GetDatabase(settings.Database).GetCollection<Notification>(settings.NotificationCollectionName);
        }

        public async Task<bool> AddAsync(Notification notification)
        {
            try
            {
                await notificationCollection.InsertOneAsync(notification);
                return true;
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return (await notificationCollection.CountAsync(Builders<Notification>.Filter.Eq("_id", id))) > 0;
        }

        public async Task MarkAllReadAsync(int user)
        {
            FilterDefinition<Notification> filter = Builders<Notification>.Filter.Eq("user", user);
            UpdateDefinition<Notification> update = Builders<Notification>.Update.Set("read", true);
            await notificationCollection.UpdateManyAsync(filter, update);
        }

        public async Task<long> UnreadCountAsync(int user)
        {
            return await notificationCollection.CountAsync(Builders<Notification>.Filter.Eq("user", user) & Builders<Notification>.Filter.Eq("read", false));
        }

        public async Task<List<Notification>> GetNotificationsForAsync(int user)
        {
            return await notificationCollection.Find(Builders<Notification>.Filter.Eq("user", user)).Sort(Builders<Notification>.Sort.Descending("timestampUtc")).ToListAsync();
        }
    }
}
