﻿using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GitHubFreshdeskIntegration.WebAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubFreshdeskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GitHubFreshdeskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncGitHubUserToFreshdesk([FromQuery] SyncGitHubUserToFreshdeskCommand command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok("User synchronized successfully.");
        }
    }
}
