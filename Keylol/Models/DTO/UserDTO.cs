﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using SteamKit2;

namespace Keylol.Models.DTO
{
    [DataContract]
    public class UserDTO
    {
        private KeylolUser _user;
        public UserDTO(KeylolUser user, bool includeSteam = false, bool includeSecurity = false)
        {
            _user = user;
            Id = user.Id;
            IdCode = user.IdCode;
            UserName = user.UserName;
            GamerTag = user.GamerTag;

            // Ignore ProfilePointBackgroundImage

            AvatarImage = user.AvatarImage;
            
            if (includeSteam)
                IncludeSteam();

            if (includeSecurity)
                IncludeSecurity();

            // Ignore claims

            // Ignore SteamBot

            // Ignore stats

            // Ignore subscribed
        }

        public void IncludeSecurity()
        {
            LockoutEnabled = _user.LockoutEnabled;
            Email = _user.Email;
        }

        public void IncludeSteam()
        {
            SteamId = _user.SteamId;
            var steamId = new SteamID();
            steamId.SetFromSteam3String(SteamId);
            SteamId64 = steamId.ConvertToUInt64().ToString();
            SteamProfileName = _user.SteamProfileName;
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string IdCode { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string GamerTag { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string AvatarImage { get; set; }
        [DataMember]
        public string ProfilePointBackgroundImage { get; set; }
        [DataMember]
        public bool? LockoutEnabled { get; set; }
        [DataMember]
        public string SteamId { get; set; }
        [DataMember]
        public string SteamId64 { get; set; }
        [DataMember]
        public string SteamProfileName { get; set; }

        [DataMember]
        public string StatusClaim { get; set; }
        [DataMember]
        public string StaffClaim { get; set; }

        [DataMember]
        public SteamBotDTO SteamBot { get; set; }

        [DataMember]
        public int? SubscribedPointCount { get; set; }

        [DataMember]
        public int? SubscriberCount { get; set; }
        [DataMember]
        public int? ArticleCount { get; set; }

        [DataMember]
        public bool? Subscribed { get; set; }
    }

    public class UserWithMoreOptionsDTO:UserDTO
    {
        public UserWithMoreOptionsDTO(KeylolUser user) : base(user)
        {
            AutoShareOnAcquiringNewGame = user.AutoShareOnAcquiringNewGame;
            AutoShareOnAddingFavorite = user.AutoShareOnAddingFavorite;
            AutoShareOnAddingNewFriend = user.AutoShareOnAddingNewFriend;
            AutoShareOnAddingVideo = user.AutoShareOnAddingVideo;
            AutoShareOnCreatingGroup = user.AutoShareOnCreatingGroup;
            AutoShareOnJoiningGroup = user.AutoShareOnJoiningGroup;
            AutoShareOnPublishingReview = user.AutoShareOnPublishingReview;
            AutoShareOnUnlockingAchievement = user.AutoShareOnUnlockingAchievement;
            AutoShareOnUpdatingWishlist = user.AutoShareOnUpdatingWishlist;
            AutoShareOnUploadingScreenshot = user.AutoShareOnUploadingScreenshot;

            EmailNotifyOnAdvertisement = user.EmailNotifyOnAdvertisement;
            EmailNotifyOnArticleReplied = user.EmailNotifyOnArticleReplied;
            EmailNotifyOnCommentReplied = user.EmailNotifyOnCommentReplied;
            EmailNotifyOnEditorRecommended = user.EmailNotifyOnEditorRecommended;
            EmailNotifyOnMessageReceived = user.EmailNotifyOnMessageReceived;

            MessageNotifyOnArticleLiked = user.MessageNotifyOnArticleLiked;
            MessageNotifyOnArticleReplied = user.MessageNotifyOnArticleReplied;
            MessageNotifyOnCommentLiked = user.MessageNotifyOnCommentLiked;
            MessageNotifyOnCommentReplied = user.MessageNotifyOnCommentReplied;
            MessageNotifyOnEditorRecommended = user.MessageNotifyOnEditorRecommended;
        }

        [DataMember]
        public bool AutoShareOnAddingNewFriend { get; set; }
        [DataMember]
        public bool AutoShareOnUnlockingAchievement { get; set; }
        [DataMember]
        public bool AutoShareOnAcquiringNewGame { get; set; }
        [DataMember]
        public bool AutoShareOnJoiningGroup { get; set; }
        [DataMember]
        public bool AutoShareOnCreatingGroup { get; set; }
        [DataMember]
        public bool AutoShareOnUpdatingWishlist { get; set; }
        [DataMember]
        public bool AutoShareOnPublishingReview { get; set; }
        [DataMember]
        public bool AutoShareOnUploadingScreenshot { get; set; }
        [DataMember]
        public bool AutoShareOnAddingVideo { get; set; }
        [DataMember]
        public bool AutoShareOnAddingFavorite { get; set; }

        [DataMember]
        public bool EmailNotifyOnArticleReplied { get; set; }
        [DataMember]
        public bool EmailNotifyOnCommentReplied { get; set; }
        [DataMember]
        public bool EmailNotifyOnEditorRecommended { get; set; }
        [DataMember]
        public bool EmailNotifyOnMessageReceived { get; set; }
        [DataMember]
        public bool EmailNotifyOnAdvertisement { get; set; }

        [DataMember]
        public bool MessageNotifyOnArticleReplied { get; set; }
        [DataMember]
        public bool MessageNotifyOnCommentReplied { get; set; }
        [DataMember]
        public bool MessageNotifyOnEditorRecommended { get; set; }
        [DataMember]
        public bool MessageNotifyOnArticleLiked { get; set; }
        [DataMember]
        public bool MessageNotifyOnCommentLiked { get; set; }
    }
}
