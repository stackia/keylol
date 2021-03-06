﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Identity;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.Comment
{
    public partial class CommentController
    {
        /// <summary>
        ///     排序方式
        /// </summary>
        public enum OrderByType
        {
            /// <summary>
            ///     楼层号降序
            /// </summary>
            SequenceNumberForAuthor,

            /// <summary>
            ///     认可数降序
            /// </summary>
            LikeCount
        }

        /// <summary>
        ///     获取指定文章下的评论（被封存的文章只能作者和运维职员可见）
        /// </summary>
        /// <remarks>响应 Header 中 X-Total-Record-Count 记录了当前文章下的总评论数目</remarks>
        /// <param name="articleId">文章 ID</param>
        /// <param name="orderBy">排序字段，默认 "SequenceNumberForAuthor"</param>
        /// <param name="desc">true 表示降序，false 表示升序，默认 false</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 20</param>
        [Route]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof(List<CommentDto>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定文章不存在")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "文章被封存，当前登录用户无权查看评论")]
        public async Task<IHttpActionResult> GetListByArticleId(string articleId,
            OrderByType orderBy = OrderByType.SequenceNumberForAuthor,
            bool desc = false, int skip = 0, int take = 20)
        {
            var userId = User.Identity.GetUserId();
            if (take > 50) take = 50;

            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
                return NotFound();

            var isKeylolOperator = User.IsInRole(KeylolRoles.Operator);
            if (article.Archived != ArchivedState.None &&
                userId != article.PrincipalId && !isKeylolOperator)
                return Unauthorized();

            var commentsQuery = _dbContext.Comments.AsNoTracking()
                .Where(comment => comment.ArticleId == articleId);
            switch (orderBy)
            {
                case OrderByType.SequenceNumberForAuthor:
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.SequenceNumberForArticle)
                        : commentsQuery.OrderBy(c => c.SequenceNumberForArticle);
                    break;

                case OrderByType.LikeCount:
                    commentsQuery = desc
                        ? commentsQuery.OrderByDescending(c => c.Likes.Count)
                        : commentsQuery.OrderBy(c => c.Likes.Count);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null);
            }
            var commentEntries = await commentsQuery.Skip(() => skip).Take(() => take).Select(comment =>
                new
                {
                    comment,
                    likeCount = comment.Likes.Count,
                    liked = comment.Likes.Any(l => l.OperatorId == userId),
                    commentator = comment.Commentator
                })
                .ToListAsync();
            var response = Request.CreateResponse(HttpStatusCode.OK,
                commentEntries.Select(entry =>
                {
                    if (entry.comment.Archived != ArchivedState.None &&
                        userId != entry.comment.CommentatorId && !isKeylolOperator)
                        return new CommentDto
                        {
                            SequenceNumberForArticle = entry.comment.SequenceNumberForArticle
                        };
                    return new CommentDto(entry.comment)
                    {
                        Commentator = new UserDto(entry.commentator),
                        LikeCount = entry.likeCount,
                        Liked = entry.liked,
                        Archived = entry.comment.Archived,
                        Warned = entry.comment.Warned
                    };
                }).ToList());
            var commentCount = await _dbContext.Comments.Where(c => c.ArticleId == articleId).CountAsync();
            response.Headers.SetTotalCount(commentCount);
            return ResponseMessage(response);
        }
    }
}