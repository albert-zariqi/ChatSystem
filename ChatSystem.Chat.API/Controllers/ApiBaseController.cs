using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Chat.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ApiBaseController : ControllerBase
    {
    }
}
