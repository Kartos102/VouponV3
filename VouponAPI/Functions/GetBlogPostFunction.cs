using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;
using Voupon.API.Util;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Voupon.API.Functions.BlogPost
{
    public class GetBlogPostFunction
    {
        private readonly HttpClient _http;

        public GetBlogPostFunction(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
        }

        [OpenApiOperation(operationId: "Get blog post", tags: new[] { "Blog" }, Description = "Get blog post", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Id of the blog post", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BlogPostResponseModel), Summary = "The paginated result of reviews")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id or external id is supplied")]


        [FunctionName("GetBlogPostFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blogs/post")] HttpRequest req, ILogger log)
        {
            var response = new BlogPostResponseModel
            {
                Data = new BlogData()
            };

            try
            {
                var tempId = req.Query["id"];

                var limit = 0;
                int.TryParse(tempId, out limit);

                var id = 0;
                int.TryParse(tempId, out id);

                try
                {
                    var result = await _http.GetAsync($"https://public-api.wordpress.com/rest/v1.1/sites/vodusresearch.wordpress.com/posts/{id}");

                    var wordpressContent = JsonConvert.DeserializeObject<WordpressBlogPost>(await result.Content.ReadAsStringAsync());

                    //var relatedBlog
                    if (wordpressContent != null)
                    {
                        response.Data = new BlogData
                        {
                            Related = new List<Related>(),
                            Blog = new Blog
                            {
                                Id = wordpressContent.Id,
                                Title = wordpressContent.Title,
                                Content = wordpressContent.Content,
                                Slug = wordpressContent.Slug,
                                FeaturedImage = (wordpressContent.FeaturedImage != null ? wordpressContent.FeaturedImage.ToString() : ""),
                                Tags = wordpressContent.TagNames,
                                Categories = wordpressContent.CategoryNames,
                                Date = wordpressContent.Date.Date
                            }
                        };
                    }

                    var stringContent = new StringContent("", Encoding.UTF8, "application/json");
                    var relatedResult = await _http.PostAsync($"https://public-api.wordpress.com/rest/v1.1/sites/vodusresearch.wordpress.com/posts/{id}/related", stringContent);

                    var relatedWordpressContent = JsonConvert.DeserializeObject<WordpressRelatedPosts>(await relatedResult.Content.ReadAsStringAsync());

                    if (relatedWordpressContent != null)
                    {
                        if(relatedWordpressContent.Total > 0)
                        {
                            var relatedList = new List<Related>();
                            foreach(var related in relatedWordpressContent.Hits)
                            {
                                var relatedPostResult = await _http.GetAsync($"https://public-api.wordpress.com/rest/v1.1/sites/vodusresearch.wordpress.com/posts/{related.Fields.PostId}");

                                var relatedPost = JsonConvert.DeserializeObject<WordpressBlogPost>(await relatedPostResult.Content.ReadAsStringAsync());

                                //var relatedBlog
                                if (relatedPost != null)
                                {
                                    if(relatedPost.FeaturedImage == null || string.IsNullOrEmpty(relatedPost.Title))
                                    {
                                        continue;
                                    }
                                        relatedList.Add(new Related
                                    {
                                        Id = relatedPost.Id,
                                        Slug = relatedPost.Slug,
                                        Title = relatedPost.Title,
                                        FeaturedImage = (relatedPost.FeaturedImage != null ? relatedPost.FeaturedImage.ToString() : ""),
                                        Date = relatedPost.Date.Date
                                    });
                                }
                            }
                            if(relatedList != null && relatedList.Any())
                            {
                                response.Data.Related = relatedList;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    return new NotFoundObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Not found"
                    });
                }
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Fail to get data [099]"
                });

            }
        }
    }

    public class BlogData
    {
        public Blog Blog { get; set; }
        public List<Related> Related { get; set; }
    }

    public class Blog
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string FeaturedImage { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Categories { get; set; }
    }

    public class Related
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string FeaturedImage { get; set; }
        public string Slug { get; set; }
        public DateTime Date { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class BlogPostResponseModel : ApiResponseViewModel
    {
        public BlogData Data { get; set; }
    }

    //  Related posts
    public partial class WordpressRelatedPosts
    {
        [JsonProperty("hits")]
        public Hit[] Hits { get; set; }

        [JsonProperty("max_score")]
        public double MaxScore { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

    public partial class Hit
    {
        [JsonProperty("_score")]
        public double Score { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }
    }

    public partial class Fields
    {
        [JsonProperty("post_id")]
        public long PostId { get; set; }

        [JsonProperty("blog_id")]
        public long BlogId { get; set; }
    }


    //  Wordpress post
    public partial class WordpressBlogPost
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("site_ID")]
        public long SiteId { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("modified")]
        public DateTimeOffset Modified { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("URL")]
        public Uri Url { get; set; }

        [JsonProperty("short_URL")]
        public Uri ShortUrl { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("excerpt")]
        public string Excerpt { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("guid")]
        public Uri Guid { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("sticky")]
        public bool Sticky { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("parent")]
        public bool Parent { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("discussion")]
        public Discussion Discussion { get; set; }

        [JsonProperty("likes_enabled")]
        public bool LikesEnabled { get; set; }

        [JsonProperty("sharing_enabled")]
        public bool SharingEnabled { get; set; }

        [JsonProperty("like_count")]
        public long LikeCount { get; set; }

        [JsonProperty("i_like")]
        public bool ILike { get; set; }

        [JsonProperty("is_reblogged")]
        public bool IsReblogged { get; set; }

        [JsonProperty("is_following")]
        public bool IsFollowing { get; set; }

        [JsonProperty("global_ID")]
        public string GlobalId { get; set; }

        [JsonProperty("featured_image")]
        public Uri FeaturedImage { get; set; }

        [JsonProperty("post_thumbnail")]
        public PostThumbnail PostThumbnail { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("geo")]
        public bool Geo { get; set; }

        [JsonProperty("menu_order")]
        public long MenuOrder { get; set; }

        [JsonProperty("page_template")]
        public string PageTemplate { get; set; }

        [JsonProperty("publicize_URLs")]
        public object[] PublicizeUrLs { get; set; }

        [JsonProperty("terms")]
        public Terms Terms { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, object> Tags { get; set; }

        public List<string> TagNames
        {
            get
            {
                return Tags.Select(x => x.Key).ToList();
            }
        }


        [JsonProperty("categories")]
        public Dictionary<string, object> Categories { get; set; }

        public List<string> CategoryNames
        {
            get
            {
                return Categories.Select(x => x.Key).ToList();
            }
        }


        [JsonProperty("attachments")]
        public Attachments Attachments { get; set; }

        [JsonProperty("attachment_count")]
        public long AttachmentCount { get; set; }

        [JsonProperty("metadata")]
        public Metadatum[] Metadata { get; set; }

        [JsonProperty("meta")]
        public WordpressBlogPostMeta Meta { get; set; }

        [JsonProperty("capabilities")]
        public Capabilities Capabilities { get; set; }

        [JsonProperty("other_URLs")]
        public OtherUrLs OtherUrLs { get; set; }
    }

    public partial class Attachments
    {
        [JsonProperty("8")]
        public The8 The8 { get; set; }
    }

    public partial class The8
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("URL")]
        public Uri Url { get; set; }

        [JsonProperty("guid")]
        public Uri Guid { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("post_ID")]
        public long PostId { get; set; }

        [JsonProperty("author_ID")]
        public long AuthorId { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("alt")]
        public string Alt { get; set; }

        [JsonProperty("thumbnails")]
        public Thumbnails Thumbnails { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("exif")]
        public Exif Exif { get; set; }

        [JsonProperty("meta")]
        public The8_Meta Meta { get; set; }
    }

    public partial class Exif
    {
        [JsonProperty("aperture")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Aperture { get; set; }

        [JsonProperty("credit")]
        public string Credit { get; set; }

        [JsonProperty("camera")]
        public string Camera { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("created_timestamp")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long CreatedTimestamp { get; set; }

        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        [JsonProperty("focal_length")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long FocalLength { get; set; }

        [JsonProperty("iso")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Iso { get; set; }

        [JsonProperty("shutter_speed")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long ShutterSpeed { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("orientation")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Orientation { get; set; }

        [JsonProperty("keywords")]
        public object[] Keywords { get; set; }
    }

    public partial class The8_Meta
    {
        [JsonProperty("links")]
        public PurpleLinks Links { get; set; }
    }

    public partial class PurpleLinks
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("help")]
        public Uri Help { get; set; }

        [JsonProperty("site")]
        public Uri Site { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Parent { get; set; }
    }

    public partial class Thumbnails
    {
        [JsonProperty("thumbnail")]
        public Uri Thumbnail { get; set; }

        [JsonProperty("medium")]
        public Uri Medium { get; set; }

        [JsonProperty("large")]
        public Uri Large { get; set; }

        [JsonProperty("newspack-article-block-landscape-large")]
        public Uri NewspackArticleBlockLandscapeLarge { get; set; }

        [JsonProperty("newspack-article-block-portrait-large")]
        public Uri NewspackArticleBlockPortraitLarge { get; set; }

        [JsonProperty("newspack-article-block-square-large")]
        public Uri NewspackArticleBlockSquareLarge { get; set; }

        [JsonProperty("newspack-article-block-landscape-medium")]
        public Uri NewspackArticleBlockLandscapeMedium { get; set; }

        [JsonProperty("newspack-article-block-portrait-medium")]
        public Uri NewspackArticleBlockPortraitMedium { get; set; }

        [JsonProperty("newspack-article-block-square-medium")]
        public Uri NewspackArticleBlockSquareMedium { get; set; }

        [JsonProperty("newspack-article-block-landscape-small")]
        public Uri NewspackArticleBlockLandscapeSmall { get; set; }

        [JsonProperty("newspack-article-block-portrait-small")]
        public Uri NewspackArticleBlockPortraitSmall { get; set; }

        [JsonProperty("newspack-article-block-square-small")]
        public Uri NewspackArticleBlockSquareSmall { get; set; }

        [JsonProperty("newspack-article-block-landscape-tiny")]
        public Uri NewspackArticleBlockLandscapeTiny { get; set; }

        [JsonProperty("newspack-article-block-portrait-tiny")]
        public Uri NewspackArticleBlockPortraitTiny { get; set; }

        [JsonProperty("newspack-article-block-square-tiny")]
        public Uri NewspackArticleBlockSquareTiny { get; set; }

        [JsonProperty("newspack-article-block-uncropped")]
        public Uri NewspackArticleBlockUncropped { get; set; }
    }

    public partial class Author
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("email")]
        public bool Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("nice_name")]
        public string NiceName { get; set; }

        [JsonProperty("URL")]
        public Uri Url { get; set; }

        [JsonProperty("avatar_URL")]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("profile_URL")]
        public Uri ProfileUrl { get; set; }

        [JsonProperty("site_ID")]
        public long SiteId { get; set; }
    }

    public partial class Capabilities
    {
        [JsonProperty("publish_post")]
        public bool PublishPost { get; set; }

        [JsonProperty("delete_post")]
        public bool DeletePost { get; set; }

        [JsonProperty("edit_post")]
        public bool EditPost { get; set; }
    }

    public partial class Categor
    {
        [JsonProperty("Merchant")]
        public Merchant Merchant { get; set; }
    }

    public partial class Merchant
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("post_count")]
        public long PostCount { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public long? Parent { get; set; }

        [JsonProperty("meta")]
        public The8_Meta Meta { get; set; }
    }

    public partial class Discussion
    {
        [JsonProperty("comments_open")]
        public bool CommentsOpen { get; set; }

        [JsonProperty("comment_status")]
        public string CommentStatus { get; set; }

        [JsonProperty("pings_open")]
        public bool PingsOpen { get; set; }

        [JsonProperty("ping_status")]
        public string PingStatus { get; set; }

        [JsonProperty("comment_count")]
        public long CommentCount { get; set; }
    }

    public partial class WordpressBlogPostMeta
    {
        [JsonProperty("links")]
        public FluffyLinks Links { get; set; }
    }

    public partial class FluffyLinks
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("help")]
        public Uri Help { get; set; }

        [JsonProperty("site")]
        public Uri Site { get; set; }

        [JsonProperty("replies")]
        public Uri Replies { get; set; }

        [JsonProperty("likes")]
        public Uri Likes { get; set; }
    }

    public partial class Metadatum
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Value { get; set; }
    }

    public partial class OtherUrLs
    {
    }

    public partial class PostThumbnail
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("URL")]
        public Uri Url { get; set; }

        [JsonProperty("guid")]
        public Uri Guid { get; set; }

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }

    public partial class Tags
    {
        [JsonProperty("Guide")]
        public Merchant Guide { get; set; }
    }

    public partial class Terms
    {
        [JsonProperty("category")]
        public Categor Category { get; set; }

        [JsonProperty("post_tag")]
        public Tags PostTag { get; set; }

        [JsonProperty("post_format")]
        public OtherUrLs PostFormat { get; set; }

        [JsonProperty("mentions")]
        public OtherUrLs Mentions { get; set; }
    }

    public partial class WordpressBlogPost
    {
        public static WordpressBlogPost FromJson(string json) => JsonConvert.DeserializeObject<WordpressBlogPost>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this WordpressBlogPost self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

}
