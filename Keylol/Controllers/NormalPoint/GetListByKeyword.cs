﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     根据关键字搜索对应据点
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="full">是否获取完整的据点信息，包括读者文章数，订阅状态等，默认 false</param>
        /// <param name="typeFilter">据点类型过滤器，用逗号分个多个类型的名字，null 表示全部类型，默认 null</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 5</param>
        [Route("keyword/{keyword}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof(List<NormalPointDto>))]
        public async Task<IHttpActionResult> GetListByKeyword(string keyword, bool full = false,
            string typeFilter = null, int skip = 0, int take = 5)
        {
            if (take > 50) take = 50;
            var typeFilterSql = string.Empty;
            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                var inTypes = string.Join(", ",
                    typeFilter.Split(',').Select(s => (int) s.Trim().ToEnum<NormalPointType>()));
                typeFilterSql = $"WHERE [t1].[Type] IN ({inTypes})";
            }

            if (!full)
            {
                return Ok((await _dbContext.NormalPoints.SqlQuery(
                    @"SELECT * FROM [dbo].[NormalPoints] AS [t1] INNER JOIN (
                        SELECT [t2].[KEY], SUM([t2].[RANK]) as RANK FROM (
		                    SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([EnglishName], [EnglishAliases]), {0})
		                    UNION ALL
		                    SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([ChineseName], [ChineseAliases]), {0})
	                    ) AS [t2] GROUP BY [t2].[KEY]
                    ) AS [t3] ON [t1].[Id] = [t3].[KEY] " + typeFilterSql + @"
                    ORDER BY [t3].[RANK] DESC
                    OFFSET ({1}) ROWS FETCH NEXT ({2}) ROWS ONLY",
                    $"\"{keyword}\" OR \"{keyword}*\"", skip, take).AsNoTracking().ToListAsync()).Select(
                        point => new NormalPointDto(point)));
            }

            var points = await _dbContext.Database.SqlQuery<NormalPointDto>(@"SELECT
                [t4].[Count],
                [t4].[Id],
                [t4].[PreferredName],
                [t4].[IdCode],
                [t4].[ChineseName],
                [t4].[EnglishName],
                [t4].[AvatarImage],
                [t4].[BackgroundImage],
                [t4].[Type],
	            (SELECT
		            COUNT(1)
		            FROM [dbo].[UserPointSubscriptions]
		            WHERE [t4].[Id] = [dbo].[UserPointSubscriptions].[Point_Id]) AS [SubscriberCount],
	            CASE WHEN (EXISTS (SELECT 1 FROM [dbo].[UserPointSubscriptions] WHERE [dbo].[UserPointSubscriptions].[Point_Id] = [t4].[Id] AND [dbo].[UserPointSubscriptions].[KeylolUser_Id] = {1})) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [Subscribed],
                (SELECT
                    COUNT(1)
                    FROM  [dbo].[ArticlePointPushes]
                    INNER JOIN [dbo].[Articles] ON [dbo].[Articles].[Id] = [dbo].[ArticlePointPushes].[Article_Id]
                    WHERE ([t4].[Id] = [dbo].[ArticlePointPushes].[NormalPoint_Id])) AS [ArticleCount]
                FROM (SELECT
                    *,
                    COUNT(1) OVER() AS [Count]
                    FROM [dbo].[NormalPoints] AS [t1]
                    INNER JOIN (SELECT
                        [t2].[KEY],
                        SUM([t2].[RANK]) AS RANK
                        FROM (SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([EnglishName], [EnglishAliases]), {0})
                            UNION ALL
                            SELECT * FROM CONTAINSTABLE([dbo].[NormalPoints], ([ChineseName], [ChineseAliases]), {0})) AS[t2]
                        GROUP BY [t2].[KEY])
                    AS [t3] ON [t1].[Id] = [t3].[KEY] " + typeFilterSql + @"
                    ORDER BY [t3].[RANK] DESC
                    OFFSET({2}) ROWS FETCH NEXT({3}) ROWS ONLY) AS [t4]",
                $"\"{keyword}\" OR \"{keyword}*\"", User.Identity.GetUserId(), skip, take).ToListAsync();

            var response = Request.CreateResponse(HttpStatusCode.OK, points);
            response.Headers.Add("X-Total-Record-Count", points.Count > 0 ? points[0].Count.ToString() : "0");
            return ResponseMessage(response);
        }
    }
}