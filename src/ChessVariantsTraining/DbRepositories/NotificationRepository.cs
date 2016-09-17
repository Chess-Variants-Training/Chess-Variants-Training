using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

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

        public bool Add(Notification notification)
        {
            try
            {
                notificationCollection.InsertOne(notification);
                return true;
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
        }


        public bool Exists(string id)
        {
            return notificationCollection.Count(Builders<Notification>.Filter.Eq("_id", id)) > 0;
        }

        public void MarkAllRead(int user)
        {
            FilterDefinition<Notification> filter = Builders<Notification>.Filter.Eq("user", user);
            UpdateDefinition<Notification> update = Builders<Notification>.Update.Set("read", true);
            notificationCollection.UpdateMany(filter, update);
        }

        public long UnreadCount(int user)
        {
            return notificationCollection.Count(Builders<Notification>.Filter.Eq("user", user) & Builders<Notification>.Filter.Eq("read", false));
        }

        public List<Notification> GetNotificationsFor(int user)
        {
            return notificationCollection.Find(Builders<Notification>.Filter.Eq("user", user)).Sort(Builders<Notification>.Sort.Descending("timestampUtc")).ToList();
        }
    }
}
