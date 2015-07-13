using protobuf.Poco;
using protobuf.Poco.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProtobufCacheClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var user = new User();
            user.Id = 1;
            user.Email = "good@domain.com";
            user.Image = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            user.CreatedAt = DateTime.Now;
            user.Tags = new List<string>();
            user.Tags.Add("soccer");

            var serializer = new Serializer();

            var content = new PushStreamContent((stream, httpContent, transportContext) =>
            {
                serializer.SerializeToStream(user, stream);
                stream.Close();
            });

            var client = new HttpClient();

            var response = client.PostAsync("http://localhost:19012/api/values/myItem?replace=false", content).ConfigureAwait(false).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var nextResponse = client.GetAsync("http://localhost:19012/api/values/myItem").ConfigureAwait(false).GetAwaiter().GetResult();

            var responseStream = nextResponse.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            var otherUser = serializer.DeserializeFromStream<User>(responseStream);


        }
    }
}
