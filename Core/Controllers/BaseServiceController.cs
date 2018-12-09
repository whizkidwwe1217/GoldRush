using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Security;
using GoldRush.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public abstract class BaseServiceController<TKey, TEntity, TService> : BaseServiceController<TKey, TEntity>
        where TEntity : class, IBaseEntity<TKey>, new()
        where TService : class, IService<TKey, TEntity>
    {
        public new TService Service { get; set; }
        public BaseServiceController(TService service) : base(service)
        {
            Service = service;
        }
    }

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public abstract class BaseServiceController<TKey, TEntity> : ControllerBase, IServiceController<TKey, TEntity>
        where TEntity : class, IBaseEntity<TKey>, new()
    {
        private readonly IService<TKey, TEntity> service;

        public BaseServiceController(IService<TKey, TEntity> service)
            : base()
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IService<TKey, TEntity> Service => service;

        protected IActionResult CreatedAt(string action, TEntity entity)
        {
            return CreatedAtAction(action, new { id = entity.Id }, entity);
        }

        [HttpGet]
        public virtual async Task<ActionResult<List<TEntity>>> List(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await service.ListAsync(cancellationToken);
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> Get(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity = await service.GetAsync(id, cancellationToken);
            if (entity == null)
                return NotFound(entity);
            return entity;
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TEntity entity)
        {
            if (entity == null)
                return BadRequest();
            try
            {
                await service.AddAsync(entity);
                await service.SaveAsync();
                return CreatedAt(nameof(Get), entity);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.InnerException.Message, details = ex.InnerException });
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(TKey id, [FromBody] TEntity entity)
        {
            if (entity == null || !entity.Id.Equals(id))
            {
                return BadRequest();
            }

            try
            {
                await service.UpdateAsync(id, entity);
                await service.SaveAsync();
                if (entity.ConcurrencyStamp != null)
                    HttpContext.Response.Headers.Add("ConcurrencyStamp", Convert.ToBase64String(entity.ConcurrencyStamp));
                if (entity.ConcurrencyTimeStamp != null)
                    HttpContext.Response.Headers.Add("ConcurrencyTimeStamp", entity.ConcurrencyTimeStamp.ToString());
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.InnerException.Message, details = ex.InnerException });
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.InnerException.Message, details = ex.InnerException });
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, details = ex });
            }

            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(TKey id)
        {
            var persistedEntity = await service.GetAsync(id);
            if (persistedEntity == null)
            {
                return NotFound();
            }

            service.Remove(persistedEntity);
            await service.SaveAsync();
            return new NoContentResult();
        }

        [HttpGet]
        [Route("[action]")]
        [ClaimRequirement(CompanyClaimTypes.Permission, "CanSearch")]
        public async Task<ActionResult<SearchResponseData<TEntity>>> SearchQuery(int? currentPage = 1, int? pageSize = 100,
            string filter = "", string sort = "", string fields = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await Service.SearchAsync(currentPage, pageSize, filter, sort, fields, cancellationToken);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("[action]")]
        [ClaimRequirement(CompanyClaimTypes.Permission, "CanSearch")]
        public async Task<ActionResult<SearchResponseData<TEntity>>> Search([FromQuery] SearchParams parameter,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await Service.SearchAsync(parameter, cancellationToken);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}