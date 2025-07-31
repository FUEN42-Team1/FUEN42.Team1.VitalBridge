using Team1.VitalBridge.BackStage.Models.DataTables;
using Team1.VitalBridge.BackStage.Models.Dto;
using Team1.VitalBridge.BackStage.Models.Service;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly NotifyService _service;

        public NotifyController(NotifyService service)
        {
            this._service = service;
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var notify = _service.GetNotifyById(id);

            return Ok(notify);
        }

        [HttpPost("getByPage")]
        public IActionResult Post([FromBody] DataTableRequest request)
        {
            var result = _service.GetNotifyByPage(request);
            var totalCount = result.Count;

            result = result.Skip(request.Start)
                            .Take(request.Length)
                            .ToList();
            var notifies = result.Select(n => new NotifyTableDto
            {
                id = n.Id,
                title = n.Title,
                notifysUrl = n.NotifysUrl,
                categoriesName = n.Categories?.Name ?? "未分類",
                sendDate = n.SendDate.ToString("yyyy/MM/dd HH:mm"),
                validityDate = n.ValidityDate?.ToString("yyyy/MM/dd HH:mm") ?? "無期限",
                userCount = n.NotifyUsers?.Count() ?? 0
            }).ToList();

            var response = new DataTableResponse<NotifyTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = totalCount,
                Data = notifies
            };

            return Ok(response);
        }

        [HttpDelete]
        public IActionResult Delete([FromForm] int id)
        {
            _service.DeleteNotify(id);
            return Ok();
        }
        public class DataTableResponse<T>
        {
            public int Draw { get; set; }
            public int RecordsTotal { get; set; }
            public int RecordsFiltered { get; set; }
            public List<T> Data { get; set; }
        }
    }
}
