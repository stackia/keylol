﻿using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.Comment
{
    /// <summary>
    ///     评论 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("comment")]
    public partial class CommentController : KeylolApiController
    {
        /// <summary>
        /// 创建 CommentController
        /// </summary>
        /// <param name="dbContext">KeylolDbContext</param>
        public CommentController(KeylolDbContext dbContext) : base(dbContext)
        {
        }
    }
}