﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.StateTreeManager;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Keylol.States
{
    /// <summary>
    /// State Tree Root
    /// </summary>
    public class Root
    {
        /// <summary>
        /// 获取新的完整状态树
        /// </summary>
        /// <param name="page">访问的页面</param>
        /// <param name="userManager"><see cref="KeylolUserManager"/></param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="coupon"><see cref="CouponProvider"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns>完整状态树</returns>
        public static async Task<Root> Get(Page page, [Injected] KeylolUserManager userManager,
            [Injected] KeylolDbContext dbContext, [Injected] CouponProvider coupon,
            [Injected] CachedDataProvider cachedData)
        {
            var root = new Root();
            var currentUserId = StateTreeHelper.CurrentUser().Identity.GetUserId();
            if (await StateTreeHelper.CanAccessAsync<Root>(nameof(CurrentUser)))
            {
                var user = await userManager.FindByIdAsync(currentUserId);
                root.CurrentUser = await CurrentUser.CreateAsync(user, userManager, dbContext, coupon);
            }

            switch (page)
            {
                case Page.Discovery:
                    root.DiscoveryPage =
                        await States.DiscoveryPage.DiscoveryPage.CreateAsync(currentUserId, dbContext, cachedData);
                    break;

                case Page.Points:
                    break;

                case Page.Timeline:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }

            return root;
        }

        /// <summary>
        /// 需要访问的页面
        /// </summary>
        public enum Page
        {
            /// <summary>
            /// 发现
            /// </summary>
            Discovery,

            /// <summary>
            /// 据点
            /// </summary>
            Points,

            /// <summary>
            /// 轨道
            /// </summary>
            Timeline
        }

        /// <summary>
        /// 当前登录的用户
        /// </summary>
        [Authorize]
        public CurrentUser CurrentUser { get; set; }

        /// <summary>
        /// 发现页
        /// </summary>
        public DiscoveryPage.DiscoveryPage DiscoveryPage { get; set; }
    }
}