using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.DataTables;
using Team1.VitalBridge.BackStage.Models.Dto;
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

        [HttpPost("getUsersByPage/{NotifyId}")]
        public IActionResult Post([FromBody] DataTableRequest request,int NotifyId)
        {
            var result = _service.GetNotifyUsersByPage(request, NotifyId);
            var totalCount = result.Count;

            result = result.Skip(request.Start)
                            .Take(request.Length)
                            .ToList();
            var users = result.Select(n => new NotifyUsersTableDTO
            {
                Id = NotifyId,
                UserId = n.UserId,
                UserName = n.UserName,
                UserEmail = n.UserEmail,
                UserRoles = n.UserRoles
            }).ToList();

            var response = new DataTableResponse<NotifyUsersTableDTO>
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = totalCount,
                Data = users
            };

            return Ok(response);
        }
    }
}
