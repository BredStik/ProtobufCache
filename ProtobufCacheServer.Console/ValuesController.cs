using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Http;

namespace ProtobufCacheServer.Console.Controllers
{
    public class ValuesController : ApiController
    {
        private static object _lock = new object();

        // GET api/values
        public IEnumerable<string> Get()
        {
            var username = User.Identity.Name;

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [Route("api/values/{key}")]
        public async Task<HttpResponseMessage> Get(string key)
        {
            var cachedItem = MemoryCache.Default.Get(key) as byte[];

            if(cachedItem == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }


            Stream stream = null;

            if (cachedItem != null)
            {
                stream = new MemoryStream();
                await stream.WriteAsync(cachedItem, 0, cachedItem.Length);
                stream.Position = 0;
            }

            //var content = new PushStreamContent(async (stream, httpContent, transportContext) =>
            //{
            //    await stream.WriteAsync(cachedItem, 0, cachedItem.Length);
            //    stream.Close();
            //});

            var content = new StreamContent(stream);
            return new HttpResponseMessage() { Content = content };
        }

        // POST api/values
        [Route("api/values/{key}")]
        public async Task Post(string key, [FromUri]int duration = 10)
        {
            var stream = await Request.Content.ReadAsStreamAsync();
            
            MemoryCache.Default.Add(key, ReadFully(stream), DateTime.Now.AddMinutes(duration));
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.WriteAsync(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        // PUT api/values/5
        [Route("api/values/{key}")]
        public async Task Put(string key, [FromUri]int duration = 10)
        {
            var stream = await Request.Content.ReadAsStreamAsync();

            lock (_lock)
            {
                if (MemoryCache.Default.Any(x => x.Key.Equals(key)))
                {
                    MemoryCache.Default.Remove(key);
                }
                else
                {
                    return;
                }

                MemoryCache.Default.Add(key, ReadFully(stream), DateTime.Now.AddMinutes(10));
            }
        }

        // DELETE api/values/5
        [Route("api/values/{key}")]
        public void Delete(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        private static Task CompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}
