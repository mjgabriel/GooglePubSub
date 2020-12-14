using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GooglePubSub.Publisher;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PubSubSample.Events.V1;
using WebApi.Contracts.V1;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IPublishMessage _messagePublisher;
        private readonly ILogger<UploadController> _logger;

        public UploadController(
            IPublishMessage messagePublisher,
            ILogger<UploadController> logger)
        {
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> Post(IEnumerable<UploadRequest> files, CancellationToken cancellationToken)
        {
            // files would be posted, etc. message publishing wouldn't be done in the controller
            // just keeping it simple

            foreach (var file in files)
            {
                await _messagePublisher.PublishMessageAsync(
                    new FileUploadCompletedEvent {Url = file.Url},
                    cancellationToken);
            }
            
            return Accepted();
        }
    }
}