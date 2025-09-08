using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoBetGoal_BackEnd
{
    public class JsonExceptionMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // 呼叫下一個 Handler (或 Action)
                return await base.SendAsync(request, cancellationToken);
            }
            catch (JsonSerializationException jex)
            {
                // JSON 反序列化失敗 → 回 400 BadRequest
                var response = request.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    errorCode = "VALIDATION_FAILED",
                    message = jex.Message,
                    details = new[] { new { field = "", errors = new[] { jex.Message } } }
                });

                return response;
            }
        }
    }
}