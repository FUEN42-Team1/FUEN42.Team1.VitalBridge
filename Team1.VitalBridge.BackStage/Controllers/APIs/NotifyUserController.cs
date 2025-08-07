using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.Service;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyUserController : ControllerBase
    {
        private readonly NotifyUserService _service;

        public NotifyUserController(NotifyUserService service)
        {
            this._service = service;
        }
        [HttpPost("{notifyId}/{userId}")]
        public IActionResult CreateNotifyUser(int notifyId, int userId)
        {
            _service.CreateNotifyUser(notifyId, userId);
            return Ok();
        }
        [HttpDelete("{notifyId}/{userId}")]
        public IActionResult DeleteNotifyUser(int notifyId, int userId)
        {
            _service.DeleteNotifyUser(notifyId, userId);
            return Ok();
        }
    }
}
