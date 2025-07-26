using IziHardGames.IziCoordinatorP2P;
using IziHardGames.IziP2P.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IziHardGames.IziCoordinatorP2P.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> NotifyNewPeer([FromBody] PeerDto peer)
        {
            await Task.CompletedTask;
            return Ok(peer);
        }
    }
}
