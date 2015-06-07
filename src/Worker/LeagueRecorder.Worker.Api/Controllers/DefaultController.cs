using System.Net;
using System.Net.Http;
using System.Web.Http;
using Anotar.NLog;
using LeagueRecorder.Worker.Api.Extensions;

namespace LeagueRecorder.Worker.Api.Controllers
{
    public class DefaultController : BaseController
    {
        public HttpResponseMessage Get()
        {
            LogTo.Debug("Request to uri: {0}", this.Request.RequestUri);
            return this.Request.GetMessage(HttpStatusCode.NotFound);
        }
    }
}